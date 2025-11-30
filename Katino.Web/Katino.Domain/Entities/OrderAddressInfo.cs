using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class OrderAddressInfo
{
    public Guid Id { get; set; }
    public string RecipientAddressNote { get; set; }

    [Required]
    public string RecipientCity { get; set; }

    [Required]
    public string RecipientAddressName { get; set; }

    [Required]
    public string RecipientHouse { get; set; }

    [Required]
    public string RecipientFlat { get; set; }

    public Order Order { get; set; }
}
