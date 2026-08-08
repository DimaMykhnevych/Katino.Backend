namespace Katino.Store.Application.DTOs.Products;

public class ProductCardPhotoDto
{
    public Guid Id { get; set; }
    public string PhotoUrl { get; set; }
    public string AltText { get; set; }
    public int DisplayOrder { get; set; }
}
