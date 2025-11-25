using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class NpCity
{
    public Guid Id { get; set; }

    [Required]
    public string Present { get; set; }

    [Required]
    public string DeliveryCity { get; set; }
}
