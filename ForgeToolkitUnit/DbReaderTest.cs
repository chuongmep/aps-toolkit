using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using APSToolkit.Auth;
using APSToolkit.Database;
using APSToolkit.Utils;
using NUnit.Framework;

namespace ForgeToolkitUnit;

[TestFixture]
public class DbReaderTest
{
    private DbReader DbReader { get; set; }
    [SetUp]
    public void Setup()
    {
        Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
        DbReader = new DbReader(Settings._RevitTestUrn, Settings.Token2Leg);
    }

    [Test]
    public Task ReadQueryDatabaseElementIdTest()
    {
        string sqlQuery = @"
            SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId, 
                   _objects_attr.name AS name,_objects_attr.display_name AS propName , 
                   _objects_val.value AS propValue
            FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
            WHERE name = 'ElementId'
            ";
        // update for get
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        Assert.IsTrue(dataTable.Rows.Count > 0);
        return Task.CompletedTask;
    }

    [Test]
    public Task ReadQueryDatabaseByExternalIdTest()
    {
        string sqlQuery = $@"
            SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId, 
                   _objects_attr.name AS name,_objects_attr.display_name AS propName , 
                   _objects_val.value AS propValue
            FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
            WHERE externalId = '5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe'
           ";
        // update for get
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        Assert.IsTrue(dataTable.Rows.Count > 0);
        return Task.CompletedTask;
    }

    [Test]
    public Task TestGetAllPublicPublicProperty()
    {
        string sqlQuery = $@"
             SELECT ids.id AS dbid, attrs.category AS category, COALESCE(NULLIF(attrs.display_name, ''), 
             attrs.name) AS name, vals.value AS value
            FROM _objects_eav eav
            LEFT JOIN _objects_id ids ON ids.id = eav.entity_id
            LEFT JOIN _objects_attr attrs ON attrs.id = eav.attribute_id
            LEFT JOIN _objects_val vals on vals.id = eav.value_id
            WHERE category NOT LIKE '\_\_%\_\_' ESCAPE '\' /* skip internal properties */
            ORDER BY dbid
           ";
        // update for get
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        Assert.IsTrue(dataTable.Rows.Count > 0);
        return Task.CompletedTask;
    }

