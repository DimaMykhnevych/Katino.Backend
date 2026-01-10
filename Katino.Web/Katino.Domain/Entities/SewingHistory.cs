using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class SewingHistory
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }

    [Required]
    public Guid SewedBy { get; set; }

    [Required]
    public int SewedQuantity { get; set; }

    [Required]
    public bool IsCustomTailoring { get; set; }

    [Required]
    public DateTime SewedDate { get; set; }

    public ProductVariant ProductVariant { get; set; }
    public AppUser SewedByUser { get; set; }
}
