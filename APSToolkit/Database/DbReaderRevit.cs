// Copyright (c) chuongmep.com. All rights reserved

using System.Data;
using System.Text.RegularExpressions;
using APSToolkit.Utils;

namespace APSToolkit.Database;

public class DBReaderRevit : DbReader
{
    /// <summary>
    /// Retrieves a list of all categories present in the model.
    /// </summary>
    /// <returns>A List of nullable strings representing all categories.</returns>
    public List<string?> GetAllCategories()
    {
        string sqlQuery = @"
    SELECT DISTINCT _objects_val.value AS value
    FROM _objects_eav
        INNER JOIN _objects_id ON _objects_eav.entity_id = _objects_id.id
        INNER JOIN _objects_attr ON _objects_eav.attribute_id = _objects_attr.id
        INNER JOIN _objects_val ON _objects_eav.value_id = _objects_val.id
    WHERE _objects_attr.name LIKE '_RC'
        AND _objects_val.value IS NOT NULL
        AND _objects_val.value <> ''
        AND LENGTH(_objects_id.external_id) = 45
    ORDER BY value
";
        // update for get
        DataTable dataTable = this.ExecuteQuery(sqlQuery);
        // get data at first column
        List<string?> categories = dataTable.AsEnumerable().Select(x => x[0].ToString()).ToList();
        return categories;
    }

