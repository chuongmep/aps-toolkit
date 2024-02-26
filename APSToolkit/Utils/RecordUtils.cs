// Copyright (c) chuongmep.com. All rights reserved

using System.Data;
using ConsoleTables;

namespace APSToolkit.Utils;

public static class RecordUtils
{
    /// <summary>
    /// Writes records in CSV format to the console, including the title column.
    /// </summary>
    /// <typeparam name="T">The type of the records.</typeparam>
    /// <param name="record">The IEnumerable collection of records to be written.</param>
    /// <remarks>
    /// This extension method utilizes ConsoleTable to format and display records in CSV format
    /// with the title column included. The records are written to the console using Markdown formatting.
    /// </remarks>
    public static void WriteConsoleRecord<T>(this IEnumerable<T> record) where T : class
    {
        // write record like csv format to console include title column
        // query.ToList().WriteConsoleRecord();
        ConsoleTable
            .From(record)
            .Configure(o => o.NumberAlignment = Alignment.Left)
            .Configure(o => o.EnableCount = true)
            .Configure(o => o.OutputTo = Console.Out)
            .Write(Format.MarkDown);
    }

    /// <summary>
    /// Writes a DataTable to the console in a tabular format with Markdown formatting.
    /// </summary>
    /// <param name="record">The DataTable to be written to the console.</param>
    /// <remarks>
    /// This extension method converts the DataTable to a ConsoleTable and formats it with Markdown
    /// before displaying it on the console. The formatting includes left-aligned numbers and enables
    /// counting rows. The resulting table is output to the console.
    /// </remarks>
    public static void WriteConsoleRecord(this DataTable record)
    {
        // convert to list enumerable from datatable
        ConsoleTable consoleTable = From(record);
        consoleTable
            .Configure(o => o.NumberAlignment = Alignment.Left)
            .Configure(o => o.EnableCount = true)
            .Configure(o => o.OutputTo = Console.Out)
            .Write(Format.MarkDown);
    }

    /// <summary>
    /// Converts a DataTable to a ConsoleTable for tabular formatting.
    /// </summary>
    /// <param name="dataTable">The DataTable to be converted to a ConsoleTable.</param>
    /// <returns>A ConsoleTable representing the tabular structure of the DataTable.</returns>
    /// <remarks>
    /// This method creates a ConsoleTable and populates it with columns from the DataTable's
    /// DataColumn collection. It then adds rows to the table, converting byte array columns to
    /// Base64 strings for display. The resulting ConsoleTable represents the tabular structure
    /// of the DataTable.
    /// </remarks>
    private static ConsoleTable From(DataTable dataTable)
    {
        var table = new ConsoleTable();

        var columns = dataTable.Columns
            .Cast<DataColumn>()
            .Select(x => x.ColumnName)
            .ToList();

        table.AddColumn(columns);

        foreach (DataRow row in dataTable.Rows)
        {
            var items = row.ItemArray.Select(x => x is byte[] data ? Convert.ToBase64String(data) : x.ToString())
                .ToArray();
            table.AddRow(items);
        }

        return table;
    }
}