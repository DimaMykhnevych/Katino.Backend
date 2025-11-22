namespace Katino.Application.DTOs.NovaPost;

public class GetSyncRecordDto
{
    public IEnumerable<SyncRecordDto> SyncRecords { get; set; }
    public int ResultsAmount { get; set; }
}