    [Test]
    public Task TestReadQueryDatabaseRevitCategory()
    {
        string sqlQuery = @"
            SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId,
                   _objects_attr.name AS name,_objects_attr.display_name AS propName , 
                   _objects_val.value AS propValue
            FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
            WHERE name LIKE '_RC' ESCAPE '\'
            ";
        // update for get
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        dataTable.ExportToCsv("result.csv");
        Assert.IsTrue(dataTable.Rows.Count > 0);
        return Task.CompletedTask;
    }

    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public Task TestReadQueryDatabaseRevitCategoryLocal(string urn)
    {

        string sqlQuery = @"
    SELECT DISTINCT _objects_id.external_id AS externalId,
                    _objects_val.value AS propValue
    FROM _objects_eav
        INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
        INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
        INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
    WHERE _objects_attr.name LIKE '_RC' AND _objects_val.value IS NOT NULL AND _objects_val.value <> ''
";
        // update for get
        DataTable dataTable = new DbReader(urn).ExecuteQuery(sqlQuery);
        dataTable.ExportToCsv("result.csv");
        Assert.IsTrue(dataTable.Rows.Count > 0);
        return Task.CompletedTask;
    }

    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void InitTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        Assert.AreNotEqual(0, dbReader.objectAttrs.Count);
        Assert.AreNotEqual(0, dbReader.objectIds.Count);
        Assert.AreNotEqual(0, dbReader.objectsEavs.Count);
        Assert.AreNotEqual(0, dbReader.objectVals.Count);
    }

    [Test]
    public void InitRemoteTest()
    {
        Assert.AreNotEqual(0, DbReader.objectAttrs.Count);
        Assert.AreNotEqual(0, DbReader.objectIds.Count);
        Assert.AreNotEqual(0, DbReader.objectsEavs.Count);
        Assert.AreNotEqual(0, DbReader.objectVals.Count);
    }
    [Test]
    public void InitMagicSqlTest()
    {
        List<_object_id> objectIds = new List<_object_id>();
        objectIds.Add(new _object_id() {id = 1, external_id = "1"});
        objectIds.Add(new _object_id() {id = 2, external_id = "2"});
        List<_objects_eav> objectsEavs = new List<_objects_eav>();
        objectsEavs.Add(new _objects_eav() {entity_id = 1, attribute_id = 1, value_id = 1});
        objectsEavs.Add(new _objects_eav() {entity_id = 2, attribute_id = 2, value_id = 2});
        List<_objects_attr> objectAttrs = new List<_objects_attr>();
        objectAttrs.Add(new _objects_attr() {id = 1, name = "1", display_name = "1"});
        objectAttrs.Add(new _objects_attr() {id = 2, name = "2", display_name = "2"});
        List<_object_val> objectVals = new List<_object_val>();
        objectVals.Add(new _object_val() {id = 1, value = "1"});
        objectVals.Add(new _object_val() {id = 2, value = "2"});
        DbReader dbReader = new DbReader(objectIds, objectAttrs, objectsEavs, objectVals);
        Assert.AreNotEqual(0, dbReader.objectAttrs.Count);
        Assert.AreNotEqual(0, dbReader.objectIds.Count);
        Assert.AreNotEqual(0, dbReader.objectsEavs.Count);
        Assert.AreNotEqual(0, dbReader.objectVals.Count);
    }
    [Test]
    public void ExecuteQueryTest()
    {
        string sqlQuery = $@"
            SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId, 
                   _objects_attr.name AS name,_objects_attr.display_name AS propName , 
                   _objects_val.value AS propValue
            FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
            WHERE externalId = '5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe'
           ";
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        dataTable.ExportToCsv("result.csv");
        Assert.IsTrue(dataTable.Columns.Count > 0);
    }
    [Test]
    public void ExecuteQueryCustomColTest()
    {
        string sqlQuery = $@"
            SELECT _objects_id.id AS dbId, 
                   _objects_id.external_id AS externalId,
                   _objects_attr.category AS category,
                   _objects_attr.data_type_context AS dataType,
                   _objects_attr.display_name AS propName , 
                   _objects_val.value AS propValue
            FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
            WHERE externalId = '5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe' 
            and category NOT LIKE '\_\_%\_\_' ESCAPE '\'
            ORDER BY propName ASC
           ";
        DataTable dataTable = DbReader.ExecuteQuery(sqlQuery);
        dataTable.ExportToCsv("result.csv");
        Assert.IsTrue(dataTable.Columns.Count > 0);
    }

    [Test]
    public void GetAllPropertiesTest()
    {
        IEnumerable<Property> allProperties = DbReader.GetAllProperties();
        IEnumerable<Property> properties = allProperties as Property[] ?? allProperties.ToArray();
        properties.ExportToCsv("result.csv");
        Assert.IsNotNull(allProperties);
        Assert.IsTrue(properties.Any());
    }

    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetPropertiesByExternalIdTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        IEnumerable<Property> allProperties =
            dbReader.GetPropertiesByExternalId("5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe");
        IEnumerable<Property> properties = allProperties as Property[] ?? allProperties.ToArray();
        properties.OrderBy(x=>x.Category).ExportToCsv("result.csv");
        Assert.IsTrue(properties.Any());
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetPublicPropertiesByExternalIdTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        IEnumerable<Property> allProperties =
            dbReader.GetPublicPropertiesByExternalId("5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe");
        allProperties.ExportToCsv("result.csv");
        Assert.IsTrue(allProperties.Any());
    }


    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllInstanceOfTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        string chairId = "5bb069ca-e4fe-4e63-be31-f8ac44e80d30-00046bfe";
        var instances =
            dbReader.GetInstanceOf(chairId);
        instances.ExportToCsv("result.csv");
        Assert.IsTrue(instances.Length > 0);
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllChildrenTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        int docId = 1;
        int[] children =
            dbReader.GetAllChildren(docId);
        children.ExportToCsv("result.csv");
        Assert.IsTrue(children.Length > 0);
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllParentTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        int chairId = 3526;
        int[] allParent =
            dbReader.GetAllParent(chairId);
        Assert.IsTrue(allParent.Length > 0);
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllPropertiesLocalTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        IEnumerable<Property> allProperties = dbReader.GetAllProperties();
        IEnumerable<Property> properties = allProperties as Property[] ?? allProperties.ToArray();
        Assert.IsTrue(properties.Any());
        properties.ExportToCsv("result.csv");
    }
    [Test]
    [TestCase("Resources/model_rvt.sdb")]
    public void GetAllElementUniqueIdTest(string dbPath)
    {
        DbReader dbReader = new DbReader(dbPath);
        string[] uniqueIds = dbReader.GetAllElementUniqueId();
        Assert.IsTrue(uniqueIds.Length > 0);
    }
}