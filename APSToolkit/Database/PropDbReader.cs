// Copyright (c) chuongmep.com. All rights reserved

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using APSToolkit.Auth;
using APSToolkit.Utils;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using OfficeOpenXml;
using RestSharp;

namespace APSToolkit.Database
{
    /// <summary>
    /// Class reader properties with gzip files
    /// </summary>
    public class PropDbReader
    {
        internal string Urn { get; set; }

        /// <summary>
        /// Object IDs
        /// </summary>
        public string[]? ids { get; set; }

        /// <summary>
        /// Object offsets
        /// </summary>
        public int[]? offsets { get; set; }

        /// <summary>
        /// Object AVS
        /// </summary>
        public int[]? avs { get; set; }

        /// <summary>
        /// Object attributes
        /// </summary>
        public object[]? attrs { get; set; }

        /// <summary>
        /// Object values
        /// </summary>
        public string[]? vals { get; set; }

        /// <summary>
        /// Decompresses a GZip-compressed byte array.
        /// </summary>
        /// <param name="input">The GZip-compressed byte array to decompress.</param>
        /// <returns>The decompressed byte array.</returns>
        private static byte[] Unzip(byte[] input)
        {
            using (var compressedStream = new MemoryStream(input))
            using (var resultStream = new MemoryStream())
            {
                GZip.Decompress(compressedStream, resultStream, true);
                return resultStream.ToArray();
            }
        }

        private Token Token { get; set; }

        /// <summary>
        /// Reads a GZip-compressed file from the specified path, decompresses its content, and returns the decompressed byte array.
        /// </summary>
        /// <param name="filePath">The path to the GZip-compressed file.</param>
        /// <returns>The decompressed byte array.</returns>
        private static byte[] Unzip(string filePath)
        {
            byte[] input = File.ReadAllBytes(filePath);
            return Unzip(input);
        }

        public PropDbReader(string urn)
        {
            this.Urn = urn;
            Token = Authentication.Get2LeggedToken().Result;
        }
        public PropDbReader()
        {
            Token = Authentication.Get2LeggedToken().Result;
        }
        /// <summary>
        /// Read All Information properties from urn model
        /// </summary>
        /// <param name="urn"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public PropDbReader(string urn, Token token)
        {
            this.Urn = urn;
            Token = token;
            DownloadStreamAsync(urn, token.access_token).Wait();
        }

