// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit.Utils;

public static class Base64Convert
{
    public static string ToBase64String(string input)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
        string base64String = System.Convert.ToBase64String(plainTextBytes);
        // replace the + and / characters with - and _ characters
        base64String = base64String.Replace('+', '-');
        base64String = base64String.Replace('/', '_');
        return base64String;
    }
    public static string FromBase64String(string input)
    {
        // replace the - and _ characters with + and / characters
        input = input.Replace('-', '+');
        input = input.Replace('_', '/');
        var base64EncodedBytes = System.Convert.FromBase64String(input);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}