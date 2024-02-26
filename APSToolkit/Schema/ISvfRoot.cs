namespace APSToolkit.Schema;

/// <summary>
/// Parsed content of an actual *.svf file.
/// </summary>
public struct ISvfRoot
{
    public ISvfManifest manifest { get; set; }
    public ISvfMetadata metadata { get; set; }
    public Dictionary<string, byte[]> embedded { get; set; }
}
