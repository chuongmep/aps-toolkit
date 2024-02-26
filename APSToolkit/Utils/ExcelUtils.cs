// Copyright (c) chuongmep.com. All rights reserved

using System.Data;
using System.Text.RegularExpressions;
using APSToolkit.Database;
using APSToolkit.DesignAutomation;
using OfficeOpenXml;

namespace APSToolkit.Utils;

public static class ExcelUtils
{
    /// <summary>
    /// Exports a two-dimensional array of objects to an Excel file at the specified file path.
    /// </summary>
    /// <param name="objects">The two-dimensional array of objects to be exported.</param>
    /// <param name="filePath">The file path where the Excel file will be created or overwritten.</param>
    public static void ExportDataToExcel(this object[,] objects, string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            // Add a worksheet to the Excel package
            var worksheet = package.Workbook.Worksheets.Add("metadata");
            for (int i = 0; i < objects.GetLength(0); i++)
            {
                for (int j = 0; j < objects.GetLength(1); j++)
                {
                    worksheet.Cells[i + 1, j + 1].Value = objects[i, j];
                }
            }

            // set freeze panes
            worksheet.View.FreezePanes(2, 2);
            worksheet.Cells.AutoFitColumns();
            // set color header background black, text white
            for (int i = 1; i <= objects.GetLength(1); i++)
            {
                worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            package.SaveAs(new System.IO.FileInfo(filePath));
        }
    }

    /// <summary>
    /// Exports a collection of objects to an Excel file at the specified file path.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="data">The collection of objects to be exported.</param>
    /// <param name="filePath">The file path where the Excel file will be created or overwritten.</param>
    /// <param name="sheetName">name of sheet</param>
    public static void ExportDataToExcel<T>(this IEnumerable<T> data, string filePath, string sheetName = "metadata")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            // Add a worksheet to the Excel package
            if (string.IsNullOrEmpty(sheetName))
            {
                sheetName = Guid.NewGuid().ToString();
            }

