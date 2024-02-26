namespace APSToolkit.Schema;

/// <summary>
/// Lightweight data structure pointing to a mesh in a specific packfile and entry.
/// Contains additional information about the type of mesh and its primitive count.
/// </summary>
public struct ISvfGeometryMetadata
{
    public byte fragType { get; set; }
    public ushort primCount { get; set; }
    public int packID { get; set; }
    public int entityID { get; set; }
    public int topoID { get; set; }
}