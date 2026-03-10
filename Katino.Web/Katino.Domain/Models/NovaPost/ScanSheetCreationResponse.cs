namespace Katino.Domain.Models.NovaPost;

public class ScanSheetCreationResponse
{
    public string Ref { get; set; }
    public string Number { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public List<object> Errors { get; set; }
    public List<object> Success { get; set; }
}
