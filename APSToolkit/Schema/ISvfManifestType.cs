using Newtonsoft.Json;

namespace APSToolkit.Schema;

/// <summary>
/// Single type definition
/// </summary>
public struct ISvfManifestType
{
    [JsonProperty("class")]
    public string typeClass { get; set; }

    [JsonProperty("type")]
    public string type { get; set; }

    [JsonProperty("version")]
    public int version { get; set; }
}