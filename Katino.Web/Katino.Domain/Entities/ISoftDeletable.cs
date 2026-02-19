namespace Katino.Domain.Entities;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}
