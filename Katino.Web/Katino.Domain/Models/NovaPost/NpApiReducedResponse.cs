namespace Katino.Domain.Models.NovaPost;

public class NpApiReducedResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; }
    public List<string> Errors { get; set; }
}
