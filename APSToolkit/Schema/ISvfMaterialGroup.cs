namespace APSToolkit.Schema;

public struct ISvfMaterialGroup
{
    public int version { get; set; }
    public string[] userassets { get; set; }
    public Dictionary<string, ISvfMaterial> materials { get; set; }
}