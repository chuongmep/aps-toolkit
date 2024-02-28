// Copyright (c) chuongmep.com. All rights reserved
using System.Data;
using System.Data.SQLite;
using System.Text;
using APSToolkit.Auth;
using APSToolkit.Utils;
using RestSharp;


namespace APSToolkit.Database;

/// <summary>
/// Class Reader Database Sqlite APS
/// </summary>
public class DbReader
{
    public List<_object_id> objectIds { get; private set; }
    public List<_objects_attr> objectAttrs { get; private set; }
    public List<_objects_eav> objectsEavs { get; private set; }
    public List<_object_val> objectVals { get; private set; }

    private static bool IsClearCache { get; set; }
    /// <summary>
    /// database path
    /// </summary>
    private string dbPath { get; set; }

    public DbReader(List<_object_id> objectIds, List<_objects_attr> objectAttrs, List<_objects_eav> objectsEavs,
        List<_object_val> objectVals)
    {
        this.objectIds = objectIds;
        this.objectAttrs = objectAttrs;
        this.objectsEavs = objectsEavs;
        this.objectVals = objectVals;
    }

    /// <summary>
    /// Init database reader
    /// </summary>
    /// <param name="databasePath">the local path of database</param>
    public DbReader(string databasePath)
    {
        dbPath = databasePath;
        ReadData(databasePath);
    }

    /// <summary>
    /// Init database reader with urn and token aps
    /// </summary>
    /// <param name="urn"></param>
    /// <param name="token"></param>
    /// <param name="isClearCache"></param>
    public DbReader(string? urn, string token,bool isClearCache=true)
    {
        // check if input urn is path, throw exception
        if (urn.Contains(@"\"))
        {
            throw new Exception("urn require is derivative urn, not path");
        }
        ReadData(urn, token).Wait();
        IsClearCache = isClearCache;
    }

    public Task<DataTable> ReadAllData()
    {
        string sqlQuery = @"
                SELECT _objects_id.id AS dbId, _objects_id.external_id AS externalId, 
                       _objects_attr.name AS propName, _objects_val.value AS propValue
                FROM _objects_eav
                INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
                INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
                INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id";
        DataTable dataTable = ExecuteQuery(sqlQuery);
        return Task.FromResult(dataTable);
    }

    /// <summary>
    /// Reads data from a remote database using the provided URN and access token asynchronously.
    /// </summary>
    /// <param name="urn">The URN (Unique Resource Name) of the database.</param>
    /// <param name="token">The access token for authentication.</param>
    /// <exception cref="Exception">Thrown if the derivative URN or token is null, the database is not found, or reading the database fails.</exception>
    private async Task ReadData(string? urn, string token)
    {
        if (string.IsNullOrEmpty(urn)) throw new Exception("derivative urn is null");
        if (string.IsNullOrEmpty(token)) throw new Exception("token is null");
        List<Derivatives.Resource> extractDataBaseAsync =
            await Derivatives.ExtractDataBaseAsync(urn, token).ConfigureAwait(false);
        Derivatives.Resource database =
            extractDataBaseAsync.FirstOrDefault(x => x.FileName.EndsWith(".db") || x.FileName.EndsWith(".sdb"));
        if (string.IsNullOrEmpty(database.RemotePath)) throw new Exception("Can not find database");
        // download and query
        RestRequest request = new RestRequest(database.RemotePath);
        request.Method = Method.Get;
        request.AddHeader("Authorization", "Bearer " + token);
        request.AddHeader("Accept-Encoding", "gzip, deflate");
        byte[]? requestData = await Derivatives.ExecuteRequestData(request, database).ConfigureAwait(false);
        if (requestData == null) throw new Exception("Can not read database");
        // save to local and query
        if(string.IsNullOrEmpty(database.FileName)) throw new Exception("Can not find database file path");
        var ext = database.FileName.EndsWith(".db") ? ".db" : ".sdb";
        var FileName = Guid.NewGuid() + ext;
        string path = Path.Combine(Path.GetTempPath(), FileName);
        // TODO : Do we have any way to Read Data from Memory Stream?
        await File.WriteAllBytesAsync(path, requestData).ConfigureAwait(false);
        this.dbPath = path;
        ReadData(path);
    }

    /// <summary>
    /// Reads data from a SQLite database file and populates lists of objects representing database tables.
    /// </summary>
    /// <param name="databasePath">The path to the SQLite database file.</param>
    /// <exception cref="Exception">Thrown if the database file is not found or if there are issues reading data from the database.</exception>
    private void ReadData(string databasePath)
    {
        objectIds = new List<_object_id>();
        objectAttrs = new List<_objects_attr>();
        objectsEavs = new List<_objects_eav>();
        objectVals = new List<_object_val>();
        // get data from database push to objectIds
        string query_ObjIds = "SELECT * FROM _objects_id";
        string query_ObjAttrs = "SELECT * FROM _objects_attr";
        string query_ObjEavs = "SELECT * FROM _objects_eav";
        string query_ObjVals = "SELECT * FROM _objects_val";
        if (!File.Exists(databasePath)) throw new Exception("can't find database file");
        using (SQLiteConnection conn = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
        {
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand(query_ObjIds, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _object_id objectId = new _object_id();
                        objectId.id = reader.GetInt32(0);
                        objectId.external_id = reader.GetString(1).ToString();
                        objectIds.Add(objectId);
                    }
                }
            }

            using (SQLiteCommand cmd = new SQLiteCommand(query_ObjAttrs, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _objects_attr objectAttr = new _objects_attr();
                        objectAttr.id = reader.GetInt32(0);
                        objectAttr.name = reader.GetString(1).ToString();
                        objectAttr.category = reader.GetString(2).ToString();
                        objectAttr.data_type = reader.GetInt32(3);
                        objectAttr.data_type_context = reader.GetValue(4).ToString();
                        objectAttr.description = reader.GetValue(5).ToString();
                        objectAttr.display_name = reader.GetValue(6).ToString();
                        objectAttr.flags = reader.GetInt32(7);
                        objectAttr.display_precision = reader.GetInt32(8);
                        objectAttr.forge_parameter_id = reader.GetValue(9).ToString();
                        objectAttrs.Add(objectAttr);
                    }
                }
            }

            using (SQLiteCommand cmd = new SQLiteCommand(query_ObjEavs, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _objects_eav objectEav = new _objects_eav();
                        objectEav.id = reader.GetInt32(0);
                        objectEav.entity_id = reader.GetInt32(1);
                        objectEav.attribute_id = reader.GetInt32(2);
                        objectEav.value_id = reader.GetInt32(3);
                        objectsEavs.Add(objectEav);
                    }
                }
            }

            using (SQLiteCommand cmd = new SQLiteCommand(query_ObjVals, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _object_val objectVal = new _object_val();
                        objectVal.id = reader.GetInt32(0);
                        objectVal.value =
                            Encoding.UTF8.GetString((byte[]) reader["value"]); // Assuming value is stored as BLOB
                        objectVals.Add(objectVal);
                    }
                }
            }

