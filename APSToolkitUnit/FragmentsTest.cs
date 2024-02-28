using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APSToolkit;
using APSToolkit.Auth;
using APSToolkit.Schema;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ForgeToolkitUnit;

public class FragmentsTest
{
    public static string Token { get; set; }
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
    }

    /// <summary>
    /// str
    /// </summary>
    /// <param name="urn"></param>
    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public async Task TestGetFragments(string urn)
    {
        var fragments = await Derivatives.ReadFragmentsRemoteAsync(urn, Settings.Token2Leg.access_token);
        Assert.AreNotEqual(0, fragments.Count);
    }

    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public async Task GetElementLocation(string urn)
    {
        Dictionary<string, ISvfFragment[]> fragments = await Derivatives.ReadFragmentsRemoteAsync(urn, Settings.Token2Leg.access_token);
        // flatten the fragments to list of svf fragments
        List<ISvfFragment> svfFragments = fragments.Values.SelectMany(x => x).ToList();
        // save to location with unique dbid and value
        Dictionary<int, ISvfFragment> locations = new Dictionary<int, ISvfFragment>();
        foreach (ISvfFragment svfFragment in svfFragments)
        {
            if (svfFragment.dbID == 0) continue;
            if(svfFragment.transform==null) continue;
            locations.TryAdd(svfFragment.dbID, svfFragment);
        }
        // sort by dbid
        locations = locations.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        // save to json string
        string json = JsonConvert.SerializeObject(locations);
        Assert.AreNotEqual(0, json.Length);
    }
    [Test]
    [TestCase(Settings._RevitTestUrn)]
    public async Task GetElementGeometry(string urn)
    {
        Dictionary<string, ISvfGeometryMetadata[]> fragments = await Derivatives.ReadGeometriesRemoteAsync(urn, Settings.Token2Leg.access_token);
        // flatten the fragments to list of svf fragments
        List<ISvfGeometryMetadata> svfFragments = fragments.Values.SelectMany(x => x).ToList();
        // save to location with unique dbid and value
        Dictionary<int, ISvfGeometryMetadata> locations = new Dictionary<int, ISvfGeometryMetadata>();
        foreach (ISvfGeometryMetadata svfFragment in svfFragments)
        {
            if (svfFragment.entityID == 0) continue;
            locations.TryAdd(svfFragment.entityID, svfFragment);
        }
        // sort by dbid
        locations = locations.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        // save to json string
        string json = JsonConvert.SerializeObject(locations);
        Assert.AreNotEqual(0, json.Length);
    }

    [Test]
    [TestCase("dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLm84d0tfSUNjUlphcHlhbUp5MmtFVmc_dmVyc2lvbj03")]
    public async Task TestGetBoundingBoxByFragment(string urn)
    {
        Dictionary<string, ISvfFragment[]> fragments = await Derivatives.ReadFragmentsRemoteAsync(urn,Settings. Token2Leg.access_token);
        string phase = fragments.Keys.FirstOrDefault(x => x.Contains("New Construction"));
        ISvfFragment[] svfFragments = fragments[phase];
        ISvfFragment[] array = svfFragments.Where(x => x.dbID == 17778).ToArray();
        Assert.AreNotEqual(0, array.Length);
        // save to json string
        string json = JsonConvert.SerializeObject(array);
        Assert.AreNotEqual(0, json.Length);
    }


}