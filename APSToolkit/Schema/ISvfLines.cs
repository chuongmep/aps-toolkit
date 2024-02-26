namespace APSToolkit.Schema;

/// <summary>
/// Line segment data.
/// </summary>
public struct ISvfLines : IMeshPack
{
    public bool isLines { get; set; }
    public int vcount { get; set; }
    public int lcount { get; set; }
    public float[] vertices { get; set; }
    public ushort[] indices { get; set; }
    public float[] colors { get; set; }
    public float lineWidth { get; set; }
}
