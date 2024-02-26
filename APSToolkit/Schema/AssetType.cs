using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace APSToolkit.Schema;


[JsonConverter(typeof(ForceDefaultConverter))]
public enum AssetType
{
    [EnumMember(Value = "Autodesk.CloudPlatform.Image")]
    Image,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyViewables")]
    PropertyViewables,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyOffsets")]
    PropertyOffsets,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyAttributes")]
    PropertyAttributes,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyValues")]
    PropertyValues,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyIDs")]
    PropertyIDs,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyAVs")]
    PropertyAVs,
    [EnumMember(Value = "Autodesk.CloudPlatform.PropertyRCVs")]
    PropertyRCVs,
    [EnumMember(Value = "ProteinMaterials")]
    ProteinMaterials,
    [EnumMember(Value = "Autodesk.CloudPlatform.PackFile")]
    PackFile,
    [EnumMember(Value = "Autodesk.CloudPlatform.FragmentList")]
    FragmentList,
    [EnumMember(Value = "Autodesk.CloudPlatform.GeometryMetadataList")]
    GeometryMetadataList,
    [EnumMember(Value = "Autodesk.CloudPlatform.InstanceTree")]
    InstanceTree,
    [EnumMember(Value = "Autodesk.CloudPlatform.ViewingMetadata")]
    ViewingMetadata,
    [EnumMember(Value = "Topology")]
    Topology
}