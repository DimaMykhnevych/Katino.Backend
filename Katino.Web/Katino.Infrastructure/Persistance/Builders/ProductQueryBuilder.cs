using Katino.Domain.Builders;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Builders;

public class ProductQueryBuilder : IProductQueryBuilder
{
    private const int DefaultPageSize = 20;
    private readonly KatinoDbContext _dbContext;
    private IQueryable<Product> _query;

    public ProductQueryBuilder(KatinoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IProductQueryBuilder SetBaseQuery()
    {
        _query = _dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Variants.Any(v => v.DeletedAt == null && v.Status != ProductStatus.Discontinued))
            .Include(p => p.Variants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.Photos)
            .Include(p => p.Variants.Where(v => v.DeletedAt == null))
                .ThenInclude(v => v.Color)
            .OrderByDescending(p => p.CreatedAt);

        return this;
    }

    public IProductQueryBuilder ApplyCategoryFilter(IEnumerable<Guid> categoryIds)
    {
        EnsureQuery();

        var ids = categoryIds?.ToList();
        if (ids is null or [])
        {
            return this;
        }

        _query = _query.Where(p => ids.Contains(p.CategoryId));
        return this;
    }

    public IProductQueryBuilder ApplySearch(string search)
    {
        EnsureQuery();

        if (string.IsNullOrWhiteSpace(search))
        {
            return this;
        }

        var term = search.Trim().ToLower();
        _query = _query.Where(p =>
            p.Name.ToLower().Contains(term) ||
            p.Variants.Any(v => v.DeletedAt == null && v.Article.ToLower().Contains(term)));

        return this;
    }

    public IProductQueryBuilder ApplyPaging(int page, int pageSize)
    {
        EnsureQuery();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;

        _query = _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return this;
    }

    public IQueryable<Product> Build()
    {
        IQueryable<Product> result = _query;
        _query = null;
        return result;
    }

    private void EnsureQuery()
    {
        _query ??= _dbContext.Products.AsQueryable();
    }
}
