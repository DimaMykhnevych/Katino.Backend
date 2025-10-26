namespace Katino.Application.DTOs.ProductPhoto;

public class ProductPhotoDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string PhotoUrl { get; set; }
    public string AltText { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }
}
