// Copyright (c) chuongmep.com. All rights reserved

using System.Runtime.Serialization;

namespace APSToolkit.Database;

public class _object_val
{
    /// <summary>
    ///  index if of value table
    /// </summary>
    [DataMember(Order = 1)]
    public int id { get; set; }


    /// <summary>
    /// Value of parameterc
    /// </summary>
    [DataMember(Order = 2)]
    public string? value { get; set; }
    
}