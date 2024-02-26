using Newtonsoft.Json;

namespace APSToolkit.Schema;


/// <summary>
/// Description of a specific asset referenced by or embedded in an SVF,
/// including the URI, compressed and uncompressed size, type of the asset itself,
/// and types of all entities inside the asset.
/// </summary>
public struct ISvfManifestAsset
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("type")]
    public AssetType type { get; set; }

    [JsonProperty("typeset")]
    public string typeset { get; set; }

    [JsonProperty("URI")]
    public string URI { get; set; }

    [JsonProperty("size")]
    public int size { get; set; }

    [JsonProperty("usize")]
    public int usize { get; set; }
}