using System;

namespace El_Shaib.Interfaces;

public interface IGenericService<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}
