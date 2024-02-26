namespace APSToolkit.Schema;

public struct ISvfMaterialProperties
{
    public Dictionary<string, int> integers { get; set; }
    public Dictionary<string, bool> booleans { get; set; }
    public Dictionary<string, StringValues> strings { get; set; }
    public Dictionary<string, StringValues> uris { get; set; }
    public Dictionary<string, UnitsAndNumberValues> scalars { get; set; }
    public Dictionary<string, RGBValuesAndStringConnections> colors { get; set; }
    public Dictionary<string, IntValues> choicelists { get; set; }
    public Dictionary<string, IntValues> uuids { get; set; }
    public string references { get; set; } //type TODO

}