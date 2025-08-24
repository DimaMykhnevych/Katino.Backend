namespace Katino.Application.DTOs.ProductVariant;

public class GetProductVariantDto
{
    public IEnumerable<ProductVariantDto> ProductVariants { get; set; }
    public int ResultsAmount { get; set; }
}

