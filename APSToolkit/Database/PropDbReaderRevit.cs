using System.Data;
using System.Text.RegularExpressions;
using APSToolkit.Utils;
using OfficeOpenXml;

// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.Database;

public class PropDbReaderRevit : PropDbReader
{
    public RevitDataConfiguration Configuration { get; set; }
    private const string SoftwareName = "Revit";
    /// <summary>
    /// Retrieves a list of categories related to the specified rule name in the context of Revit.
    /// </summary>
    /// <remarks>
    /// This method filters properties based on the specified rule name and extracts unique categories.
    /// </remarks>
    /// <returns>
    /// A list of category names associated with the specified rule name in the Revit context.
    /// </returns>
    public Dictionary<int, string> GetAllCategories()
    {
        Dictionary<int, string> categories = new Dictionary<int, string>();
        GetRecursiveChild(ref categories, 1, "_RC");
        return categories;
    }
    /// <summary>
    /// Retrieves a list of unique families based on the properties with the name "_RFN".
    /// </summary>
    /// <remarks>
    /// This method filters properties to select values associated with the specified property name "_RFN".
    /// </remarks>
    /// <returns>
    /// A list of unique family names extracted from the properties with the name "_RFN".
    /// </returns>
    public Dictionary<int, string> GetAllFamilies()
    {
        Dictionary<int, string> families = new Dictionary<int, string>();
        GetRecursiveChild(ref families, 1, "_RFN");
        return families;
    }

    /// <summary>
    /// Retrieves a list of unique family types based on the properties with the name "_RFT".
    /// </summary>
    /// <remarks>
    /// This method filters properties to select values associated with the specified property name "_RFT".
    /// </remarks>
    /// <returns>
    /// A list of unique family types extracted from the properties with the name "_RFT".
    /// </returns>
    public Dictionary<int, string> GetAllFamilyTypes()
    {
        Dictionary<int, string> allFamilyTypes = new Dictionary<int, string>();
        GetRecursiveChild(ref allFamilyTypes, 1, "_RFT");
        return allFamilyTypes;
    }
    private void GetRecursiveChild(ref Dictionary<int,string> output, int id,string name)
    {
        int[] children = GetChildren(id);
        for (int i = 0; i < children.Length; i++)
        {
            int child = children[i];
            Property[] docs = EnumerateProperties(child);
            string? property = docs.FirstOrDefault(x => x.Name == name)?.Value;
            if (string.IsNullOrEmpty(property))
            {
                GetRecursiveChild(ref output,child, name);
            }
            else
            {
                output[child] = property.Trim();
            }
        }
    }
    public void ExportAllDataToExcel(string path)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using ExcelPackage excelPackage = new ExcelPackage();
        int[] categories = GetChildren(1);
        for (int i = 0; i < categories.Length; i++)
        {
            string categoryName = EnumerateProperties(categories[i]).FirstOrDefault(x => x.Name == "_RC")?.Value ??
                                  string.Empty;
            if (string.IsNullOrEmpty(categoryName)) continue;
            if (categoryName.ToLower() == "center line") continue;
            if (categoryName.EndsWith(".dwg")) continue;
            // check name larger than 31 characters
            categoryName = ResolveLimitWorksheetName(categoryName);
            if (excelPackage.Workbook.Worksheets.Any(x => x.Name.ToLower() == categoryName.ToLower()))
            {
                //random 5 number from guid generator
                string ran = Guid.NewGuid().ToString().Substring(0, 5);
                categoryName += $"_{ran}";
                categoryName = ResolveLimitWorksheetName(categoryName);
            }

            // create worksheet
            excelPackage.Workbook.Worksheets.Add(categoryName);
            //get revit family Name of category
            int[] cates = GetChildren(categories[i]);
            // start export data
            DataTable dataTable = new DataTable();
            GetChildren(ref dataTable, cates);
            if (dataTable.Rows.Count == 0) continue;
            excelPackage.Workbook.Worksheets[categoryName].Cells["A1"].LoadFromDataTable(dataTable, true);
        }

