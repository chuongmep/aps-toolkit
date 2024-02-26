namespace APSToolkit.Schema;

public struct ISvfMaterialMaps
{
    public ISvfMaterialMap? diffuse { get; set; }
    public ISvfMaterialMap? specular { get; set; }
    public ISvfMaterialMap? normal { get; set; }
    public ISvfMaterialMap? bump { get; set; }
    public ISvfMaterialMap? alpha { get; set; }
}