            conn.Close();
        }
    }

    /// <summary>
    /// Executes an SQL query on the SQLite database and returns the result as a DataTable.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <returns>A DataTable containing the result of the query.</returns>
    public DataTable ExecuteQuery(string query)
    {
        var dt = new DataTable();
        using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            dt.Load(reader);
        }

        return dt.FixBytesValue();
    }

    /// <summary>
    /// Retrieves all children of a specified database ID from the SQLite database.
    /// </summary>
    /// <param name="dbId"></param>
    /// <returns>A DataTable containing the database ID (dbid) and child IDs (child_id) of all children.</returns>
    public int[] GetAllChildren(int dbId)
    {
        List<int> instances = new List<int>();

        foreach (var prop in GetPropertiesByDbId(dbId))
        {
            if (prop.Category == "__child__")
            {
                instances.Add(int.Parse(prop.Value));
            }
        }

        return instances.ToArray();
    }

    /// <summary>
    /// Retrieves all parent relationships of a specified database ID from the SQLite database.
    /// </summary>
    /// <returns>A DataTable containing the database ID (dbid) and parent IDs (child_id) of all parent relationships.</returns>
    public int[] GetAllParent(int dbId)
    {
        List<int> parent = new List<int>();

        foreach (var prop in GetPropertiesByDbId(dbId))
        {
            if (prop.Category == "__parent__")
            {
                parent.Add(int.Parse(prop.Value));
            }
        }

        return parent.ToArray();
    }

    public int[] GetInstanceOf(string external_id)
    {
        List<int> instances = new List<int>();
        IEnumerable<Property> propertiesByDbId = GetPropertiesByExternalId(external_id);
        foreach (var prop in propertiesByDbId)
        {
            if (prop.Category == "__instanceof__")
            {
                instances.Add(int.Parse(prop.Value));
            }
        }
        return instances.ToArray();
    }

    /// <summary>
    /// Retrieves all properties from the objectsEavs, objectIds, objectAttrs, and objectVals collections.
    /// </summary>
    /// <returns>A list of Property objects containing information about each property.</returns>
    public IEnumerable<Property> GetAllProperties()
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            select new Property()
            {
                Id = objectId.external_id,
                Category = attribute.category,
                Name = attribute.name,
                Value = value.value,
                DataType = EnumRecords.GetDataType(attribute.data_type),
                DataTypeContext = attribute.data_type_context,
                Description = attribute.description,
                DisplayName = attribute.display_name,
                DisplayPrecision = attribute.display_precision,
                ForgeParameterId = attribute.forge_parameter_id,
                Flags = attribute.flags,
            };
        return query;
    }

    public string?[] GetAllElementUniqueId()
    {
        return objectIds.Select(x => x.external_id)
            .Where(x => !x!.EndsWith("Prototype"))
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Retrieves properties based on the specified external ID from the objectsEavs, objectIds, objectAttrs, and objectVals collections.
    /// </summary>
    /// <param name="external_id">The external ID for which to retrieve properties.</param>
    /// <returns>A list of Property objects containing information about each property.</returns>
    public IEnumerable<Property> GetPropertiesByExternalId(string external_id)
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where objectId.external_id == external_id //&& attribute.display_name !=string.Empty
            select new Property()
            {
                Id = objectId.external_id,
                Category = attribute.category,
                Name = attribute.name,
                Value = value.value,
                DataType = EnumRecords.GetDataType(attribute.data_type),
                DataTypeContext = attribute.data_type_context,
                Description = attribute.description,
                DisplayName = attribute.display_name,
                DisplayPrecision = attribute.display_precision,
                ForgeParameterId = attribute.forge_parameter_id,
                Flags = attribute.flags,
            };
        return query;
    }

    /// <summary>
    /// Retrieves public properties (where display_name is not empty) based on the specified external ID from the objectsEavs, objectIds, objectAttrs, and objectVals collections.
    /// </summary>
    /// <param name="external_id">The external ID for which to retrieve public properties.</param>
    public IEnumerable<Property> GetPublicPropertiesByExternalId(string external_id)
    {
        IEnumerable<Property> query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where objectId.external_id == external_id && attribute.display_name != string.Empty
            select new Property()
            {
                Id = objectId.external_id,
                Category = attribute.category,
                Name = attribute.name,
                Value = value.value,
                DataType = EnumRecords.GetDataType(attribute.data_type),
                DataTypeContext = attribute.data_type_context,
                Description = attribute.description,
                DisplayName = attribute.display_name,
                DisplayPrecision = attribute.display_precision,
                ForgeParameterId = attribute.forge_parameter_id,
                Flags = attribute.flags,
            };
        return query;
    }

    /// <summary>
    /// Retrieves a list of public properties associated with a given database ID.
    /// </summary>
    /// <param name="id">The database ID for which public properties need to be retrieved.</param>
    /// <returns>
    /// A list of <see cref="Property"/> objects containing information such as ID, Category, Name, Value, DataType, DataTypeContext,
    /// Description, DisplayName, DisplayPrecision, ForgeParameterId, and Flags.
    /// </returns>
    public IEnumerable<Property> GetPublicPropertiesByDbId(int id)
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where objectId.id == id && attribute.display_name != string.Empty
            select new Property()
            {
                Id = objectId.external_id,
                Category = attribute.category,
                Name = attribute.name,
                Value = value.value,
                DataType = EnumRecords.GetDataType(attribute.data_type),
                DataTypeContext = attribute.data_type_context,
                Description = attribute.description,
                DisplayName = attribute.display_name,
                DisplayPrecision = attribute.display_precision,
                ForgeParameterId = attribute.forge_parameter_id,
                Flags = attribute.flags,
            };
        return query;
    }
    public IEnumerable<Property> GetPropertiesByDbId(int id)
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where objectId.id == id
            select new Property()
            {
                Id = objectId.external_id,
                Category = attribute.category,
                Name = attribute.name,
                Value = value.value,
                DataType = EnumRecords.GetDataType(attribute.data_type),
                DataTypeContext = attribute.data_type_context,
                Description = attribute.description,
                DisplayName = attribute.display_name,
                DisplayPrecision = attribute.display_precision,
                ForgeParameterId = attribute.forge_parameter_id,
                Flags = attribute.flags,
            };
        return query;
    }



}