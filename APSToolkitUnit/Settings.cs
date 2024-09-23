using System;
using System.Collections.Generic;
using APSToolkit;

namespace ForgeToolkitUnit;

public static class Settings
{
    public const string _NavisUrn =
        "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLmZqSkFRUUx6U3BxLTR3eWRPdG9DMGc_dmVyc2lvbj0x";

    public const string _RevitTestUrn =
        "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLnZMb1pqZHRLU3ktbVFjZEZBamFCOFE_dmVyc2lvbj0x";

    public const string _IfcUrn =
        "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLjEzLVdVN2NBU2kyQThVdUNqQVFmUkE_dmVyc2lvbj0x";

    // real model
    public const string _RevitRealUrn =
        "dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLm84d0tfSUNjUlphcHlhbUp5MmtFVmc_dmVyc2lvbj03";

    public const string HubId = "b.1715cf2b-cc12-46fd-9279-11bbc47e72f6";

    // public const string ProjectId = "b.ec0f8261-aeca-4ab9-a1a5-5845f952b17d";
    public const string ProjectId = "b.ca790fb5-141d-4ad5-b411-0461af2e9748";

    public const string ModelGuid = "de840e8a-ab89-4833-ae8e-c47bc6ca94d8";

    public const string ProjectGuid = "de37dbf5-20d5-46be-b488-fec29e3b436e";
    // for real :SG_Meta:  b.ca790fb5-141d-4ad5-b411-0461af2e9748

    public const string FolderId = "urn:adsk.wipprod:fs.folder:co.2yCTHGmWSvSCzlaIzdrFKA";

    // Cloud Model Urn Myhouse.Rvt
    public const string ItemId = "urn:adsk.wipprod:dm.lineage:Od8txDbKSSelToVg1oc1VA";
    public const string VersionId = "urn:adsk.wipprod:fs.file:vf.Od8txDbKSSelToVg1oc1VA?version=13";
    public const string FileName = "MyHouse.rvt";

    public static Token Token2Leg;
    public static Token Token3Leg;

    public static IEnumerable<object[]> UrnTestCases()
    {
        yield return new object[] {_NavisUrn};
        yield return new object[] {_RevitRealUrn};
    }
    public static IEnumerable<object[]> RevitTestUrn()
    {
        yield return new object[] {_RevitTestUrn};
    }
    public static IEnumerable<object[]> RevitRealUrn()
    {
        yield return new object[] {_RevitRealUrn};
    }
    public static IEnumerable<object[]> IFCTestUrn()
    {
        yield return new object[] {_IfcUrn};
    }
    public static IEnumerable<object[]> NavisTestUrn()
    {
        yield return new object[] {_NavisUrn};
    }
}