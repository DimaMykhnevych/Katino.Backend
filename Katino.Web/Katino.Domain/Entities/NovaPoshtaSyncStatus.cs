using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Entities;

public class NovaPoshtaSyncStatus
{
    public Guid Id { get; set; }
    public SyncType SyncType { get; set; }
    public SyncStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ApiRequestedRecords { get; set; }
    public int? DbInsertedRecords { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TriggeredBy { get; set; }
}
