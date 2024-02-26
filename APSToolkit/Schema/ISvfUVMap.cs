namespace APSToolkit.Schema;

/// <summary>
/// Single UV channel. IMesh can have more of these.
/// </summary>
public struct ISvfUVMap
{
    public string name { get; set; }
    public string file { get; set; }
    public float[] uvs { get; set; }
}