        /// <summary>
        /// Downloads and extracts specific JSON resources related to object properties from the Autodesk Forge derivative service.
        /// The extracted data is stored in the instance of the PropDbReader class.
        /// </summary>
        /// <param name="urn">The unique identifier for the model in the Autodesk Forge derivative service.</param>
        /// <param name="accessToken">The access token for authenticating requests to the Autodesk Forge service.</param>
        /// <returns>An asynchronous task representing the PropDbReader instance with the downloaded and extracted data.</returns>
        private async Task<PropDbReader> DownloadStreamAsync(string? urn, string accessToken)
        {
            List<Derivatives.Resource> resourcesToDownload =
                await Derivatives.ExtractProbDbAsync(urn, accessToken).ConfigureAwait(false);
            List<string> fileNames = new List<string>()
            {
                "objects_ids.json.gz",
                "objects_offs.json.gz",
                "objects_avs.json.gz",
                "objects_attrs.json.gz",
                "objects_vals.json.gz",
            };
            // filter the list of resources to download
            resourcesToDownload = resourcesToDownload.FindAll(r => fileNames.Contains(r.FileName));
            byte[]? _ids = new byte[] { };
            byte[]? _offsets = new byte[] { };
            byte[]? _avs = new byte[] { };
            byte[]? _attrs = new byte[] { };
            byte[]? _vals = new byte[] { };
            foreach (Derivatives.Resource resource in resourcesToDownload)
            {
                // prepare the GET to download the file
                RestRequest request = new RestRequest(resource.RemotePath);
                request.Method = Method.Get;
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("Accept-Encoding", "gzip, deflate");
                if (resource.FileName == "objects_attrs.json.gz")
                {
                    await Derivatives.ExecuteRequestData(request, resource)
                        .ContinueWith((task) => { _attrs = task.Result; })
                        .ConfigureAwait(false);
                }
                else if (resource.FileName == "objects_ids.json.gz")
                {
                    await Derivatives.ExecuteRequestData(request, resource)
                        .ContinueWith((task) => { _ids = task.Result; })
                        .ConfigureAwait(false);
                }
                else if (resource.FileName == "objects_offs.json.gz")
                {
                    await Derivatives.ExecuteRequestData(request, resource)
                        .ContinueWith((task) => { _offsets = task.Result; })
                        .ConfigureAwait(false);
                }
                else if (resource.FileName == "objects_avs.json.gz")
                {
                    await Derivatives.ExecuteRequestData(request, resource)
                        .ContinueWith((task) => { _avs = task.Result; })
                        .ConfigureAwait(false);
                }
                else if (resource.FileName == "objects_vals.json.gz")
                {
                    await Derivatives.ExecuteRequestData(request, resource)
                        .ContinueWith((task) => { _vals = task.Result; })
                        .ConfigureAwait(false);
                }
            }

            if (_ids == null || _offsets == null || _avs == null || _attrs == null || _vals == null)
            {
                throw new ArgumentNullException(
                    $"Error downloading properties, may be the model is not ready or processing");
            }

            ids = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_ids)));
            offsets = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_offsets)));
            avs = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_avs)));
            attrs = JsonConvert.DeserializeObject<object[]>(Encoding.UTF8.GetString(Unzip(_attrs)));
            vals = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_vals)));
            return this;
        }

        /// <summary>
        /// Initialize PropDbReader with byte[]
        /// </summary>
        /// <param name="_ids"></param>
        /// <param name="_offsets"></param>
        /// <param name="_avs"></param>
        /// <param name="_attrs"></param>
        /// <param name="_vals"></param>
        public PropDbReader(byte[] _ids, byte[] _offsets, byte[] _avs, byte[] _attrs, byte[] _vals)
        {
            ids = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_ids)));
            offsets = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_offsets)));
            avs = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(_avs)));
            attrs = JsonConvert.DeserializeObject<object[]>(Encoding.UTF8.GetString(Unzip(_attrs)));
            vals = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(_vals)));
        }

        /// <summary>
        /// Initialize PropDbReader with gzip files
        /// </summary>
        /// <param name="ids_gzip">objects_ids.json.gz</param>
        /// <param name="offsets_gzip">objects_offs.json.gz</param>
        /// <param name="avs_gzip">objects_attrs.json.gz</param>
        /// <param name="attrs_gzip">objects_attrs.json.gz</param>
        /// <param name="vals_gzip">objects_vals.json.gz</param>
        public PropDbReader(string ids_gzip, string offsets_gzip, string avs_gzip, string attrs_gzip, string vals_gzip)
        {
            ids = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(ids_gzip)));
            offsets = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(offsets_gzip)));
            avs = JsonConvert.DeserializeObject<int[]>(Encoding.UTF8.GetString(Unzip(avs_gzip)));
            attrs = JsonConvert.DeserializeObject<object[]>(Encoding.UTF8.GetString(Unzip(attrs_gzip)));
            vals = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Unzip(vals_gzip)));
        }

        /// <summary>
        /// Retrieves all properties of all objects in the model.
        /// </summary>
        /// <returns></returns>
        public Property[] GetAllProperties()
        {
            List<Property> properties = new List<Property>();
            for (int i = 1; i < ids.Length; i++)
            {
                Property[] PropertyInstances = EnumerateProperties(i);
                properties.AddRange(PropertyInstances);
            }

            return properties.ToArray();
        }

        /// <summary>
        /// Enumerates the properties of an object identified by the given ID, using information
        /// from the PropDbReader instance's offsets, avs, attrs, and vals arrays.
        /// </summary>
        /// <param name="id">The ID of the object for which properties are enumerated.</param>
        /// <returns>An array of Property objects representing the properties of the specified object.</returns>
        public Property[] EnumerateProperties(int id)
        {
            List<Property> properties = new List<Property>();

            if (id > 0 && id < offsets.Length)
            {
                //Debug.Log("ID : " + id);
                int avStart = 2 * offsets[id];
                int avEnd = (id == offsets.Length - 1) ? avs.Length : 2 * offsets[id + 1];
                for (int i = avStart; i < avEnd; i += 2)
                {
                    int attrOffset = avs[i];
                    int valOffset = avs[i + 1];
                    var attrObj = attrs[attrOffset];
                    string[] attr = new string[0];
                    if (!(attrObj is int))
                    {
                        attr = JsonConvert.DeserializeObject<string[]>(attrObj.ToString());
                    }

                    var value = vals[valOffset];
                    //yield { name: attr[0], category: attr[1], value };
                    properties.Add(new Property()
                    {
                        Id = ids[id],
                        Name = attr[0],
                        Category = attr[1],
                        DataType = EnumRecords.GetDataType(int.Parse(attr[2])),
                        DataTypeContext = attr[3],
                        Description = attr[4],
                        DisplayName = attr[5],
                        Flags = int.Parse(attr[6]),
                        DisplayPrecision = int.Parse(attr[7]),
                        ForgeParameterId = attr[8],
                        Value = value
                    });
                }
            }

            return properties.ToArray();
        }


        public int GetDbIndexExternalId(string externalId)
        {
            int index = -1;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == externalId)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public string? GetExternalIdByDbId(int id)
        {
            return ids[id] ?? null;
        }

        /// <summary>
        /// Finds "public" properties of given object.
        /// Additional properties like parent-child relationships are not included in the output.
        /// </summary>
        /// <param name="id">id Object ID</param>
        /// <returns name="string">Dictionary of property names and values.</returns>
        public Dictionary<string, string?> GetPublicProperties(int id)
        {
            Dictionary<string, string?> props = new Dictionary<string, string?>();
            Regex rg = new Regex(@"^__\w+__$");
            foreach (var prop in EnumerateProperties(id))
            {
                if (!string.IsNullOrEmpty(prop.Category) && rg.IsMatch(prop.Category))
                {
                    // Skip internal attributes
                }
                else
                {
                    props[prop.Name] = prop.Value;
                }
            }

            return props;
        }

        /// <summary>
        /// Finds "public" and internal properties of given object.
        /// Additional properties like parent-child relationships are not included in the output.
        /// </summary>
        /// <param name="id">id Object ID</param>
        /// <returns name="string">Dictionary of property names and values.</returns>
        public Dictionary<string, string?> GetAllProperties(int id)
        {
            Dictionary<string, string?> props = new Dictionary<string, string?>();
            foreach (var prop in EnumerateProperties(id))
            {
                props[prop.Name] = prop.Value;
            }

            return props;
        }

        /// <summary>
        /// Gets the external ID associated with the specified internal ID.
        /// </summary>
        /// <param name="id">The internal ID for which to retrieve the external ID.</param>
        /// <returns>The external ID corresponding to the given internal ID.</returns>
        public string GetExternalId(int id)
        {
            return ids[id];
        }

        /// <summary>
        /// Just support get Revit ElementId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetElementId(int id)
        {
            // pattent get element id : eg Seating-LAMMHULTS-PENNE-Chair [289790] => 289790
            Regex regex = new Regex(@"\[(.*?)\]");
            string? name = GetName(id);
            Match match = regex.Match(name);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }


        /// <summary>
        /// Retrieves properties grouped by category for the specified internal ID.
        /// </summary>
        /// <param name="id">The internal ID for which to retrieve properties.</param>
        /// <returns>
        /// A dictionary where keys are property categories, and values are lists of key-value pairs
        /// representing properties and their values within each category.
        /// </returns>
        public Dictionary<string, List<KeyValuePair<string, object?>>> GetPropertiesByCategory(int id)
        {
            Dictionary<string, List<KeyValuePair<string, object?>>> properties =
                new Dictionary<string, List<KeyValuePair<string, object?>>>();
            Regex rg = new Regex(@"^__\w+__$");

            List<string> categories = new List<string>();

            List<Property> props = EnumerateProperties(id).ToList();
            foreach (var prop in props)
            {
                if (!string.IsNullOrEmpty(prop.Category))
                {
                    if (rg.IsMatch(prop.Category))
                    {
                        // Skip internal attributes
                    }
                    else
                    {
                        if (!categories.Exists(x => x.Contains(prop.Category)))
                        {
                            categories.Add(prop.Category);
                        }
                    }
                }
            }

            for (int j = 0; j < categories.Count; j++)
            {
                string CategoryPropKey = categories[j];

                var propResult = props.FindAll(x => x.Category == CategoryPropKey);

                List<KeyValuePair<string, object?>> propDictonnary = new List<KeyValuePair<string, object?>>();

                propResult.ForEach((prop) =>
                {
                    //string propKey = prop.DisplayName ?? prop.PropName;
                    string propKey = prop.Name;
                    propDictonnary.Add(new KeyValuePair<string, object?>(propKey, prop.Value));
                });

                properties.Add(CategoryPropKey, propDictonnary);
            }


            return properties;
        }

        /// <summary>
        /// Exports data from a PropDbReader to an Excel file at the specified file path.
        /// </summary>
        /// <param name="filePath">The file path where the Excel file will be created or overwritten.</param>
        /// <param name="sheetName">name of sheet</param>
        /// <returns>The file path of the exported Excel file.</returns>
        public void ExportDataToExcel(string filePath, string sheetName)
        {
            // Create a new Excel package
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<int, string> ParametersDict = new Dictionary<int, string>();
            ParametersDict.Add(1, "DbIndex");
            ParametersDict.Add(2, "ExternalId");
            ParametersDict.Add(3, "Name");
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Add headers to the worksheet
                worksheet.Cells["A1"].Value = "DbIndex";
                worksheet.Cells["B1"].Value = "ExternalId";
                worksheet.Cells["C1"].Value = "Name";
                int rowIndex = 0;
                // Iterate over each ID in propDbReader.ids
                for (int i = 1; i < ids.Length; i++)
                {
                    // Rule to skip children
                    int[] children = GetChildren(i);
                    if (children.Length == 0)
                    {
                        Property[] PropertyInstances = EnumerateProperties(i);
                        // Set dbindex in the first column
                        worksheet.Cells[rowIndex + 2, 1].Value = i;
                        // Set name in the second column
                        worksheet.Cells[rowIndex + 2, 2].Value = GetName(i);
                        // Set externalId in the third column
                        worksheet.Cells[rowIndex + 2, 3].Value = ids[i];
                        foreach (Property enumerateProperty in PropertyInstances.Where(x =>
                                     !string.IsNullOrEmpty(x.DisplayName)))
                        {
                            // Find the column index dynamically
                            int columnIndex = worksheet.GetOrCreateColumnIndex(enumerateProperty,
                                ref ParametersDict);
                            worksheet.Cells[rowIndex + 2, columnIndex].Value = enumerateProperty.Value;
                        }

                        // the reason here because we also need export parameter type
                        int dbTypeId = GetInstanceOf(i).FirstOrDefault();
                        GetAllProperties(dbTypeId);
                        Property[] PropertyTypes = EnumerateProperties(dbTypeId);
                        // Iterate over each property and export to subsequent columns
                        foreach (var enumerateProperty in PropertyTypes.Where(x =>
                                     !string.IsNullOrEmpty(x.DisplayName)))
                        {
                            // Find the column index dynamically
                            int columnIndex = worksheet.GetOrCreateColumnIndex(enumerateProperty,
                                ref ParametersDict);
                            worksheet.Cells[rowIndex + 2, columnIndex].Value = enumerateProperty.Value;
                        }

                        PropertyInstances.ToList().AddRange(PropertyTypes);
                        foreach (var parameter in ParametersDict)
                        {
                            if (parameter.Key > 3 && PropertyInstances.All(x => x.DisplayName != parameter.Value))
                            {
                                worksheet.Cells[rowIndex + 2, parameter.Key].Value = "N/A";
                            }
                        }

                        rowIndex++;
                    }
                }

                worksheet.View.FreezePanes(2, 2);
                worksheet.Cells.AutoFitColumns();
                // get all column name and fill black color, text white
                for (int i = 1; i <= ParametersDict.Count; i++)
                {
                    worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                    worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                package.SaveAs(new System.IO.FileInfo(filePath));
            }
        }


        /// <summary>
        /// Retrieves the child IDs associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which child IDs are retrieved.</param>
        /// <returns>An array of integers representing the child IDs of the specified object.</returns>
        public int[] GetChildren(int id)
        {
            List<int> children = new List<int>();
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__child__")
                {
                    children.Add(int.Parse(prop.Value));
                }
            }

            return children.ToArray();
        }

        public DataTable GetDataByDbId(int dbId)
        {
            DataTable dataTable = new DataTable();
            GetChildrenRecursion(ref dataTable, new[] { dbId });
            return dataTable;
        }

        private void GetChildrenRecursion(ref DataTable dataTable, int[] child)
        {
            if (child.Length == 0) return;
            for (int i = 0; i < child.Length; i++)
            {
                Property[] properties = EnumerateProperties(child[i]);
                if (!dataTable.Columns.Contains("DbId")) dataTable.Columns.Add("DbId");
                if (!dataTable.Columns.Contains("ExternalId")) dataTable.Columns.Add("ExternalId");
                // create rows
                DataRow dataRow = dataTable.NewRow();
                dataRow["DbId"] = child[i];
                dataRow["ExternalId"] = properties.Select(x => x.Id).FirstOrDefault()?.ToString();
                var names = properties.Select(x => x.Name).Distinct().ToList();
                // create columns
                foreach (var name in names)
                {
                    if (!dataTable.Columns.Contains(name))
                    {
                        dataTable.Columns.Add(name);
                    }
                }

                foreach (var property in properties)
                {
                    string columnName = property.Name;
                    if (!dataTable.Columns.Contains(columnName))
                    {
                        dataTable.Columns.Add(columnName);
                    }
                    dataRow[columnName] = property.Value;
                }

                dataTable.Rows.Add(dataRow);
                // recursive
                int[] children = GetChildren(child[i]);
                GetChildrenRecursion(ref dataTable, children);
            }
        }

        /// <summary>
        /// Retrieves the parent IDs associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which parent IDs are retrieved.</param>
        /// <returns>An array of integers representing the parent IDs of the specified object.</returns>
        public int[] GetParent(int id)
        {
            List<int> children = new List<int>();
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__parent__")
                {
                    children.Add(int.Parse(prop.Value));
                }
            }

            return children.ToArray();
        }

        /// <summary>
        /// Retrieves the instance IDs associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which instance IDs are retrieved.</param>
        /// <returns>An array of integers representing the instance IDs of the specified object.</returns>
        public int[] GetInstanceOf(int id)
        {
            List<int> children = new List<int>();
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__instanceof__")
                {
                    children.Add(int.Parse(prop.Value));
                }
            }

            return children.ToArray();
        }

        /// <summary>
        /// Retrieves the name associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which the name is retrieved.</param>
        /// <returns>The name of the specified object, or an empty string if not found.</returns>
        public string? GetName(int id)
        {
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__name__")
                {
                    return prop.Value;
                }
            }

            return "";
        }

        /// <summary>
        /// Retrieves the list of viewable locations associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which viewable locations are retrieved.</param>
        /// <returns>An array of viewable locations, or an empty array if none are found.</returns>
        public string?[] GetViewAbleIn(int id)
        {
            List<string?> children = new List<string?>();
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__viewable_in__")
                {
                    children.Add(prop.Value);
                }
            }

            return children.ToArray();
        }

        /// <summary>
        /// Retrieves the category information associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which category information is retrieved.</param>
        /// <returns>The category value, or an empty string if no category information is found.</returns>
        public string? GetCategory(int id)
        {
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__category__")
                {
                    return prop.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves the document information associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which document information is retrieved.</param>
        /// <returns>The document value, or an empty string if no document information is found.</returns>
        public string? GetDocument(int id)
        {
            foreach (var prop in EnumerateProperties(id))
            {
                if (prop.Category == "__document__")
                {
                    return prop.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves the hyperlink associated with the specified object ID.
        /// </summary>
        /// <param name="id">The ID of the object for which the hyperlink is retrieved.</param>
        /// <returns>The hyperlink value, or an empty string if no hyperlink is found.</returns>
        public string? GetHyperlink(int id)
        {
            foreach (var VARIABLE in EnumerateProperties(id))
            {
                if (VARIABLE.Category == "__hyperlink__")
                {
                    return VARIABLE.Value;
                }
            }

            return string.Empty;
        }
    }


    public class Property
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        /// <summary>
        /// Property category
        /// </summary>
        public string? Category { get; set; }

        public DataType? DataType { get; set; }
        public string? DataTypeContext { get; set; }
        public string? Description { get; set; }
        public string? DisplayName { get; set; }
        public int? Flags { get; set; }
        public int? DisplayPrecision { get; set; }
        public string? ForgeParameterId { get; set; }
        public string? Value { get; set; }
    }

    public enum DataType
    {
        Unknown = 0,
        Boolean = 1,
        Integer = 2,
        Double = 3,
        Blob = 10,
        DbKey = 11,
        String = 20,
        LocalizableString = 21
    }
}