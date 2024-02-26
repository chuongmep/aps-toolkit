// Copyright (c) chuongmep.com. All rights reserved

using System.Runtime.Serialization;

namespace APSToolkit.Database;
// Copyright (c) chuongmep.com. All rights reserved

public class _objects_eav
{
    [DataMember(Order = 1)]
    public int id { get; set; }
    [DataMember(Order = 2)]
    public int entity_id { get; set; }
    [DataMember(Order = 3)]
    public int attribute_id { get; set; }
    [DataMember(Order = 4)]
    public int value_id { get; set; }

}