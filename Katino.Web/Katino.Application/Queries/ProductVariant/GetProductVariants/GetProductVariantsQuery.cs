using Katino.Application.DTOs.ProductVariant;
using MediatR;

namespace Katino.Application.Queries.ProductVariantN.GetProductVariants;

public class GetProductVariantsQuery : IRequest<GetProductVariantDto>
{
    public string ProductName { get; set; }
    public Guid? CategoryId { get; set; }
    public ProductStatusDto? ProductStatus { get; set; }
    public ProductStatusDto[] ProductStatusesToExclude { get; set; }
    public bool? GetLastAddedProductVariant { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
