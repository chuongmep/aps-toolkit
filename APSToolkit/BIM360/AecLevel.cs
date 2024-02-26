// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.BIM360;

public class AecLevel
{
    public string Guid { get; set; }
    public string Name { get; set; }
    public double Elevation { get; set; }
    public double Height { get; set; }
    public AecLevelExtension Extension { get; set; }
}