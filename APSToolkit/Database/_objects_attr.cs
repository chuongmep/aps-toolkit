// Copyright (c) chuongmep.com. All rights reserved

using System.Runtime.Serialization;

namespace APSToolkit.Database;
// Copyright (c) chuongmep.com. All rights reserved

public class _objects_attr
{
    [DataMember(Order = 1)]
    public int id { get; set; }
    [DataMember(Order = 2)]
    public string? name { get; set; }
    [DataMember(Order = 3)]
    public string? category { get; set; }
    [DataMember(Order = 4)]
    public int data_type { get; set; }
    [DataMember(Order = 5)]
    public string? data_type_context { get; set; }
    [DataMember(Order = 6)]
    public string? description { get; set; }
    [DataMember(Order = 7)]
    public string? display_name { get; set; }
    [DataMember(Order = 8)]
    public int flags { get; set; }
    [DataMember(Order = 9)]
    public int display_precision { get; set; }
    [DataMember(Order = 10)]
    public string? forge_parameter_id { get; set; }
}