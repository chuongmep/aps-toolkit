// Copyright (c) chuongmep.com. All rights reserved

using JObject = Newtonsoft.Json.Linq.JObject;

namespace APSToolkit.BIM360;

public class BIMObject
{
    public int? svf2Id { get; set; }
    public int? otgId { get; set; }
    public string? lineageId { get; set; }
    public string? externalId { get; set; }
    public int? lmvId { get; set; }
    public string? databaseId { get; set; }
    public Dictionary<string,object> props { get; set; }
    public string? propsHash { get; set; }
    public string? geomHash { get; set; }
    public JObject? bboxMin { get; set; }
    public JObject? bboxMax { get; set; }
    public List<object>? views { get; set; }
}
