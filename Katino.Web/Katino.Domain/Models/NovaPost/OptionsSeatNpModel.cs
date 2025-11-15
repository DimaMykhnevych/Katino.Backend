using Newtonsoft.Json;

namespace Katino.Domain.Models.NovaPost;

public class OptionsSeatNpModel
{
    [JsonProperty("volumetricWidth")]
    public string VolumetricWidth { get; set; }

    [JsonProperty("volumetricLength")]
    public string VolumetricLength { get; set; }

    [JsonProperty("volumetricHeight")]
    public string VolumetricHeight { get; set; }

    [JsonProperty("weight")]
    public string Weight { get; set; }
}
