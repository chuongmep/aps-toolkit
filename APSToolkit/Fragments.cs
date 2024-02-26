// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System.Diagnostics;
using APSToolkit.Schema;

namespace APSToolkit
{
    public class Fragments
    {
        /// <summary>
        /// Parses fragments from a binary buffer, typically stored in a file called 'FragmentList.pf'.
        /// </summary>
        /// <param name="buffer">The binary buffer containing fragment information.</param>
        /// <returns>An array of objects representing parsed SVF fragments.</returns>
        /// <remarks>
        /// This method reads fragment information from a binary buffer, such as the one stored in a 'FragmentList.pf' file.
        /// - The buffer parameter should contain binary data representing SVF fragments.
        /// - The method parses the buffer to extract fragment details, including visibility, material ID, geometry ID, transform, bounding box, and database ID.
        /// - The parsed fragments are returned as an array of ISvfFragment objects.
        /// </remarks>
        public static ISvfFragment[] ParseFragments(byte[] buffer)
        {
            List<ISvfFragment> fragments = new List<ISvfFragment>();
            PackFileReader pfr = new PackFileReader(buffer);

            for (int i = 0, len = pfr.NumEntries(); i < len; i++)
            {
                ISvfManifestType? entryType = pfr.SeekEntry(i);
                Debug.Assert(entryType != null);
                Debug.Assert(entryType?.version > 4);

                byte flags = pfr.GetUint8();
                bool visible = (flags & 0x01) != 0;
                int materialId = pfr.GetVarint();
                int geometryId = pfr.GetVarint();
                ISvfTransform? transform = pfr.GetTransform();
                float[] bbox = new float[6] { 0, 0, 0, 0, 0, 0 };
                float[] bboxOffset = new float[3] { 0, 0, 0 };
                if (entryType?.version > 3)
                {
                    if (transform != null && transform?.t != null)
                    {
                        bboxOffset[0] = (float)transform?.t.X;
                        bboxOffset[1] = (float)transform?.t.Y;
                        bboxOffset[2] = (float)transform?.t.Z;
                    }
                }

                for (int j = 0; j < 6; j++)
                {
                    bbox[j] = pfr.GetFloat32() + bboxOffset[j % 3];
                }

                int dbId = pfr.GetVarint();

                ISvfFragment fragment = new ISvfFragment()
                {
                    visible = visible,
                    materialID = materialId,
                    geometryID = geometryId,
                    dbID = dbId,
                    transform = transform,
                    bbox = bbox
                };
                fragments.Add(fragment);
            }

            return fragments.ToArray();
        }
    }
}