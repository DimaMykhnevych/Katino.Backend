using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

// TODO create table later
public class NpCity
{
    public Guid Id { get; set; }

    [Required]
    public string Present { get; set; }

    [Required]
    public string DeliveryCity { get; set; }
}
