using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.Nodes;
using APSToolkit;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class CompareTest
{
    [SetUp]
    public void SetUp()
    {
        var auth = new Auth();
        Settings.Token2Leg = auth.Get2LeggedToken().Result;
    }

    [Test]
    public void CompareExecute()
    {
        string path1 = @"D:\API\Forge\ExploreForgeGeometry\APSToolkitUnit\bin\Debug\net6\version9.json";
        string path2 = @"D:\API\Forge\ExploreForgeGeometry\APSToolkitUnit\bin\Debug\net6\version10.json";
        // get string json path1
        var stringJsonPath1 = System.IO.File.ReadAllText(path1);
        // get string json path2
        var stringJsonPath2 = System.IO.File.ReadAllText(path2);
        List<RoomData> roomDatas = JsonConvert.DeserializeObject<List<RoomData>>(stringJsonPath1);
        RoomData enumerable = roomDatas.FirstOrDefault(x => x.Id == "3026299");
        List<RoomData> roomDatas2 = JsonConvert.DeserializeObject<List<RoomData>>(stringJsonPath2);
        JsonNode node1 = JsonNode.Parse(stringJsonPath1);
        JsonNode node2 = JsonNode.Parse(stringJsonPath2);
        JsonNode diff = node1.Diff(node2);
        Console.WriteLine(diff);
    }
}
public class RoomData
{
    public string Id { get; set; }
    public string UniqueId { get; set; }
    public Dictionary<string,string> Parameters { get; set; }
    public List<RoomPoint> Boundaries { get; set; }
}
public class RoomPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public RoomPoint(double x, double y, double z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
}