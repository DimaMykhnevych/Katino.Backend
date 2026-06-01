namespace Katino.Domain.Entities;

public class Collection
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ProductCollection> ProductCollections { get; set; } = [];
}
