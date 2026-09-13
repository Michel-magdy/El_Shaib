namespace El_Shaib.Interfaces;

public interface IGenericService<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
}
