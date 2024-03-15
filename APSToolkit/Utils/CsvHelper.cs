// Copyright (c) chuongmep.com. All rights reserved
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace APSToolkit.Utils;

public static class CsvHelper
{
    /// <summary>
    /// Exports a collection of objects to a CSV file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="data">The collection of objects to be exported.</param>
    /// <param name="path">The file path where the CSV file will be created or overwritten.</param>
    /// <param name="delimiter">The delimiter to be used in the CSV file (e.g., ",", ";", "\t").</param>
    public static void ExportToCsv<T>(this IEnumerable<T> data, string path, string delimiter = "\t")
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            Encoding = Encoding.UTF8,
        };
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, configuration))
        {
            // check if case data is single list with string or int
            if (data is IEnumerable<string> list)
            {
                foreach (var item in list)
                {
                    csv.WriteField(item);
                    csv.NextRecord();
                }
                return;
            }
            if (data is IEnumerable<int> listInt)
            {
                foreach (var item in listInt)
                {
                    csv.WriteField(item);
                    csv.NextRecord();
                }
                return;
            }
            csv.WriteRecords(data);
        }
    }

    /// <summary>
    /// Exports a DataTable to a CSV file at the specified path.
    /// </summary>
    /// <param name="dataTable">The DataTable to be exported.</param>
    /// <param name="path">The file path where the CSV file will be created or overwritten.</param>
    /// <param name="delimiter">The delimiter to be used in the CSV file (e.g., ",", ";", "\t").</param>
    /// <param name="isHeader">Whether to include the column headers in the CSV file.</param>
    public static void ExportToCsv(this DataTable dataTable, string path, string delimiter = "\t", bool isHeader = true)
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            Encoding = Encoding.UTF8,
        };
        object[,] objects = dataTable.ToArray();
        // write header and data
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, configuration))
        {
            if (isHeader)
            {
                for (int i = 0; i < objects.GetLength(1); i++)
                {
                    csv.WriteField(dataTable.Columns[i].ColumnName);
                }

                csv.NextRecord();
            }

            for (int i = 0; i < objects.GetLength(0); i++)
            {
                for (int j = 0; j < objects.GetLength(1); j++)
                {
                    csv.WriteField(objects[i, j]);
                }

                csv.NextRecord();
            }
        }
    }

    /// <summary>
    /// Reads data from a CSV file and returns a collection of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects to be returned. This type must be a class.</typeparam>
    /// <param name="path">The file path of the CSV file to be read.</param>
    /// <param name="delimiter">The delimiter used in the CSV file (e.g., ",", ";", "\t"). Default is "\t".</param>
    /// <returns>A collection of objects of type T.</returns>
    public static IEnumerable<T> ReadFromCsv<T>(string path, string delimiter = "\t") where T : class
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            Encoding = Encoding.UTF8,
        };
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, configuration))
        {
            return csv.GetRecords<T>().ToList();
        }
    }
}