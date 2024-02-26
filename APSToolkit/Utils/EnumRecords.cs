// Copyright (c) chuongmep.com. All rights reserved

using APSToolkit.Database;

namespace APSToolkit.Utils;

public static class EnumRecords
{
    /// <summary>
    /// Converts an integer value to the corresponding DataType enum value.
    /// </summary>
    /// <param name="value">The integer value representing the data type.</param>
    /// <returns>The DataType enum value.</returns>
    /// <remarks>
    /// If the provided integer value does not match any enum value, DataType.Unknown is returned.
    /// </remarks>
    public static DataType GetDataType(int value)
    {
        if (Enum.IsDefined(typeof(DataType), value))
        {
            return (DataType) value;
        }

        // Handle the case where the value does not match any enum value.
        return DataType.Unknown;
    }

}