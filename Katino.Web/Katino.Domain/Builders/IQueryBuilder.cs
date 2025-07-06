namespace Katino.Domain.Builders;

public interface IQueryBuilder<TEntity>
{
    IQueryable<TEntity> Build();
}