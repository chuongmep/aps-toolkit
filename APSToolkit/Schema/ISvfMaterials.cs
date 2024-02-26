namespace APSToolkit.Schema;

public struct ISvfMaterials
{
    public string name { get; set; }
    public string version { get; set; }
    public Dictionary<string, string> scene { get; set; }
    public Dictionary<string, ISvfMaterialGroup> materials { get; set; }
}