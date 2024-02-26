// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using APSToolkit.Schema;

namespace APSToolkit
{
    public class SvfReader
    {
        /// <summary>
        /// Reads SVF (Simple Vector Format) from the local file system.
        /// </summary>
        /// <param name="svfPath">The path to the main SVF file.</param>
        /// <returns>A Reader instance for accessing SVF data.</returns>
        /// <remarks>
        /// This method reads SVF data from the local file system, allowing for further processing or visualization.
        /// - The svfPath parameter should point to the main SVF file.
        /// - The method automatically resolves and reads additional SVF-related files from the same folder.
        /// - The returned Reader instance provides methods to access and process SVF data.
        /// </remarks>
        private static Reader ReadFromFileSystem(string svfPath)
        {
            string? svfFolderPath = Path.GetDirectoryName(svfPath);

            System.Func<string, byte[]> resolve = (uri) => {
                var buffer = File.ReadAllBytes(Path.Combine(svfFolderPath, uri));
                return buffer;
            };
            return new Reader(svfPath, resolve);
        }


        /// <summary>
        /// Reads SVF (Simple Vector Format) content from a local file.
        /// </summary>
        /// <param name="_svfModelPath">The path to the main SVF file.</param>
        /// <returns>An object representing the SVF content.</returns>
        /// <remarks>
        /// This method reads SVF content from a local file, allowing for further processing or visualization.
        /// - The _svfModelPath parameter should point to the main SVF file.
        /// - The method uses a Reader to access and process SVF data from the specified file.
        /// - The returned ISvfContent object represents the parsed SVF content.
        /// </remarks>
        public static ISvfContent ReadSvf(string _svfModelPath)
        {
            Reader reader = ReadFromFileSystem(_svfModelPath);
            return reader.read();
        }

    }
    
}