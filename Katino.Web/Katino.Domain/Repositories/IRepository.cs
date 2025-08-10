namespace Katino.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> Get(int id);
    Task<IEnumerable<TEntity>> GetAll();
    Task<TEntity> Insert(TEntity entity);
    void Delete(TEntity entity);
    Task DeleteById(int id);
    Task Update(TEntity entity);
    Task Save();
}