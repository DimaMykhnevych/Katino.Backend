namespace Katino.Application.DTOs.NovaPost;

public class SyncRecordDto
{
    public Guid Id { get; set; }
    public SyncTypeDto SyncType { get; set; }
    public SyncStatusDto Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? ApiRequestedRecords { get; set; }
    public int? DbInsertedRecords { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TriggeredBy { get; set; }
    public string TriggeredByUsername { get; set; }
}
