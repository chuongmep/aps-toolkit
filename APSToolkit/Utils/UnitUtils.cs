// Copyright (c) chuongmep.com. All rights reserved

using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace APSToolkit.Utils;

public static class UnitUtils
{
    /// <summary>
    ///  Get All Units from units.json
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static List<UnitsData>? GetAllUnits()
    {
        // read units.json from resources
        Assembly? assembly = Assembly.GetExecutingAssembly();
        string resourceName = "APSToolkit.Resources.units.json";
        string[] resourceNames = assembly.GetManifestResourceNames();
        if(resourceNames.Length==0) throw new Exception("Resource not found!");
        using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                return new List<UnitsData>();
            }
            // Read the JSON data from the stream
            using (StreamReader reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                // deaserialize json to list object
                List<UnitsData>? unitsDataList = JsonConvert.DeserializeObject<List<UnitsData>>(json);
                return unitsDataList;
            }
        }
    }
    public static Dictionary<string,string> GetAllDictUnits()
    {
        List<UnitsData>? unitsDataList = GetAllUnits();
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var unitsData in unitsDataList)
        {
            dict.Add(unitsData.TypeId, unitsData.UnitLabel);
        }
        return dict;
    }

    public static UnitsData? ParseUnitsData(string typeId)
    {
        if (string.IsNullOrEmpty(typeId)) return null;
        // e.g : autodesk.unit.unit:britishThermalUnits-1.0.1 => autodesk.unit.unit:britishThermalUnits-1.0.1  and 1.0.1
        var pattern = @"(.*)(-)(.*)";
        var match = Regex.Match(typeId, pattern);
        UnitsData unitsData = new UnitsData();
        unitsData.TypeId = match.Groups[1].Value;
        unitsData.Version = match.Groups[3].Value;
        return unitsData;
    }
    /// <summary>
    /// Get Label Symbol unit
    /// e.g :autodesk.unit.unit:millimeters-1.0.1 => mm
    /// </summary>
    /// <param name="forgeTypeId"></param>
    /// <returns></returns>
    public static string GetLabelForSymbol(string forgeTypeId)
    {
        List<UnitsData>? units = GetAllUnits();
        var pattern = @"(.*)(-)(.*)";
        var match = Regex.Match(forgeTypeId, pattern);
        string typeId = match.Groups[1].Value;
        var label = units?.FirstOrDefault(x => x.TypeId == typeId)?.UnitLabel;
        return label ?? string.Empty;
    }
}

public class UnitsData
{
    public string TypeId { get; set; }
    public string UnitLabel { get; set; }
    public string Version { get; set; }
}