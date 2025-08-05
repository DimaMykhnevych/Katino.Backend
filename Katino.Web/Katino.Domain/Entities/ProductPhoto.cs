namespace Katino.Domain.Entities;

public class ProductPhoto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string PhotoUrl { get; set; }
    public string AltText { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; }
}