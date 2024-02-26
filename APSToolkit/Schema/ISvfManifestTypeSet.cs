using Newtonsoft.Json;

namespace APSToolkit.Schema;

/// <summary>
/// Collection of type definitions.
/// </summary>
public struct ISvfManifestTypeSet
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("types")]
    public List<ISvfManifestType> types { get; set; }
}