// Copyright (c) chuongmep.com. All rights reserved

using OfficeOpenXml.Packaging.Ionic.Zlib;

namespace APSToolkit.Utils;

public static class GzipUtils
{
    /// <summary>
    /// Unzips a Gzip-compressed file and returns the decompressed content as a string.
    /// </summary>
    /// <param name="gzipPath">The file path of the Gzip-compressed file.</param>
    /// <returns>The decompressed content as a string.</returns>
    public static string UnzipGzip(string gzipPath)
    {
        using (FileStream fs = new FileStream(gzipPath, FileMode.Open))
        {
            using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    string json = reader.ReadToEnd();
                    return json;
                }
            }
        }
    }

    /// <summary>
    /// Compresses a file using Gzip and returns the path of the compressed file.
    /// </summary>
    /// <param name="filePath">The file path of the file to be compressed.</param>
    /// <returns>The path of the compressed file.</returns>
    public static string ZipFile(string filePath)
    {
        string gzipPath = filePath + ".gz";
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (FileStream gzipStream = File.Create(gzipPath))
            {
                using (GZipStream gzip = new GZipStream(gzipStream, CompressionMode.Compress))
                {
                    fs.CopyTo(gzip);
                }
            }
        }

        return gzipPath;
    }
}