        // save excel file
        excelPackage.SaveAs(new FileInfo(path));
    }

    public DataTable GetAllDataByParameter(List<string> parameters)
    {
        int[] categories = GetChildren(1);
        DataTable dataTable = new DataTable();
        for (int i = 0; i < categories.Length; i++)
        {
            int[] cates = GetChildren(categories[i]);
            GetChildren(ref dataTable, cates, parameters);
        }

        return dataTable;
    }

    public void ExportAllDataToParquet(string directory)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        int[] categories = GetChildren(1);
        // get location element
        for (int i = 0; i < categories.Length; i++)
        {
            string categoryName = EnumerateProperties(categories[i]).FirstOrDefault(x => x.Name == "_RC")?.Value ??
                                  string.Empty;
            if (string.IsNullOrEmpty(categoryName)) continue;
            if (categoryName.ToLower() == "center line") continue;
            if (categoryName.EndsWith(".dwg")) continue;
            //get revit family Name of category
            int[] cates = GetChildren(categories[i]);
            // start export data
            DataTable dataTable = new DataTable();
            GetChildren(ref dataTable, cates);
            if (dataTable.Rows.Count == 0) continue;
            // remove special character < > : " / \ | ? *
            categoryName = Regex.Replace(categoryName, @"[<>:""/\\|?*]", "");
            string fileName = Path.Combine(directory, categoryName + ".parquet");
            dataTable.ExportToParquet(fileName);
        }
    }

    public DataTable GetAllData()
    {
        int[] categories = GetChildren(1);
        // get location element
        DataTable dataTable = new DataTable();
        for (int i = 0; i < categories.Length; i++)
        {
            int[] cates = GetChildren(categories[i]);
            // start export data
            GetChildren(ref dataTable, cates);
        }

        return dataTable;
    }


    /// <summary>
    /// Export all data to excel by category
    /// </summary>
    /// <param name="path">the path of category</param>
    /// <param name="categoryName">name of category</param>
    /// <param name="sheetName">the sheet name of excel sheet</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void ExportAllDataToExcelByCategory(string path, string categoryName,
        string sheetName)
    {
        if (categoryName.StartsWith(SoftwareName)) categoryName = categoryName.Substring(5).Trim();
        int dbId = GetAllCategories().FirstOrDefault(x => x.Value == categoryName).Key;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using ExcelPackage excelPackage = new ExcelPackage();
        if (string.IsNullOrEmpty(categoryName)) throw new ArgumentNullException(nameof(categoryName));
        // check name larger than 31 characters
        categoryName = ResolveLimitWorksheetName(categoryName);
        if (excelPackage.Workbook.Worksheets.Any(x => x.Name.ToLower() == categoryName.ToLower()))
        {
            //random 5 number from guid generator
            string ran = Guid.NewGuid().ToString().Substring(0, 5);
            categoryName += $"_{ran}";
            categoryName = ResolveLimitWorksheetName(categoryName);
        }

        // create worksheet
        if (string.IsNullOrEmpty(sheetName))
        {
            excelPackage.Workbook.Worksheets.Add(categoryName);
            sheetName = categoryName;
        }
        else
            excelPackage.Workbook.Worksheets.Add(sheetName);

        //get revit family Name of category
        int[] cates = GetChildren(dbId);
        // start export data
        DataTable dataTable = new DataTable();

        GetChildren(ref dataTable, cates);
        if (dataTable.Rows.Count == 0) return;
        excelPackage.Workbook.Worksheets[sheetName].Cells["A1"].LoadFromDataTable(dataTable, true);
        var worksheet = excelPackage.Workbook.Worksheets[sheetName];
        worksheet.View.FreezePanes(2, 2);
        worksheet.Cells.AutoFitColumns();
        // set color row %2==0 background gray
        for (int i = 2; i <= dataTable.Rows.Count + 1; i++)
        {
            if (i % 2 == 0)
            {
                for (int j = 1; j <= dataTable.Columns.Count; j++)
                {
                    worksheet.Cells[i, j].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[i, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }
        }

        // set color header background black, text white
        for (int i = 1; i <= dataTable.Columns.Count; i++)
        {
            worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
            worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // tune on filter column
        worksheet.Cells[1, 1, dataTable.Rows.Count + 1, dataTable.Columns.Count].AutoFilter = true;
        // save excel file
        excelPackage.SaveAs(new FileInfo(path));
    }

    public DataTable GetAllDataByCategory(string categoryName)
    {
        if (categoryName.StartsWith("Revit")) categoryName = categoryName.Substring(5).Trim();
        int dbId = GetAllCategories().FirstOrDefault(x => x.Value == categoryName).Key;
        int[] cates = GetChildren(dbId);
        DataTable dataTable = new DataTable();
        GetChildren(ref dataTable, cates);
        return dataTable;
    }
    /// <summary>
    /// Retrieves all data associated with a given family name from the database.
    /// </summary>
    /// <param name="familyName">The name of the family to retrieve data for.</param>
    /// <returns>A DataTable containing all data related to the specified family.</returns>
    public DataTable GetAllDataByFamily(string familyName)
    {
        int dbId = GetAllFamilies().FirstOrDefault(x => x.Value == familyName).Key;
        int[] cates = GetChildren(dbId);
        DataTable dataTable = new DataTable();
        GetChildren(ref dataTable, cates);
        return dataTable;
    }
    public DataTable GetAllDataByFamilyType(string typeName)
    {
        int dbId = GetAllFamilyTypes().FirstOrDefault(x => x.Value == typeName).Key;
        int[] cates = GetChildren(dbId);
        DataTable dataTable = new DataTable();
        GetChildren(ref dataTable, cates);
        return dataTable;
    }
    /// <summary>
    /// Get all data by revit category and parameters name
    /// </summary>
    /// <param name="categoryName">the category of Revit</param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable GetDataByCategoryAndParameters(string categoryName,List<string> parameters)
    {
        if (categoryName.StartsWith(SoftwareName)) categoryName = categoryName.Substring(5).Trim();
        int dbId = GetAllCategories().FirstOrDefault(x => x.Value == categoryName).Key;
        int[] cates = GetChildren(dbId);
        DataTable dataTable = new DataTable();
        GetChildren(ref dataTable, cates,parameters);
        return dataTable;
    }

    public DataTable GetDataByCategoriesAndParameters(List<string> categories,List<string> parameters)
    {
        // resolve category name
        var newCategories = categories.Select(x => x.StartsWith(SoftwareName) ? x.Substring(5).Trim() : x).ToList();
        DataTable dataTable = new DataTable();
        Dictionary<int,string> allCategories = GetAllCategories()
            .Where(x => newCategories.Contains(x.Value))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (KeyValuePair<int,string> cate in allCategories)
        {
            int[] cates = GetChildren(cate.Key);
            GetChildren(ref dataTable, cates,parameters);
        }

        return dataTable;
    }
    // public DataTable GetAllDataParameterByCategory(string category,List<string> parameters, bool isIncludeType = true, bool isAddUnit = true)
    // {
    //     int[] categories = GetChildren(1);
    //     DataTable dataTable = new DataTable();
    //     for (int i = 0; i < categories.Length; i++)
    //     {
    //         int[] cates = GetChildren(categories[i]);
    //         GetChildren(ref dataTable, cates, isIncludeType, isAddUnit, parameters);
    //     }
    //     return dataTable;
    // }

    private static string ResolveLimitWorksheetName(string worksheetName)
    {
        string newKey = worksheetName.Trim();
        if (worksheetName.Length >= 32)
        {
            // get 5 random characters from guid generator
            newKey = newKey.Substring(0, 19) + "..." + newKey.Substring(newKey.Length - 9);
        }

        return newKey;
    }

    /// <summary>
    /// Recursive get all children of the object
    /// </summary>
    /// <param name="dataTable">datatable</param>
    /// <param name="child">index child</param>
    public void GetChildren(ref DataTable dataTable, int[] child)
    {
        if (child.Length == 0) return;
        for (int i = 0; i < child.Length; i++)
        {
            Property[] properties = EnumerateProperties(child[i]);
            bool flag = properties.FirstOrDefault(x => x.Name == "_RC")?.Value != null;
            if (flag)
            {
                int[] children = GetChildren(child[i]);
                GetChildren(ref dataTable, children);
            }
            else
            {
                if (!dataTable.Columns.Contains("DbId")) dataTable.Columns.Add("DbId");
                if (!dataTable.Columns.Contains("ExternalId")) dataTable.Columns.Add("ExternalId");
                // create rows
                DataRow dataRow = dataTable.NewRow();
                dataRow["DbId"] = child[i];
                dataRow["ExternalId"] = properties.Select(x => x.Id).FirstOrDefault()?.ToString();
                if (Configuration.IsGetBBox)
                {
                    if (!dataTable.Columns.Contains("BBox")) dataTable.Columns.Add("BBox");
                    if (Configuration.Fragments.ContainsKey(child[i]))
                    {
                        float[] bb = Configuration.Fragments[child[i]].bbox;
                        // convert to string with separator ,
                        string bbox = string.Join(",", bb);
                        dataRow["BBox"] = bbox;
                    }
                }

                var names = properties.Select(x => x.Name).Distinct().ToList();
                // create columns
                foreach (var name in names)
                {
                    if (name == "parent" || name == "instanceof_objid") continue;
                    if (!dataTable.Columns.Contains(name))
                    {
                        dataTable.Columns.Add(name);
                    }
                }

                foreach (var property in properties)
                {
                    string columnName = property.Name;
                    if (columnName == "parent") continue;
                    if (columnName == "instanceof_objid" && Configuration.IsGetParameterType)
                    {
                        Property[] types = EnumerateProperties(int.Parse(property.Value));
                        foreach (var type in types)
                        {
                            if (type.Name == "parent") continue;
                            if (!dataTable.Columns.Contains(type.Name))
                            {
                                dataTable.Columns.Add(type.Name);
                            }

                            dataRow[type.Name] = type.Value;
                        }

                        continue;
                    }

                    if (!dataTable.Columns.Contains(columnName))
                    {
                        dataTable.Columns.Add(columnName);
                    }

                    if (Configuration.IsAddUnits && !string.IsNullOrEmpty(property.DataTypeContext))
                    {
                        UnitsData? unitsData = UnitUtils.ParseUnitsData(property.DataTypeContext);
                        if (Configuration.Units.TryGetValue(unitsData.TypeId, out var unit))
                        {
                            dataRow[columnName] = property.Value + " " + unit;
                        }
                        else
                        {
                            dataRow[columnName] = property.Value;
                        }
                    }
                    else
                    {
                        dataRow[columnName] = property.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }
        }
    }

    /// <summary>
    /// Get all data by parameters
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="child"></param>
    /// <param name="parameters"></param>
    public void GetChildren(ref DataTable dataTable, int[] child, List<string> parameters)
    {
        if (child.Length == 0) return;
        for (int i = 0; i < child.Length; i++)
        {
            Property[] properties = EnumerateProperties(child[i]);
            bool flag = properties.FirstOrDefault(x => x.Name == "_RC")?.Value != null;
            if (flag)
            {

                int[] children = GetChildren(child[i]);
                GetChildren(ref dataTable, children,parameters);
            }
            else
            {
                if (!dataTable.Columns.Contains("DbId")) dataTable.Columns.Add("DbId");
                if (!dataTable.Columns.Contains("ExternalId")) dataTable.Columns.Add("ExternalId");
                // create rows
                DataRow dataRow = dataTable.NewRow();
                dataRow["DbId"] = child[i];
                dataRow["ExternalId"] = properties.Select(x => x.Id).FirstOrDefault()?.ToString()!;
                if (Configuration.IsGetBBox)
                {
                    if (!dataTable.Columns.Contains("BBox")) dataTable.Columns.Add("BBox");
                    if (Configuration.Fragments.ContainsKey(child[i]))
                    {
                        float[] bb = Configuration.Fragments[child[i]].bbox;
                        // convert to string with separator ,
                        string bbox = string.Join(",", bb);
                        dataRow["BBox"] = bbox;
                    }
                }
                properties = properties.Where(x => parameters.Contains(x.Name)).ToArray();
                var names = properties.Select(x => x.Name).Distinct().ToList();
                // create columns
                foreach (var name in names)
                {
                    if (name == "parent" || name == "instanceof_objid") continue;
                    if (!dataTable.Columns.Contains(name))
                    {
                        dataTable.Columns.Add(name);
                    }
                }

                foreach (var property in properties)
                {
                    string columnName = property.Name;
                    if (columnName == "parent") continue;
                    if (columnName == "instanceof_objid" && Configuration.IsGetParameterType)
                    {
                        Property[] types = EnumerateProperties(int.Parse(property.Value));
                        foreach (var type in types)
                        {
                            if (type.Name == "parent") continue;
                            if (!dataTable.Columns.Contains(type.Name))
                            {
                                dataTable.Columns.Add(type.Name);
                            }
                            if (Configuration.IsAddUnits && !string.IsNullOrEmpty(property.DataTypeContext))
                            {
                                UnitsData? unitsData = UnitUtils.ParseUnitsData(property.DataTypeContext);
                                if (Configuration.Units.TryGetValue(unitsData.TypeId, out var unit))
                                {
                                    dataRow[type.Name] = property.Value + " " + unit;
                                }
                            }
                            else
                            {
                                dataRow[type.Name] = type.Value;
                            }
                        }

                        continue;
                    }

                    if (!dataTable.Columns.Contains(columnName))
                    {
                        dataTable.Columns.Add(columnName);
                    }

                    if (Configuration.IsAddUnits && !string.IsNullOrEmpty(property.DataTypeContext))
                    {
                        UnitsData? unitsData = UnitUtils.ParseUnitsData(property.DataTypeContext);
                        if (Configuration.Units.TryGetValue(unitsData.TypeId, out var unit))
                        {
                            dataRow[columnName] = property.Value + " " + unit;
                        }
                    }
                    else
                    {
                        dataRow[columnName] = property.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }
        }
    }


    /// <summary>
    /// Retrieves an array of public properties excluding internal categories and reserved names.
    /// </summary>
    /// <remarks>
    /// This method filters properties to exclude those associated with internal categories (__parent__, __child__, __viewable_in__)
    /// and reserved names (_RC, _FRN, _RFT).
    /// </remarks>
    /// <returns>
    /// An array of public properties excluding internal categories and reserved names.
    /// </returns>
    public Property[] GetAllPublicProperties()
    {
        Property[] properties = GetAllProperties()
            .Where(x => x.Category != "__parent__")
            .Where(x => x.Category != "__child__")
            .Where(x => x.Category != "__viewable_in__")
            .Where(x => x.Name != "_RC")
            .Where(x => x.Name != "_FRN")
            .Where(x => x.Name != "_RFT")
            .ToArray();
        return properties;
    }

    public PropDbReaderRevit(string urn, string accessToken) : base(urn, accessToken)
    {
        Configuration = new RevitDataConfiguration(urn, accessToken);
    }

    public PropDbReaderRevit(byte[] _ids, byte[] _offsets, byte[] _avs, byte[] _attrs, byte[] _vals) : base(_ids,
        _offsets, _avs, _attrs, _vals)
    {
    }

    public PropDbReaderRevit(string ids_gzip, string offsets_gzip, string avs_gzip, string attrs_gzip, string vals_gzip)
        : base(ids_gzip, offsets_gzip, avs_gzip, attrs_gzip, vals_gzip)
    {
    }
}