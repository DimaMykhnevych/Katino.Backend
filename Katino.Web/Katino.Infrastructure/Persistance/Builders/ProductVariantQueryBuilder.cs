using Katino.Domain.Builders;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Builders;

public class ProductVariantQueryBuilder : IProductVariantQueryBuilder
{
    private const int DefaultPageSize = 20;
    private readonly KatinoDbContext _dbContext;
    private IQueryable<ProductVariant> _query;

    public ProductVariantQueryBuilder(KatinoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<ProductVariant> Build()
    {
        IQueryable<ProductVariant> result = _query;
        _query = null;
        return result;
    }

    public IProductVariantQueryBuilder SetBaseQuery()
    {
        _query = _dbContext.ProductVariants
            .AsNoTracking()
            .AsSplitQuery()
            .Include(pv => pv.Product)
                .ThenInclude(p => p.Category)
            .Include(pv => pv.Size)
            .Include(pv => pv.Color)
            .Include(pv => pv.Photos)
            .Include(pv => pv.Sewers)
                .ThenInclude(pv => pv.Sewer)
            .Include(pv => pv.Measurements)
                .ThenInclude(pvm => pvm.MeasurementType)
            .OrderByDescending(pv => pv.CreatedAt);

        return this;
    }

    public IProductVariantQueryBuilder ApplyNameFilter(string? productName)
    {
        EnsureQuery();

        if (string.IsNullOrWhiteSpace(productName))
        {
            return this;
        }

        var term = productName.Trim().ToLower();
        _query = _query.Where(pv => pv.Product.Name.ToLower().Contains(term));

        return this;
    }

    public IProductVariantQueryBuilder ApplyCategoryFilter(Guid? categoryId)
    {
        EnsureQuery();

        if (categoryId == null)
        {
            return this;
        }

        _query = _query.Where(pv => pv.Product.Category.Id == categoryId);

        return this;
    }

    public IProductVariantQueryBuilder ApplyStatusFilter(ProductStatus? productStatus)
    {
        EnsureQuery();

        if (productStatus == null)
        {
            return this;
        }

        _query = _query.Where(pv => pv.Status == productStatus);

        return this;
    }

    public IProductVariantQueryBuilder ApplyPaging(int page, int pageSize)
    {
        EnsureQuery();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;

        _query = _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return this;
    }

    private void EnsureQuery()
    {
        _query ??= _dbContext.ProductVariants.AsQueryable();
    }
}