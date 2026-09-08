using System;
using El_Shaib.Interfaces;
using El_Shaib.Models;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class GenericService<T> : IGenericService<T> where T : class
{
    protected readonly AppDbContext context;
    protected readonly DbSet<T> entity;


    public GenericService(AppDbContext _context)
    {
        this.context = _context;
        this.entity = context.Set<T>();
    }

    public virtual void Add(T entity)
    {
        this.entity.Add(entity);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        var Obj = GetById(id);
        if (Obj != null)
        {
            entity.Remove(Obj);
            context.SaveChanges();
        }
        return;
    }


    public virtual async Task<List<T>> GetAllAsync()
    {
        return entity.ToList();
    }

    public virtual T? GetById(int id)
    {
        return entity.Find(id);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return entity.Find(id);
    }

    public void Update(T entity)
    {
        this.entity.Update(entity);
        context.SaveChanges();
    }
}