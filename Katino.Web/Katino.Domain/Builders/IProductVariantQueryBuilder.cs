using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Builders;

public interface IProductVariantQueryBuilder : IQueryBuilder<ProductVariant>
{
    IProductVariantQueryBuilder SetBaseQuery();
    IProductVariantQueryBuilder ApplyNameFilter(string productName);
    IProductVariantQueryBuilder ApplyCategoryFilter(Guid? categoryId);
    IProductVariantQueryBuilder ApplyStatusFilter(ProductStatus? productStatus);
    IProductVariantQueryBuilder ApplyExcludedStatusesFilter(ProductStatus[] productStatusesToExclude);
    IProductVariantQueryBuilder ApplyPaging(int page, int pageSize);
}
