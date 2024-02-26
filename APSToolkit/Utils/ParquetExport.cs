// Copyright (c) chuongmep.com. All rights reserved

using System.Text.RegularExpressions;
using APSToolkit.BIM360;
using ChoETL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace APSToolkit.Utils;

public class ParquetExport
{
    Dictionary<string, BIMField> fields { get; set; }
    private BIMObject[] Objects { get; set; }


    /// <summary>
    /// Init RawData with fields and objects
    /// </summary>
    /// <param name="bimFields"></param>
    /// <param name="objects"></param>
    public  ParquetExport(Dictionary<string, BIMField> bimFields,BIMObject?[] objects)
    {
        this.fields = bimFields;
        this.Objects = objects;
    }
    public void WriteToParquet(string filepath)
    {
        ChoParquetRecordConfiguration choParquetRecordConfiguration = new ChoParquetRecordConfiguration();
        choParquetRecordConfiguration.CompressionMethod = Parquet.CompressionMethod.Snappy;
        using (ChoParquetWriter parser = new ChoParquetWriter(filepath, choParquetRecordConfiguration))
        {
            for (int i = 0; i < Objects.Length; i++)
            {
                Metadata metadata = new Metadata();
                SetCategory(metadata,Objects[i]);
                foreach (KeyValuePair<string,object> propertyProp in Objects[i].props)
                {
                    BIMField field = fields[propertyProp.Key];
                    if (field.category.ToString() =="__name__")
                    {
                        string elementId = GetElementId(propertyProp.Value.ToString()??string.Empty);
                        WriteObjectElementId(parser,Objects[i],elementId);
                    }
                    string pattern = @"^__\w+__$";
                    if (Regex.IsMatch(field.category, pattern)) continue;
                    metadata.Guid = Objects[i].externalId ?? string.Empty;
                    metadata.ParameterGroup = field.category ??string.Empty;
                    metadata.ParameterName = field.name ?? string.Empty;
                    metadata.DataType = field.type ?? string.Empty;
                    metadata.DataTypeContext = field.uom ?? string.Empty;
                    metadata.ParameterValue = propertyProp.Value.ToString()??string.Empty;
                    parser.Write(metadata);
                }
                WriteObjectLocation(parser,Objects[i]);
            }
            parser.Close();
        }

    }

    private void SetCategory(Metadata metadata,BIMObject bimObject)
    {
        // find _RC in fields
        var field = fields.FirstOrDefault(x => x.Value.name == "_RC");
        string fieldKey = field.Key;
        if (string.IsNullOrEmpty(fieldKey)) return;
        // get value category in props
        if (bimObject.props.TryGetValue(fieldKey, out var prop))
        {
            metadata.Category = prop?.ToString() ?? String.Empty;
        }
    }
    private string SetCategory(BIMObject bimObject)
    {
        var field = fields.FirstOrDefault(x => x.Value.name == "_RC");
        string fieldKey = field.Key;
        if (string.IsNullOrEmpty(fieldKey)) return string.Empty;
        if (bimObject.props.TryGetValue(fieldKey, out var prop))
        {
            return prop?.ToString() ?? String.Empty;
        }
        return String.Empty;
    }

    /// <summary>
    ///  Get Element Id from name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string GetElementId(string name)
    {
        // get elementId from name with pattern Name[5656]
        string pattern = @"\[\d+\]";
        Match match = Regex.Match(name, pattern);
        if (match.Success)
        {
            string elementId = match.Value;
            elementId = elementId.Replace("[", "").Replace("]", "");
            return elementId;
        }
        return "-1";
    }

    private void WriteObjectElementId(ChoParquetWriter choParquetWriter,BIMObject bimObject,string elementid)
    {
        string groupName = "Element ID";
        string intType = "Integer";
        Metadata metadata = new Metadata()
        {
            // SourceFile = displayName,
            ParameterName = "Value",
            ParameterValue = elementid,
            ParameterGroup = groupName,
            Category = SetCategory(bimObject),
            DataType = intType,
            DataTypeContext = String.Empty,
            Guid = bimObject?.externalId??string.Empty,
        };
        choParquetWriter.Write(metadata);
    }
    private void WriteObjectLocation(ChoParquetWriter choParquetWriter,BIMObject bimObject)
    {
        if(bimObject.bboxMin == null || bimObject.bboxMax == null) return;
        JObject bboxMin = bimObject.bboxMin;
        System.Numerics.Vector3 m1 = new System.Numerics.Vector3
        {
            X = (float)bboxMin["x"],
            Y = (float)bboxMin["y"],
            Z = (float)bboxMin["z"]
        };
        JObject bboxMax = bimObject.bboxMax;
        System.Numerics.Vector3 m2 = new System.Numerics.Vector3
        {
            X = (float)bboxMax["x"],
            Y = (float)bboxMax["y"],
            Z = (float)bboxMax["z"]
        };
        var centerPoint = (m1 + m2) / 2;
        string groupName = "Location";
        string doubleType = "Double";
        Metadata metadataX = new Metadata()
        {
            // SourceFile = displayName,
            ParameterName = "Location.X",
            ParameterValue = centerPoint.X,
            ParameterGroup = groupName,
            Category = SetCategory(bimObject),
            DataType = doubleType,
            DataTypeContext = String.Empty,
            Guid = bimObject.externalId!.ToString()
        };
        choParquetWriter.Write(metadataX);
        Metadata metadataY = new Metadata()
        {
            // SourceFile = displayName,
            ParameterName = "Location.Y",
            ParameterValue = centerPoint.Y,
            ParameterGroup =groupName,
            Category = SetCategory(bimObject),
            DataType = doubleType,
            DataTypeContext = String.Empty,
            Guid = bimObject.externalId
        };
        choParquetWriter.Write(metadataY);
        Metadata metadataZ = new Metadata()
        {
            // SourceFile = displayName,
            ParameterName = "Location.Z",
            ParameterValue = centerPoint.Z,
            ParameterGroup = groupName,
            Category = SetCategory(bimObject),
            DataType = doubleType,
            DataTypeContext = String.Empty,
            Guid = bimObject.externalId
        };
        choParquetWriter.Write(metadataZ);

    }
}

public class Metadata
{
    [JsonProperty("Guid")] public string Guid { get; set; }
    [JsonProperty("Category")] public string? Category { get; set; }
    [JsonProperty("ParameterGroup")] public string ParameterGroup { get; set; }
    [JsonProperty("ParameterName")] public string ParameterName { get; set; }
    [JsonProperty("DataType")] public string DataType { get; set; }
    [JsonProperty("DataTypeContext")] public string DataTypeContext { get; set; }
    [JsonProperty("ParameterValue")] public dynamic ParameterValue { get; set; }
}