            if (sheetName == "Sheet1")
                throw new Exception("sheet name can not be Sheet1");
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            var properties = typeof(T).GetProperties();
            // set header name
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }

            // set data
            int rowIndex = 0;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[rowIndex + 2, i + 1].Value = properties[i].GetValue(item);
                }

                rowIndex++;
            }

            worksheet.View.FreezePanes(2, 2);
            worksheet.Cells.AutoFitColumns();
            // tune on filter for all columns
            worksheet.Cells[1, 1, 1, properties.Length].AutoFilter = true;
            // set color header background black, text white
            for (int i = 1; i <= properties.Length; i++)
            {
                worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            package.SaveAs(new System.IO.FileInfo(filePath));
        }
    }

    public static void ExportDataToExcel<T>(this Dictionary<string, IEnumerable<T>> data, string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            foreach (var sheetData in data)
            {
                string sheetName = sheetData.Key;
                if (string.IsNullOrEmpty(sheetName))
                {
                    sheetName = Guid.NewGuid().ToString();
                }

                if (sheetName == "Sheet1")
                    throw new Exception("Sheet name cannot be Sheet1");

                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                var properties = typeof(T).GetProperties();

                // set header name
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
                }

                // set data
                int rowIndex = 0;
                foreach (var item in sheetData.Value)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cells[rowIndex + 2, i + 1].Value = properties[i].GetValue(item);
                    }

                    rowIndex++;
                }

                worksheet.View.FreezePanes(2, 2);
                worksheet.Cells.AutoFitColumns();

                // set color header background black, text white
                for (int i = 1; i <= properties.Length; i++)
                {
                    worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                    worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
            }

            package.SaveAs(new System.IO.FileInfo(filePath));
        }
    }

    /// <summary>
    /// Exports a DataTable to an Excel file at the specified file path.
    /// </summary>
    /// <param name="dataTable">The DataTable to be exported.</param>
    /// <param name="filePath">The file path where the Excel file will be created or overwritten.</param>
    /// <param name="sheetName">name of sheet(default )</param>
    public static void ExportDataToExcel(this DataTable dataTable, string filePath, string sheetName = "metadata")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            // Add a worksheet to the Excel package
            if (sheetName == "Sheet1")
                throw new Exception("sheet name can not be Sheet1");
            if (string.IsNullOrEmpty(sheetName))
            {
                throw new Exception("sheet name can not be null or empty");
            }

            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            // remove column name empty
            for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(dataTable.Columns[i].ColumnName))
                {
                    dataTable.Columns.RemoveAt(i);
                }
            }

            // set header name
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = dataTable.Columns[i].ColumnName;
            }

            // set data
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    object? value = dataTable.Rows[i][j];
                    worksheet.Cells[i + 2, j + 1].Value = GetStringValue(value);
                }

                if (i % 2 == 0)
                {
                    worksheet.Row(i + 2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }

            // set freeze panes
            worksheet.View.FreezePanes(2, 2);
            worksheet.Cells.AutoFitColumns();
            // set color header background black, text white
            for (int i = 1; i <= dataTable.Columns.Count; i++)
            {
                worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            package.SaveAs(new System.IO.FileInfo(filePath));
        }
    }
    private static bool ValidateSheetName(string sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            return false;
        }

        if (sheetName.Length > 31)
        {
            return false;
        }

        if (Regex.IsMatch(sheetName, @"[\\/?*[\]:]"))
        {
            return false;
        }
        if (sheetName == "Sheet1") return false;

        return true;
    }

    public static void ExportDataToExcel(this Dictionary<string, DataTable> data, string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (ExcelPackage excelPackage = new ExcelPackage())
        {
            foreach (KeyValuePair<string, DataTable> keyValuePair in data)
            {
                string sheetName = ResolveRevitWorksheetName(keyValuePair.Key);
                // check sheet name is exist
                if (excelPackage.Workbook.Worksheets.Any(x => x.Name.ToLower() == sheetName.ToLower()))
                {
                    sheetName += "1";
                }

                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
                worksheet.Cells["A1"].LoadFromDataTable(keyValuePair.Value, true);
            }

            //auto fit columns
            foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
            {
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(2, 2);
                for (int i = 1; i <= worksheet.Dimension.Columns; i++)
                {
                    worksheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                    worksheet.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
            }

            excelPackage.SaveAs(new System.IO.FileInfo(filePath));
        }
    }

    /// <summary>
    /// Reads the header column from an Excel file.
    /// </summary>
    /// <param name="filePath">The path of the Excel file.</param>
    /// <param name="sheetName">The name of the sheet to read from.</param>
    /// <returns>A list containing the header column data.</returns>
    /// <exception cref="NullReferenceException">Thrown when the specified sheet name is not found.</exception>
    public static List<string> ReadColumnHeader(string filePath, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                throw new NullReferenceException("Sheet name not found!");
            }
            int colCount = worksheet.Dimension.Columns;
            var data = new List<string>();
            for (int i = 0; i < colCount; i++)
            {
                data.Add(worksheet.Cells[1, i + 1].Value.ToString());
            }

            return data;
        }
    }

    /// <summary>
    /// Resolves and formats a worksheet name based on specific rules.
    /// </summary>
    /// <param name="worksheetName">The original worksheet name to be resolved.</param>
    /// <returns>
    /// A formatted worksheet name based on rules specified for resolving.
    /// </returns>
    private static string ResolveRevitWorksheetName(string worksheetName)
    {
        string softWareName = "Revit";
        string newKey = worksheetName.Trim();
        if (newKey.StartsWith(softWareName) && newKey.Length >= 5 && newKey != softWareName)
        {
            newKey = newKey.Replace(softWareName, string.Empty).Trim();
            if (worksheetName.Length >= 32)
            {
                newKey = newKey.Substring(0, 19) + "..." + newKey.Substring(newKey.Length - 9);
            }
        }

        return newKey;
    }

    /// <summary>
    /// Gets the string representation of an object, handling DBNull, Int64, and byte[].
    /// </summary>
    /// <param name="value">The object for which to get the string representation.</param>
    /// <returns>
    /// The string representation of the object. Returns an empty string for DBNull,
    /// converts Int64 to Int32, and converts byte[] to string using UTF-8 encoding.
    /// </returns>
    private static string? GetStringValue(object? value)
    {
        if (value is DBNull)
        {
            return string.Empty;
        }

        // Check for Int64
        if (value is Int64)
        {
            return Convert.ToInt32(value).ToString();
        }

        // Check for byte[] and convert to string
        if (value is byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        return value.ToString();
    }

    /// <summary>
    /// Gets or creates the column index for a property in the Excel worksheet.
    /// </summary>
    /// <param name="worksheet">The Excel worksheet.</param>
    /// <param name="property">The property for which to get or create the column index.</param>
    /// <param name="ParametersDict"></param>
    /// <returns>The column index for the property in the Excel worksheet.</returns>
    internal static int GetOrCreateColumnIndex(this ExcelWorksheet worksheet, Property property,
        ref Dictionary<int, string> ParametersDict)
    {
        if (ParametersDict.Any(x => x.Value == property.DisplayName))
        {
            return ParametersDict.First(x => x.Value == property.DisplayName).Key;
        }

        int columnIndex = ParametersDict.Count + 1;
        ParametersDict.Add(columnIndex, property.DisplayName);
        worksheet.Cells[1, columnIndex].Value = property.DisplayName;
        return columnIndex;
    }

    public static object[][] ReadDataFromStreamExcel(byte[] stream, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            using (var streamExcel = new System.IO.MemoryStream(stream))
            {
                package.Load(streamExcel);
            }

            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                throw new Exception("Sheet name not found!");
            }

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var data = new object[rowCount - 1][];
            for (int i = 0; i < rowCount - 1; i++)
            {
                data[i] = new object[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    data[i][j] = worksheet.Cells[i + 2, j + 1].Value;
                }
            }

            return data;
        }
    }

    public static object[][] ReadDataFromExcel(string filePath, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                throw new Exception("Sheet name not found!");
            }

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var data = new object[rowCount - 1][];
            for (int i = 0; i < rowCount - 1; i++)
            {
                data[i] = new object[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    data[i][j] = worksheet.Cells[i + 2, j + 1].Value;
                }
            }

            return data;
        }
    }
    public static DataTable ReadDataFromExcelToDataTable(string filePath, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                throw new Exception("Sheet name not found!");
            }

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var dataTable = new DataTable();
            for (int i = 0; i < colCount; i++)
            {
                dataTable.Columns.Add(worksheet.Cells[1, i + 1].Value.ToString());
            }

            for (int i = 0; i < rowCount - 1; i++)
            {
                var row = dataTable.NewRow();
                for (int j = 0; j < colCount; j++)
                {
                    row[j] = worksheet.Cells[i + 2, j + 1].Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }

    public static List<string?> ReadDataByColumnName(string filePath, string sheetName, string columnName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
            {
                throw new Exception("Sheet name not found!");
            }

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var data = new List<string?>();
            for (int i = 0; i < rowCount - 1; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    if (worksheet.Cells[1, j + 1].Value.ToString() == columnName)
                    {
                        data.Add(worksheet.Cells[i + 2, j + 1].Value.ToString());
                    }
                }
            }

            return data;
        }
    }

    public static List<Params> ReadParams(string filePath, string SheetName, string parameterName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet == null)
            {
                throw new Exception("Sheet name not found!");
            }

            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            var data = new List<Params>();
            for (int i = 0; i < rowCount - 1; i++)
            {
                Params param = new Params();
                for (int j = 0; j < colCount; j++)
                {
                    if (worksheet.Cells[1, j + 1].Value.ToString() == "ExternalId")
                    {
                        param.UniqueId = worksheet.Cells[i + 2, j + 1].Value.ToString();
                    }

                    if (worksheet.Cells[1, j + 1].Value.ToString() == "ElementId")
                    {
                        param.ElementId = worksheet.Cells[i + 2, j + 1].Value.ToString();
                    }

                    if (worksheet.Cells[1, j + 1].Value.ToString() == parameterName)
                    {
                        param.ParameterName = parameterName;
                        param.ParameterValue = worksheet.Cells[i + 2, j + 1].Value.ToString();
                    }
                }

                data.Add(param);
            }

            return data;
        }
    }
}