using El_Shaib.Interfaces;
using El_Shaib.Models;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class GenericService<T> : IGenericService<T> where T : class
{
    protected readonly AppDbContext context;
    protected readonly DbSet<T> entity;

    public GenericService(AppDbContext context)
    {
        this.context = context;
        this.entity = context.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await entity.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await entity.FindAsync(id);
    }
}