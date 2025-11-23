using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

// TODO create table later
public class NpContactPerson
{
    public Guid Id { get; set; }

    [Required]
    public string Ref { get; set; }

    [Required]
    public string CounterpartyRef { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string MiddleName { get; set; }

    [Required]
    public string Phones { get; set; }
}
