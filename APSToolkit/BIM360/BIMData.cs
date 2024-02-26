// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.BIM360;

public class BIMData
{
    public int? DbId { get; set; }
    public string? externalId { get; set; }
    public System.Numerics.Vector3? bboxMin { get; set; }
    public System.Numerics.Vector3? bboxMax { get; set; }
    public BIMProperty[] properties { get; set; }

}