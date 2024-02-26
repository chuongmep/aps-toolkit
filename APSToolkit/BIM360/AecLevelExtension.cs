// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.BIM360;

public class AecLevelExtension
{
    public bool BuildingStory { get; set; }
    public bool Structure { get; set; }
    public double ComputationHeight { get; set; }
    public bool GroundPlane { get; set; }
    public bool HasAssociatedViewPlans { get; set; }
    public double? ProjectElevation { get; set; }
}
