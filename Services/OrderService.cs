using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace El_Shaib.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public OrderService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<DeliveryArea>> GetDeliveryAreasAsync()
    {
        return await _cache.GetOrCreateAsync("active_delivery_areas", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            var areas = await _context.DeliveryAreas
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            // If no areas configured yet in DB, provide default seeded fallback areas for Luxor
            if (areas.Count == 0)
            {
                areas = new List<DeliveryArea>
                {
                    new() { Id = 1, Name = "مدينة الأقصر - وسط البلد", City = "الأقصر", DeliveryFee = 20m, EstimatedTime = "خلال 3 ساعات", IsActive = true },
                    new() { Id = 2, Name = "الكرنك", City = "الأقصر", DeliveryFee = 25m, EstimatedTime = "خلال 4 ساعات", IsActive = true },
                    new() { Id = 3, Name = "العوامية والعشى", City = "الأقصر", DeliveryFee = 25m, EstimatedTime = "خلال 4 ساعات", IsActive = true },
                    new() { Id = 4, Name = "البياضية", City = "الأقصر", DeliveryFee = 30m, EstimatedTime = "نفس اليوم", IsActive = true },
                    new() { Id = 5, Name = "القرنة - البر الغربي", City = "الأقصر", DeliveryFee = 35m, EstimatedTime = "نفس اليوم", IsActive = true },
                    new() { Id = 6, Name = "أرمنت", City = "الأقصر", DeliveryFee = 40m, EstimatedTime = "خلال 24 ساعة", IsActive = true }
                };
            }

            return areas;
        }) ?? new List<DeliveryArea>();
    }

    public async Task<DeliveryArea?> GetDeliveryAreaByIdAsync(int id)
    {
        var area = await _context.DeliveryAreas.FindAsync(id);
        if (area != null) return area;

        var defaults = await GetDeliveryAreasAsync();
        return defaults.FirstOrDefault(a => a.Id == id);
    }

    public async Task<Order> CreateOrderAsync(CheckoutViewModel model, CartViewModel cart, int? customerId = null)
    {
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("لا يمكن إتمام الطلب لأن سلة التسوق فارغة.");
        }

        // Resolve or create Customer record
        int resolvedCustomerId;
        if (customerId.HasValue)
        {
            resolvedCustomerId = customerId.Value;
        }
        else
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Phone == model.Phone || (!string.IsNullOrEmpty(model.Email) && c.Email == model.Email));

            if (existingCustomer != null)
            {
                resolvedCustomerId = existingCustomer.Id;
            }
            else
            {
                var newCustomer = new Customer
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Email = string.IsNullOrWhiteSpace(model.Email) ? $"{Guid.NewGuid():N}@guest.elshaib.com" : model.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
                resolvedCustomerId = newCustomer.Id;
            }
        }

        // Resolve delivery fee
        var deliveryArea = await GetDeliveryAreaByIdAsync(model.DeliveryAreaId);
        var subtotal = cart.SubTotal;
        var deliveryFee = subtotal >= cart.FreeDeliveryThreshold ? 0m : (deliveryArea?.DeliveryFee ?? 25m);
        var total = subtotal + deliveryFee;

        var fullDeliveryAddress = $"{model.City} - {model.District} - شارع {model.Street}";
        if (deliveryArea != null)
        {
            fullDeliveryAddress = $"[{deliveryArea.Name}] " + fullDeliveryAddress;
        }

        // Unique order number: SHP-YYYYMMDD-XXXX
        var orderNumber = $"SHP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = resolvedCustomerId,
            Status = OrderStatus.Pending,
            PaymentMethod = model.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            SubTotal = subtotal,
            DeliveryFee = deliveryFee,
            Discount = 0m,
            Total = total,
            DeliveryAddress = fullDeliveryAddress,
            DeliveryPhone = model.Phone,
            Notes = model.AdditionalNotes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                TotalPrice = item.LineTotal
            });

            // Adjust stock
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null && product.StockQuantity > 0)
            {
                product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim());
    }

    public async Task<OrderConfirmationViewModel?> GetOrderConfirmationAsync(string orderNumber)
    {
        var order = await GetOrderByNumberAsync(orderNumber);
        if (order == null) return null;

        return new OrderConfirmationViewModel
        {
            OrderNumber = order.OrderNumber,
            CustomerName = order.Customer?.FullName ?? "عميل كريم",
            Phone = order.DeliveryPhone ?? order.Customer?.Phone ?? "",
            DeliveryAddress = order.DeliveryAddress ?? "",
            EstimatedTime = "خلال ٢٤ ساعة",
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            SubTotal = order.SubTotal,
            DeliveryFee = order.DeliveryFee,
            Discount = order.Discount,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemViewModel
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}

