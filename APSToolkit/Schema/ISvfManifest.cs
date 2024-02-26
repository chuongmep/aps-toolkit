using Newtonsoft.Json;

namespace APSToolkit.Schema;


/// <summary>
/// Top-level manifest containing URIs and types of all assets
/// referenced by or embedded in a specific SVF file.
/// The URIs are typically relative to the SVF file itself.
/// </summary>
public struct ISvfManifest
{

    [JsonProperty("name")]
    public string name { get; set; }

    [JsonProperty("manifestversion")]
    public int manifestversion { get; set; }

    [JsonProperty("toolkitversion")]
    public string toolkitversion { get; set; }

    [JsonProperty("assets")]
    public List<ISvfManifestAsset> assets { get; set; }

    [JsonProperty("typesets")]
    public List<ISvfManifestTypeSet> typesets { get; set; }

}
