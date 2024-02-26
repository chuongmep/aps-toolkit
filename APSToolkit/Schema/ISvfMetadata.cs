using Newtonsoft.Json;

namespace APSToolkit.Schema;


/// <summary>
/// Additional metadata for SVF such as the definition of "up" vector,
/// default background, etc.
/// </summary>
public struct ISvfMetadata
{
    [JsonProperty("version")]
    public string version { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, object> metadata { get; set; }
}