    /// <summary>
    /// Retrieves a list of Element Properties based on the specified property category.
    /// </summary>
    /// <param name="category">The property category for which Element Properties need to be retrieved.
    /// e.g: Identity Data, Text, Phasing,...</param>
    /// <returns>A List of <see cref="Property"/> objects containing information such as ID, Category, Name, Value, DataType, DataTypeContext,
    /// Description, DisplayName, DisplayPrecision, ForgeParameterId, and Flags.</returns>
    public List<Property> GetPropertyByCategory(string category)
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where attribute.category == category
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
        var result = query.Any() ? query.OrderBy(x => x.Name).ToList() : query.ToList();
        return result;
    }

    /// <summary>
    /// Retrieves a list of properties based on the specified property name.
    /// </summary>
    /// <param name="propertyName">The parameter name for which properties need to be retrieved.</param>
    /// <returns>A List of <see cref="Property"/> objects containing information such as ID, Category, Name, Value, DataType, DataTypeContext,
    /// Description, DisplayName, DisplayPrecision, ForgeParameterId, and Flags.</returns>
    public List<Property> GetPropertyByName(string propertyName)
    {
        var query = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where attribute.display_name != string.Empty && attribute.display_name == propertyName
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

        var result = query.Any() ? query.OrderBy(x => x.Name).ToList() : query.ToList();
        return result;
    }

    /// <summary>
    /// Retrieves external IDs associated with the specified Revit category.
    /// e.g : Doors, Windows, Furniture, etc.
    /// </summary>
    /// <param name="categoryName">The Revit category for which external IDs need to be retrieved.</param>
    /// <returns>An IEnumerable of strings representing uniqueId of elements.</returns>
    public IEnumerable<string> GetExternalIdByRevitCategory(string categoryName)
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
        var result = query.Where(x => x.Name.Contains("Category") && x.Value.Contains($"Revit {categoryName}"));
        return result.Select(x => x.Id);
    }

    /// <summary>
    /// Retrieves public properties associated with the specified Revit category.
    /// e.g : Doors, Windows, Furniture, etc.
    /// </summary>
    /// <param name="revitCategory">The Revit category for which properties need to be retrieved.</param>
    /// <param name="isIncludeType">A flag indicating whether to include type properties.</param>
    /// <returns>
    /// An IEnumerable of <see cref="Property"/> objects containing information such as ID, Category, Name, Value, DataType, DataTypeContext,
    /// Description, DisplayName, DisplayPrecision, ForgeParameterId, and Flags.
    /// </returns>
    public IEnumerable<Property> GetPublicPropertiesByRevitCategory(string revitCategory, bool isIncludeType)
    {
        var parameters = from eav in objectsEavs
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
        var result = parameters.Where(x => x.Name.Contains("Category") && x.Value.Contains($"Revit {revitCategory}"));
        List<string> externalIds = result.Select(x => x.Id).ToList();
        var instanceParameters = parameters.Where(x => externalIds.Contains(x.Id)).ToList();
        if (isIncludeType)
        {
            foreach (string externalId in externalIds)
            {
                //TODO : It too slow, need to improve
                // May be get list dbid and export together with instance id
                string? instanceOf = parameters.Where(x => x.Id == externalId)
                    .FirstOrDefault(x => x.Category == "__instanceof__").Value;
                IEnumerable<Property> types = GetPublicPropertiesByDbId(int.Parse(instanceOf));
                // TODO : Still keep family symbol id is better ?
                // How to filter data ?
                types.ToList().ForEach(x => x.Id = externalId);
                if (types.Any(x => x.DisplayName == "Workset"))
                {
                    // replace to Workset(Type)
                    types.ToList().ForEach(x => x.DisplayName = "Workset(Type)");
                }

                // set external override for id
                instanceParameters.AddRange(types);
            }
        }

        string pattern = @"^__\w+__$";
        instanceParameters = instanceParameters
            .Where(x => !Regex.IsMatch(x.Category, pattern))
            .ToList();
        // filter parameters have id contains in externalIds
        return instanceParameters.OrderBy(x => x.Id).ThenBy(x => x.DisplayName);
    }

    /// <summary>
    /// Retrieves properties associated with the specified Revit category.
    /// e.g : Doors, Windows, Furniture, etc.
    /// </summary>
    /// <param name="revitCategory">The Revit category for which properties need to be retrieved.</param>
    /// <param name="isIncludeType">A flag indicating whether to include type properties.</param>
    /// <returns>
    /// An IEnumerable of <see cref="Property"/> objects containing information such as ID, Category, Name, Value, DataType, DataTypeContext,
    /// Description, DisplayName, DisplayPrecision, ForgeParameterId, and Flags.
    /// </returns>
    public IEnumerable<Property> GetPropertiesByRevitCategory(string revitCategory, bool isIncludeType)
    {
        var parameters = from eav in objectsEavs
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
        var result = parameters.Where(x => x.Name.Contains("Category") && x.Value.Contains($"Revit {revitCategory}"));
        List<string> externalIds = result.Select(x => x.Id).ToList();
        var instanceParameters = parameters.Where(x => externalIds.Contains(x.Id)).ToList();
        if (isIncludeType)
        {
            foreach (string externalId in externalIds)
            {
                int dbId = GetInstanceOf(externalId).FirstOrDefault();
                IEnumerable<Property> types = GetPropertiesByDbId(dbId);
                instanceParameters.AddRange(types);
            }
        }

        return instanceParameters.OrderBy(x => x.Id).ThenBy(x => x.DisplayName);
    }

    /// <summary>
    /// Retrieves data from the specified Revit category and organizes it into a DataTable.
    /// </summary>
    /// <param name="revitCategory">The name of the Revit category to retrieve data for.</param>
    /// <param name="isIncludeType">
    ///   Indicates whether to include type information in the retrieved data.
    /// </param>
    /// <returns>
    ///   A DataTable containing data from the specified Revit category, formatted based on the provided rules.
    /// </returns>
    public DataTable GetDataByRevitCategory(string revitCategory, bool isIncludeType)
    {
        IEnumerable<Property> properties = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where !attribute.category.Contains("__parent__")
                  && !attribute.category.Contains("__viewable_in__")
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
        properties = properties.ToArray();
        List<string> externalIds = properties
            .Where(x => x.Name.Contains("Category") && x.Value.Contains($"Revit {revitCategory}"))
            .Select(x => x.Id)
            .Distinct()
            .ToList();
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("ExternalId");
        foreach (string externalId in externalIds)
        {
            DataRow dataRow = dataTable.NewRow();
            dataRow["ExternalId"] = externalId;
            List<Property> instanceProperties = properties.Where(x => x.Id == externalId)
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToList();
            foreach (Property instanceProperty in instanceProperties)
            {
                if (instanceProperty.Category.Contains("__instanceof__") && isIncludeType)
                {
                    string? instanceOf = instanceProperty.Value;
                    _object_id objectId = objectIds[int.Parse(instanceOf) - 1];
                    string? externalIdOf = objectId.external_id;
                    List<Property> typeProperties = properties.Where(x => x.Id == externalIdOf)
                        .Where(x => !string.IsNullOrEmpty(x.Name))
                        .Where(x => x.Category != "__instanceof__")
                        .ToList();
                    foreach (Property typeProperty in typeProperties)
                    {
                        if (typeProperty.Name == "Workset")
                        {
                            typeProperty.Name = "Workset(Type)";
                        }

                        string typeColumnName = typeProperty.Name;
                        if (!dataTable.Columns.Contains(typeColumnName))
                        {
                            dataTable.Columns.Add(typeColumnName);
                        }

                        dataRow[typeColumnName] = typeProperty.Value;
                    }
                }

                string columnName = instanceProperty.Name;
                if (!dataTable.Columns.Contains(columnName))
                {
                    dataTable.Columns.Add(columnName);
                }

                dataRow[instanceProperty.Name] = instanceProperty.Value;
            }

            dataTable.Rows.Add(dataRow);
        }

        string[] columnOrder = {"ExternalId", "ElementId", "name", "Category", "CategoryId"};
        string sortExpression = dataTable.GetSortExpression(columnOrder);
        dataTable.DefaultView.Sort = sortExpression;
        dataTable = dataTable.DefaultView.ToTable();
        return dataTable;
    }

    /// <summary>
    /// Retrieves all data related to Revit categories and organizes it into a dictionary of DataTables,
    /// where each DataTable represents data for a specific Revit category.
    /// </summary>
    /// <param name="isIncludeType">
    ///   Indicates whether to include type information in the exported data.
    /// </param>
    /// <returns>
    ///   A dictionary where each key represents a Revit category and the corresponding value is a DataTable
    ///   containing data related to that category.
    /// </returns>
    public Dictionary<string, DataTable> GetAllDataGroupByRevitCategory(bool isIncludeType)
    {
        Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();
        IEnumerable<Property> properties = from eav in objectsEavs
            join objectId in objectIds on eav.entity_id equals objectId.id
            join attribute in objectAttrs on eav.attribute_id equals attribute.id
            join value in objectVals on eav.value_id equals value.id
            where !attribute.category.Contains("__parent__")
                  && !attribute.category.Contains("__viewable_in__")
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
        properties = properties.ToArray();
        List<string?> categoriesExclude = new List<string?>()
        {
            "Revit Center Line",
            "Revit Center line",
            "Revit Lines",
            "Revit <Sketch>",
            "Revit Matchline",
            "Revit Analysis Display Style",
            "Revit Reference Planes",
            "Revit Scope Boxes",
            "Revit Section Boxes",
            "Revit Work Plane Grid",
            "",
        };
        Dictionary<string, string> categories = properties
            .Where(x => x.Name.Equals("Category")
                        && x.Value.Contains("Revit")
                        && !x.Value.Trim().Equals("Revit")
                        && !categoriesExclude.Contains(x.Value))
            .ToDictionary(x => x.Id, x => x.Value);
        // group dictionary by value
        var groupByCategory = categories.GroupBy(x => x.Value);
        foreach (var group in groupByCategory)
        {
            string revitCategory = group.Key;
            List<string> externalIds = group.Select(x => x.Key).ToList();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ExternalId");
            foreach (string externalId in externalIds)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["ExternalId"] = externalId;
                List<Property> instanceProperties = properties.Where(x => x.Id == externalId)
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .ToList();
                foreach (Property instanceProperty in instanceProperties)
                {
                    if (instanceProperty.Category.Equals("__instanceof__") && isIncludeType)
                    {
                        string? instanceOf = instanceProperty.Value;
                        _object_id objectId = objectIds[int.Parse(instanceOf) - 1];
                        string? externalIdOf = objectId.external_id;
                        List<Property> typeProperties = properties.Where(x => x.Id == externalIdOf)
                            .Where(x => !string.IsNullOrEmpty(x.Name))
                            .Where(x => x.Category != "__instanceof__")
                            .ToList();
                        foreach (Property typeProperty in typeProperties)
                        {
                            if (typeProperty.Name == "Workset")
                            {
                                typeProperty.Name = "Workset(Type)";
                            }

                            string typeColumnName = typeProperty.Name;
                            if (!dataTable.Columns.Contains(typeColumnName))
                            {
                                dataTable.Columns.Add(typeColumnName);
                            }

                            dataRow[typeColumnName] = typeProperty.Value;
                        }
                    }

                    else
                    {
                        string columnName = instanceProperty.Name;
                        if (!dataTable.Columns.Contains(columnName))
                        {
                            dataTable.Columns.Add(columnName);
                        }
                        dataRow[instanceProperty.Name] = instanceProperty.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }

            string[] columnOrder = {"ExternalId", "ElementId", "name", "Category", "CategoryId"};
            string sortExpression = dataTable.GetSortExpression(columnOrder);
            dataTable.DefaultView.Sort = sortExpression;
            dataTable = dataTable.DefaultView.ToTable();
            result.Add(revitCategory, dataTable);
        }

        return result;
    }

    /// <summary>
    /// Exports data from a specified Revit category to a CSV file.
    /// </summary>
    /// <param name="revitCategory">The name of the Revit category whose data is to be exported.</param>
    /// <param name="filePath">The path to the CSV file to be created or overwritten.</param>
    /// <param name="isIncludeType">
    ///   Indicates whether to include type information in the exported data.
    /// </param>
    public void ExportDataToCsv(string revitCategory, string filePath, bool isIncludeType)
    {
        revitCategory = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(revitCategory);
        DataTable data = GetDataByRevitCategory(revitCategory, isIncludeType);
        data.ExportToCsv(filePath);
    }

    /// <summary>
    /// Exports data from a specified Revit category to an Excel file.
    /// </summary>
    /// <param name="revitCategory">The name of the Revit category whose data is to be exported.</param>
    /// <param name="filePath">The path to the Excel file to be created or overwritten.</param>
    /// <param name="sheetName">The name of the Excel sheet where the data will be exported.</param>
    /// <param name="isIncludeType">
    ///   (Optional) Indicates whether to include type information in the exported data.
    ///   Default is <c>false</c>.
    /// </param>
    public void ExportDataToExcel(string revitCategory, string filePath, string sheetName, bool isIncludeType = false)
    {
        revitCategory = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(revitCategory);
        DataTable data = GetDataByRevitCategory(revitCategory, isIncludeType);
        data.ExportDataToExcel(filePath, sheetName);
    }

    /// <summary>
    /// Exports all data grouped by Revit category to an Excel file.
    /// </summary>
    /// <param name="filePath">The path to the Excel file to be created or overwritten.</param>
    /// <param name="isIncludeType">
    ///   (Optional) Indicates whether to include type information in the exported data.
    ///   Default is <c>false</c>.
    /// </param>
    public void ExportAllDataToExcel(string filePath,bool isIncludeType = false)
    {
        Dictionary<string,DataTable> dataTables = GetAllDataGroupByRevitCategory(isIncludeType);
        dataTables.ExportDataToExcel(filePath);
    }


    public DBReaderRevit(List<_object_id> objectIds, List<_objects_attr> objectAttrs, List<_objects_eav> objectsEavs,
        List<_object_val> objectVals) : base(objectIds, objectAttrs, objectsEavs, objectVals)
    {
    }

    public DBReaderRevit(string databasePath) : base(databasePath)
    {
    }

    public DBReaderRevit(string? urn, string token) : base(urn, token)
    {
    }
}