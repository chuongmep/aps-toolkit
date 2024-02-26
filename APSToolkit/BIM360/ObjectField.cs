// Copyright (c) chuongmep.com. All rights reserved

using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;

namespace APSToolkit.BIM360;

[JsonConverter(typeof(JsonStringContract))]
public class BIMField
{
    public string key { get; set; }
    public string category { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string uom { get; set; }
}
