using Newtonsoft.Json;

namespace Katino.Domain.Models.NovaPost;

public class NpApiRequest<T>
{
    [JsonProperty("apiKey")]
    public string ApiKey { get; set; }

    [JsonProperty("modelName")]
    public string ModelName { get; set; }

    [JsonProperty("calledMethod")]
    public string CalledMethod { get; set; }

    [JsonProperty("methodProperties")]
    public T MethodProperties { get; set; }
}
