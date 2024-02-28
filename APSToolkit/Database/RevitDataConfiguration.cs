using APSToolkit.Auth;
using APSToolkit.Schema;
using APSToolkit.Utils;
using OfficeOpenXml.Style;

namespace APSToolkit.Database;

public class RevitDataConfiguration
{

    /// <summary>
    /// True to add units to the value of parameter object APS
    /// </summary>
    public bool IsAddUnits { get; set; } = true;

    /// <summary>
    /// True to get the parameter type of the object APS
    /// </summary>
    public bool IsGetParameterType { get; set; } = true;
    /// <summary>
    /// True to get the internal parameter of the object APS
    /// </summary>
    public bool IsGetInternalParameter { get; set; } = false;

    /// <summary>
    /// True to get the bounding box of the object
    /// </summary>
    public bool IsGetBBox { get; set; } = false;

    public string Urn { get; set; }

    public Token Token { get; set; }

    public Dictionary<string, string> Units = new Dictionary<string, string>();

    public Dictionary<int,ISvfFragment> Fragments = new Dictionary<int, ISvfFragment>();
    public RevitDataConfiguration()
    {

    }
    public RevitDataConfiguration(string urn,Token token) : this()
    {
        this.Urn = urn;
        this.Token = token;
        InitConfiguration();
    }
    public RevitDataConfiguration(string urn) : this()
    {
        this.Urn = urn;
        this.Token = Authentication.Get2LeggedToken().Result;
        InitConfiguration();
    }


    private async void InitConfiguration()
    {
        if (IsAddUnits)
        {
            Units = UnitUtils.GetAllDictUnits();
        }

        if (IsGetBBox)
        {
            Dictionary<int,ISvfFragment> svfFragments = await GetFragments(Token.access_token);
            Fragments = svfFragments;
        }
    }

    private async Task<Dictionary<int, ISvfFragment>> GetFragments(string accessToken)
    {
        Dictionary<string, ISvfFragment[]> fragments =
            await Derivatives.ReadFragmentsRemoteAsync(Urn, accessToken).ConfigureAwait(false);
        // flatten the fragments to list of svf fragments
        List<ISvfFragment> svfFragments = fragments.Values.SelectMany(x => x).ToList();
        // save to location with unique dbid and value
        Dictionary<int, ISvfFragment> locations = new Dictionary<int, ISvfFragment>();
        foreach (ISvfFragment svfFragment in svfFragments)
        {
            if (svfFragment.dbID == 0) continue;
            if (svfFragment.transform == null) continue;
            locations.TryAdd(svfFragment.dbID, svfFragment);
        }
        return locations;
    }

}