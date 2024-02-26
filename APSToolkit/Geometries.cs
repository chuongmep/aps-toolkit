// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System.Diagnostics;
using APSToolkit.Schema;

namespace APSToolkit
{
    public class Geometries
    {
        /// <summary>
        /// Parses geometries from a binary buffer, typically stored in a file called 'GeometryMetadata.pf',
        /// which is referenced in the SVF manifest as an asset of type 'Autodesk.CloudPlatform.GeometryMetadataList'.
        /// </summary>
        /// <param name="buffer">The binary buffer containing geometry metadata information.</param>
        /// <returns>An array of objects representing parsed SVF geometry metadata.</returns>
        /// <remarks>
        /// This method reads geometry metadata information from a binary buffer, such as the one stored in a 'GeometryMetadata.pf' file.
        /// - The buffer parameter should contain binary data representing SVF geometry metadata.
        /// - The method parses the buffer to extract geometry details, including fragment type, primitive count, pack ID, and entity ID.
        /// - The parsed geometries are returned as an array of ISvfGeometryMetadata objects.
        /// </remarks>
        public static ISvfGeometryMetadata[] ParseGeometries(byte[] buffer)
        {
            List<ISvfGeometryMetadata> geometries = new List<ISvfGeometryMetadata>();

            PackFileReader pfr = new PackFileReader(buffer);
            for (int i = 0, len = pfr.NumEntries(); i < len; i++)
            {
                ISvfManifestType? entry = pfr.SeekEntry(i);
                Debug.Assert(entry != null);
                Debug.Assert(entry?.version >= 3);

                byte fragType = pfr.GetUint8();
                // Skip past object space bbox -- we don't use that
                pfr.Seek(pfr.Offset + 24);
                ushort primCount = pfr.GetUint16();
                string pId = pfr.GetString(pfr.GetVarint()).Replace(".pf", "");
                int packId = Int32.Parse(pId);
                int entityId = pfr.GetVarint();

                ISvfGeometryMetadata geometry = new ISvfGeometryMetadata()
                {
                    fragType = fragType,
                    primCount = primCount,
                    packID = packId,
                    entityID = entityId
                };

                geometries.Add(geometry);
            }

            return geometries.ToArray();
        }
    }
}