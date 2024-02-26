namespace APSToolkit.Schema;

/// <summary>
///  Interface to group ISvfMesh, ISvfLines and ISvfPoint
/// </summary>
public interface IMeshPack { }

/// <summary>
/// Triangular mesh data, including indices, vertices, optional normals and UVs.
/// </summary>
public struct ISvfMesh : IMeshPack
{
    public int vcount { get; set; }
    public int tcount { get; set; }
    public int uvcount { get; set; }
    public int attrs { get; set; }
    public int flags { get; set; }
    public string comment { get; set; }
    public List<ISvfUVMap> uvmaps { get; set; }
    public uint[] indices { get; set; }
    public float[] vertices { get; set; }
    public float[] normals { get; set; }
    public float[] colors { get; set; }
    public System.Numerics.Vector3 min { get; set; }
    public System.Numerics.Vector3 max { get; set; }
}
