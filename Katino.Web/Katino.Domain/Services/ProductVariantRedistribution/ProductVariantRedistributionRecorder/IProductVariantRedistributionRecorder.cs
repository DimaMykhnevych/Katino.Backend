using Katino.Domain.Models;

namespace Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;

public interface IProductVariantRedistributionRecorder
{
    Task RecordAsync(ProductVariantRedistributionEvent redistributionEvent);
}
