namespace APSToolkit.Schema;

/// <summary>
/// Point cloud data.
/// </summary>
public struct ISvfPoints : IMeshPack
{
    public bool isPoints { get; set; }
    public int vcount { get; set; }
    public float[] vertices { get; set; }
    public float[] colors { get; set; }
    public float pointSize { get; set; }
}