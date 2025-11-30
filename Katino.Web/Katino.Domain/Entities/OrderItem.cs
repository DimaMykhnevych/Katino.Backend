using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }

    [Required]
    public bool IsCustomTailoring { get; set; }
    public string Comment { get; set; }
    public int Quantity { get; set; }

    public Guid ProductVariantId { get; set; }
    public Guid OrderId { get; set; }

    // Navigation properties
    public ProductVariant ProductVariant { get; set; }
    public Order Order { get; set; }
}
