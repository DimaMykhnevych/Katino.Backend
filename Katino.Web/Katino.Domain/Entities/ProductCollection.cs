namespace Katino.Domain.Entities;

public class ProductCollection
{
    public Guid CollectionId { get; set; }
    public Guid ProductId { get; set; }

    public Collection Collection { get; set; }
    public Product Product { get; set; }
}
