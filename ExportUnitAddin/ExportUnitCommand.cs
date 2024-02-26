using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace ExportUnitAddin;

[Transaction(TransactionMode.Manual)]
public class ExportUnitCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        IList<ForgeTypeId> forgeTypeIds = UnitUtils.GetAllUnits();
        List<UnitsData> UnitDict = new List<UnitsData>();
        foreach (var forgeTypeId in forgeTypeIds)
        {
            // get label symbol
            var symbol = FormatOptions
                .GetValidSymbols(forgeTypeId).FirstOrDefault(x => x.Empty() == false);
            if (symbol != null)
            {
                string labelForSymbol = LabelUtils.GetLabelForSymbol(symbol);
                // pattent to split version and string :
                // e.g : autodesk.unit.unit:britishThermalUnits-1.0.1 => autodesk.unit.unit:britishThermalUnits-1.0.1  and 1.0.1
                var pattern = @"(.*)(-)(.*)";
                var match = Regex.Match(forgeTypeId.TypeId, pattern);
                UnitsData unitsData = new UnitsData();
                unitsData.UnitLabel = labelForSymbol;
                unitsData.TypeId = match.Groups[1].Value;
                unitsData.Version = match.Groups[3].Value;
                UnitDict.Add(unitsData);
            }
        }
        // save to txt and open
        string path = Path.Combine(Path.GetTempPath(), "units.json");
        // save list object to json
        string json = JsonConvert.SerializeObject(UnitDict,Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(path, json, Encoding.UTF8);
        Process.Start(path);
        return Result.Succeeded;
    }
}
public class UnitsData
{
    public string TypeId { get; set; }
    public string UnitLabel { get; set; }
    public string Version { get; set; }
}