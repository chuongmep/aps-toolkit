using APSToolkit.Database;

namespace APSToolkit.Schema;

public struct ISvfContent
{
    public ISvfMetadata metadata { get; set; }
    public ISvfFragment[] fragments { get; set; }
    public ISvfGeometryMetadata[] geometries { get; set; }
    public IMeshPack[][] meshpacks { get; set; }
    public ISvfMaterial?[] materials { get; set; }
    public PropDbReader properties { get; set; }
    public Dictionary<string, byte[]> images { get; set; }
}
