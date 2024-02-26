// Copyright (c) chuongmep.com. All rights reserved

using System.Runtime.Serialization;

namespace APSToolkit.Database;

public class _object_id
{
    [DataMember(Order = 1)]
    public int id { get; set; }

    /// <summary>
    ///  guid of object
    /// </summary>
    [DataMember(Order = 2)]
    public string?  external_id { get; set; }

    /// <summary>
    ///  viewable id of object
    /// </summary>
    [DataMember(Order = 3)]
    public string viewable_id { get; set; }
}