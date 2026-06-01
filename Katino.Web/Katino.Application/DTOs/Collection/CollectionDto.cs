namespace Katino.Application.DTOs.Collection;

public class CollectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ProductInCollectionDto> Products { get; set; }
}
