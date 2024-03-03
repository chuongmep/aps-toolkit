using System.Data;
using ChoETL;

namespace APSToolkit.Utils;

public static class DataFrame
{

    /// <summary>
    /// Loads a DataFrame from a DataTable.
    /// </summary>
    /// <param name="dataTable">The DataTable to be converted into a DataFrame.</param>
    /// <returns>A DataFrame that represents the provided DataTable.</returns>
    public static Microsoft.Data.Analysis.DataFrame LoadFromDataTable(DataTable dataTable)
    {
        Microsoft.Data.Analysis.DataFrame dataFrame = dataTable.ToDataFrame();
        return dataFrame;
    }

    /// <summary>
    /// Loads a DataFrame from a Parquet file.
    /// </summary>
    /// <param name="filePath">The path to the Parquet file.</param>
    /// <returns>A DataFrame that represents the data in the Parquet file.</returns>
    public static Microsoft.Data.Analysis.DataFrame LoadFromParquet(string filePath)
    {
        using (var r = new ChoParquetReader(filePath))
        {
            var dataFrame = r.AsDataTable();
            return dataFrame.ToDataFrame();
        }
    }

    /// <summary>
    /// Loads a DataFrame from a Parquet file represented as a byte array.
    /// </summary>
    /// <param name="stream">The byte array representing the Parquet file.</param>
    /// <returns>A DataFrame that represents the data in the Parquet file.</returns>
    public static Microsoft.Data.Analysis.DataFrame LoadFromParquet(byte[] stream)
    {
        Stream s = new MemoryStream(stream);
        using (var r = new ChoParquetReader(s))
        {
            var dataFrame = r.AsDataTable();
            return dataFrame.ToDataFrame();
        }
    }

    /// <summary>
    /// Loads a DataFrame from an Excel file.
    /// </summary>
    /// <param name="filePath">The path to the Excel file.</param>
    /// <param name="sheetName">The name of the sheet in the Excel file to load.</param>
    /// <returns>A DataFrame that represents the data in the specified sheet of the Excel file.</returns>
    public static Microsoft.Data.Analysis.DataFrame LoadFromExcel(string filePath, string sheetName)
    {
        DataTable dt = ExcelUtils.ReadDataFromExcelToDataTable(filePath, sheetName);
        return dt.ToDataFrame();
    }

    public static void ExportToExcel(Microsoft.Data.Analysis.DataFrame dataFrame,string filePath, string sheetName)
    {
        DataTable dt = dataFrame.ToTable();
        dt.ExportDataToExcel(filePath, sheetName);
    }
    public static void ExportToCsv(Microsoft.Data.Analysis.DataFrame dataFrame,string filePath)
    {
        DataTable dt = dataFrame.ToTable();
        dt.ExportToCsv(filePath);
    }
    public static void ExportToParquet(Microsoft.Data.Analysis.DataFrame dataFrame,string filePath)
    {
        DataTable dt = dataFrame.ToTable();
        dt.ExportToParquet(filePath);
    }
}