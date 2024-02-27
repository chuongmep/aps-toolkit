// Copyright (c) chuongmep.com. All rights reserved

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using ChoETL;
using Microsoft.Data.Analysis;
using Newtonsoft.Json;
using Parquet;

namespace APSToolkit.Utils;

public static class DataTableUtils
{
    /// <summary>
    /// Gets a comma-separated string representing the sort expression based on the specified column order.
    /// </summary>
    /// <param name="dataTable">The DataTable for which the sort expression is generated.</param>
    /// <param name="columnOrder">An array specifying the order of columns for the sort expression.</param>
    /// <returns>
    /// A comma-separated string representing the sort expression.
    /// If any column in <paramref name="columnOrder"/> does not exist in <paramref name="dataTable"/>,
    /// the non-existent columns are excluded from the result.
    /// </returns>
    public static string GetSortExpression(this DataTable dataTable, string[] columnOrder)
    {
        // Check if all columns in columnOrder exist in the DataTable
        if (columnOrder.All(col => dataTable.Columns.Contains(col)))
        {
            return string.Join(",", columnOrder);
        }

        return string.Join(",", columnOrder.Where(col => dataTable.Columns.Contains(col)));
    }

    /// <summary>
    /// Exports the data from a DataTable to a Parquet file.
    /// </summary>
    /// <param name="dataTable">The DataTable containing the data to export.</param>
    /// <param name="filePath">The path to the Parquet file to be created.</param>
    /// <remarks>
    /// This method uses the ChoParquetWriter to write the DataTable to a Parquet file with Snappy compression.
    /// </remarks>
    public static void ExportToParquet(this DataTable dataTable, string filePath)
    {
        ChoParquetRecordConfiguration configuration = new ChoParquetRecordConfiguration();
        configuration.CompressionMethod = CompressionMethod.Gzip;
        configuration.NullValue = null;
        configuration.NullValueHandling = ChoNullValueHandling.Null;
        configuration.ThrowExceptionIfDynamicPropNotExists = false;
        configuration.ThrowAndStopOnMissingField = false;
        // configuration.UseNestedKeyFormat =  true;
        ChoETLSettings.KeySeparator = '!';
        using ChoParquetWriter parser = new ChoParquetWriter(filePath, configuration);
        //DataTable newTable = dataTable.FixColumnNames();
        parser.Write(dataTable);
    }

    /// <summary>
    /// Exports the DataTable to a Parquet format and returns it as a byte array.
    /// </summary>
    /// <param name="dataTable">The DataTable to export.</param>
    /// <returns>A byte array representing the Parquet data.</returns>
    public static byte[] ExportToParquetStream(this DataTable dataTable)
    {
        ChoParquetRecordConfiguration configuration = new ChoParquetRecordConfiguration();
        configuration.CompressionMethod = CompressionMethod.Gzip;
        configuration.NullValue = null;
        configuration.NullValueHandling = ChoNullValueHandling.Null;
        configuration.ThrowExceptionIfDynamicPropNotExists = false;
        configuration.ThrowAndStopOnMissingField = false;
        // configuration.UseNestedKeyFormat =  true;
        ChoETLSettings.KeySeparator = '!';
        // Create a memory stream
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Use the memory stream as the target stream for ChoParquetWriter
            using (ChoParquetWriter parser = new ChoParquetWriter(memoryStream, configuration))
            {
                // Write DataTable to the stream
                parser.Write(dataTable);
            }
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Fix case column name contains special characters to export parquet
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private static DataTable FixColumnNames(this DataTable dataTable)
    {
        // any column name have character "." will be replaced by "_"
        foreach (DataColumn column in dataTable.Columns)
        {
            column.ColumnName = Regex.Replace(column.ColumnName, @"\.", "_");
        }
        return dataTable;
    }

    /// <summary>
    /// Expand data object to dynamic object
    /// </summary>
    /// <param name="dataTable">DataTable</param>
    /// <returns></returns>
    private static IEnumerable<dynamic> ToDynamic(this DataTable dataTable)
    {
        foreach (DataRow row in dataTable.Rows)
        {
            dynamic expando = new System.Dynamic.ExpandoObject();
            var expandoDict = expando as IDictionary<string, object>;
            foreach (DataColumn col in dataTable.Columns)
            {
                expandoDict[col.ColumnName] = row[col];
            }

            yield return expando;
        }
    }
    /// <summary>
    /// Converts a DataTable to a two-dimensional array of objects.
    /// </summary>
    /// <param name="datatable">The DataTable to be converted.</param>
    /// <returns>A two-dimensional array of objects representing the DataTable.</returns>
    public static object[,] ToArray(this DataTable datatable)
    {
        var ret = new object[datatable.Rows.Count, datatable.Columns.Count];
        for (var i = 0; i < datatable.Rows.Count; i++)
        {
            for (var j = 0; j < datatable.Columns.Count; j++)
            {
                object value = datatable.Rows[i][j];

                // Check for DBNull
                if (value is DBNull)
                {
                    ret[i, j] = string.Empty;
                }
                // Check for Int64
                else if (value is Int64)
                {
                    ret[i, j] = Convert.ToInt32(value);
                }
                // Check for byte[] and convert to string
                else if (value is byte[] bytes)
                {
                    ret[i, j] = System.Text.Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    // Handle other data types as-is
                    ret[i, j] = value;
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// Creates a new DataTable with byte[] columns converted to string and retains other columns as-is.
    /// </summary>
    /// <param name="dataTable">The DataTable to be processed.</param>
    /// <returns>A new DataTable with byte[] columns converted to string.</returns>
    public static DataTable FixBytesValue(this DataTable dataTable)
    {
        var dt = new DataTable();
        foreach (DataColumn column in dataTable.Columns)
        {
            if (column.DataType == typeof(byte[]))
            {
                dt.Columns.Add(column.ColumnName, typeof(string));
            }
            else
            {
                dt.Columns.Add(column.ColumnName, column.DataType);
            }
        }

        foreach (DataRow row in dataTable.Rows)
        {
            var newRow = dt.NewRow();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == typeof(byte[]))
                {
                    newRow[column.ColumnName] = Encoding.UTF8.GetString((byte[])row[column.ColumnName]);
                }
                else
                {
                    newRow[column.ColumnName] = row[column.ColumnName];
                }
            }

            dt.Rows.Add(newRow);
        }

        return dt;
    }

    /// <summary>
    /// Converts a DataTable to a DataFrame.
    /// </summary>
    /// <param name="dataTable">The DataTable to be converted.</param>
    /// <returns>A DataFrame with the same columns and data as the input DataTable.</returns>
    public static DataFrame ToDataFrame(this DataTable dataTable)
    {
        DataFrame dataFrame = new DataFrame();

        foreach (DataColumn column in dataTable.Columns)
        {
            // get values from column cast as string
            string[] values = dataTable.AsEnumerable().Select(r => r.Field<object>(column.ColumnName)?.ToString()).ToArray();
            DataFrameColumn dataFrameColumn = DataFrameColumn.Create(column.ColumnName, values);
            dataFrame.Columns.Add(dataFrameColumn);
        }
        return dataFrame;
    }
}