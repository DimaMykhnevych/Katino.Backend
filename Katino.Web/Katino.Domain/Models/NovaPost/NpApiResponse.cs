namespace Katino.Domain.Models.NovaPost;

public class NpApiResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
    public object Info { get; set; }
    public List<string> ErrorCodes { get; set; }
    public List<string> WarningCodes { get; set; }
}
