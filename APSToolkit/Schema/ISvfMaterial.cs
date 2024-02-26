namespace APSToolkit.Schema;

public struct ISvfMaterial
{
    public float[] diffuse { get; set; }
    public float[] specular { get; set; }
    public float[] ambient { get; set; }
    public float[] emissive { get; set; }
    public float? glossiness { get; set; }
    public float? reflectivity { get; set; }
    public float? opacity { get; set; }
    public bool? metal { get; set; }

    public ISvfMaterialMaps? maps { get; set; }

    //second part of the material defined in the material class
    public string tag { get; set; }
    public string proteinType { get; set; }
    public string definition { get; set; }
    public bool transparent { get; set; }
    public string[] keywords { get; set; }
    public string[] categories { get; set; }
    public ISvfMaterialProperties properties { get; set; }
    public Dictionary<string, StringConnections> textures { get; set; }
}