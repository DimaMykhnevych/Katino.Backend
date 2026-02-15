using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Entities;

public class NovaPoshtaSyncStatus
{
    public Guid Id { get; set; }
    public SyncType SyncType { get; set; }
    public SyncStatus Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? ApiRequestedRecords { get; set; }
    public int? DbInsertedRecords { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TriggeredBy { get; set; }
}
