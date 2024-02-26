// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System.Diagnostics;
using APSToolkit.Schema;

namespace APSToolkit
{
    public class Meshes
    {
        /// <summary>
        /// Parses meshes from a binary buffer, typically stored in files called '<number>.pf',
        /// referenced in the SVF manifest as an asset of type 'Autodesk.CloudPlatform.PackFile'.
        /// </summary>
        /// <param name="buffer">Binary buffer to parse.</param>
        /// <returns>Instances of parsed meshes, or null values if the mesh cannot be parsed (and to maintain the indices used in <c>IGeometry</c>).
        /// </returns>
        public static IMeshPack[] ParseMeshes(byte[] buffer)
        {
            List<IMeshPack> meshPacks = new List<IMeshPack>();
            PackFileReader pfr = new PackFileReader(buffer);

            for (int i = 0, len = pfr.NumEntries(); i < len; i++)
            {
                ISvfManifestType? entry = pfr.SeekEntry(i);
                Debug.Assert(entry != null);
                Debug.Assert(entry?.version >= 1);

                switch (entry?.type)
                {
                    case "Autodesk.CloudPlatform.OpenCTM":
                        meshPacks.Add(ParseMeshOctm(pfr));
                        break;
                    case "Autodesk.CloudPlatform.Lines":
                        meshPacks.Add(ParseLines(pfr, (int)entry?.version));
                        break;
                    case "Autodesk.CloudPlatform.Points":
                        meshPacks.Add(ParsePoints(pfr, (int)entry?.version));
                        break;
                }
            }

            return meshPacks.ToArray();
        }

        /// <summary>
        /// Parses an OpenCTM mesh from a binary buffer using the OpenCTM format.
        /// </summary>
        /// <param name="pfr">PackFileReader instance.</param>
        /// <returns>An instance of parsed OpenCTM mesh or null if parsing is not supported.</returns>
        private static ISvfMesh? ParseMeshOctm(PackFileReader pfr)
        {
            string fourcc = pfr.GetString(4);
            Debug.Assert(fourcc == "OCTM");

            int version = pfr.GetInt32();
            Debug.Assert(version == 5);

            string method = pfr.GetString(3);
            pfr.GetUint8(); // Read the last 0 char of the RAW or MG2 fourCC

            switch (method)
            {
                case "RAW":
                    return ParseMeshRaw(pfr);
                default:
                    Debug.Write("Unsupported OpenCTM method " + method);
                    return null;
            }
        }

        private static ISvfMesh ParseMeshRaw(PackFileReader pfr)
        {
            // We will create a single ArrayBuffer to back both the vertex and index buffers.
            // The indices will be places after the vertex information, because we need alignment of 4 bytes.

            int vcount = pfr.GetInt32(); // Num of vertices
            int tcount = pfr.GetInt32(); // Num of triangles
            int uvcount = pfr.GetInt32(); // Num of UV maps
            int attrs = pfr.GetInt32(); // Number of custom attributes per vertex
            int flags = pfr.GetInt32(); // Additional flags (e.g., whether normals are present)
            string comment = pfr.GetString(pfr.GetInt32());

            // Indices
            string name = pfr.GetString(4);
            Debug.Assert(name == "INDX");
            uint[] indices = new uint[tcount * 3];
            for (int i = 0; i < tcount * 3; i++)
            {
                indices[i] = pfr.GetUint32();
            }

            // Vertices
            name = pfr.GetString(4);
            Debug.Assert(name == "VERT");
            float[] vertices = new float[vcount * 3];
            System.Numerics.Vector3 min = new System.Numerics.Vector3()
                { X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue };
            System.Numerics.Vector3 max = new System.Numerics.Vector3()
                { X = float.MinValue, Y = float.MinValue, Z = float.MinValue };
            for (int i = 0; i < vcount * 3; i += 3)
            {
                float x = vertices[i] = pfr.GetFloat32();
                float y = vertices[i + 1] = pfr.GetFloat32();
                float z = vertices[i + 2] = pfr.GetFloat32();

                min.X = Math.Min(min.X, x);
                max.X = Math.Max(max.X, x);
                min.Y = Math.Min(min.Y, y);
                max.Y = Math.Max(max.Y, y);
                min.Z = Math.Min(min.Z, z);
                max.Z = Math.Max(max.Z, z);
            }


            // Normals
            float[] normals = null;
            if ((flags & 1) != 0)
            {
                name = pfr.GetString(4);
                Debug.Assert(name == "NORM");
                normals = new float[vcount * 3];
                for (int i = 0; i < vcount; i++)
                {
                    float x = pfr.GetFloat32();
                    float y = pfr.GetFloat32();
                    float z = pfr.GetFloat32();
                    // Make sure the normals have unit length
                    float dot = x * x + y * y + z * z;
                    if (dot != 1.0)
                    {
                        float len = (float)Math.Sqrt(dot);
                        x /= len;
                        y /= len;
                        z /= len;
                    }

                    normals[i * 3] = x;
                    normals[i * 3 + 1] = y;
                    normals[i * 3 + 2] = z;
                }
            }


            // Parse zero or more UV maps
            ISvfUVMap[] uvmaps = new ISvfUVMap[uvcount];
            for (int i = 0; i < uvcount; i++)
            {
                name = pfr.GetString(4);
                Debug.Assert(name == "TEXC");

                ISvfUVMap uvmap = new ISvfUVMap()
                {
                    name = pfr.GetString(pfr.GetInt32()),
                    file = pfr.GetString(pfr.GetInt32()),
                    uvs = new float[vcount * 2]
                };
                for (int j = 0; j < vcount; j++)
                {
                    uvmap.uvs[j * 2] = pfr.GetFloat32();
                    uvmap.uvs[j * 2 + 1] = 1.0f - pfr.GetFloat32();
                }

                uvmaps[i] = uvmap;
            }


            // Parse custom attributes (currently we only support "Color" attrs)
            float[] colors = null;
            if (attrs > 0)
            {
                name = pfr.GetString(4);
                Debug.Assert(name == "ATTR");
                for (int i = 0; i < attrs; i++)
                {
                    string attrName = pfr.GetString(pfr.GetInt32());
                    if (attrName == "Color")
                    {
                        colors = new float[vcount * 4];
                        for (int j = 0; j < vcount; j++)
                        {
                            colors[j * 4] = pfr.GetFloat32();
                            colors[j * 4 + 1] = pfr.GetFloat32();
                            colors[j * 4 + 2] = pfr.GetFloat32();
                            colors[j * 4 + 3] = pfr.GetFloat32();
                        }
                    }
                    else
                    {
                        pfr.Seek(pfr.Offset + vcount * 4);
                    }
                }
            }


            ISvfMesh mesh = new ISvfMesh()
            {
                vcount = vcount,
                tcount = tcount,
                uvcount = uvcount,
                attrs = attrs,
                flags = flags,
                comment = comment,
                uvmaps = uvmaps.ToList(),
                indices = indices,
                vertices = vertices,
                min = min,
                max = max
            };

            if (normals != null)
            {
                mesh.normals = normals;
            }

            if (colors != null)
            {
                mesh.colors = colors;
            }

            return mesh;
        }

        private static float CheckValue(float inputValue)
        {
            if (Math.Abs(inputValue) > 0 && Math.Abs(inputValue) < 0.00000001f)
            {
                Debug.WriteLine("Value replaced");
                return 0f;
            }

            return inputValue;
        }

        private static ISvfLines ParseLines(PackFileReader pfr, int entryVersion)
        {
            Debug.Assert(entryVersion >= 2);

            ushort vertexCount = pfr.GetUint16();
            ushort indexCount = pfr.GetUint16();
            ushort boundsCount = pfr.GetUint16(); // Ignoring for now
            float lineWidth = (entryVersion > 2) ? pfr.GetFloat32() : 1.0f;
            bool hasColors = pfr.GetUint8() != 0;

            ISvfLines lines = new ISvfLines()
            {
                isLines = true,
                vcount = vertexCount,
                lcount = indexCount / 2,
                vertices = new float[vertexCount * 3],
                indices = new ushort[indexCount],
                lineWidth = lineWidth
            };

            // Parse vertices
            for (int i = 0, len = vertexCount * 3; i < len; i++)
            {
                lines.vertices[i] = pfr.GetFloat32();
            }

            // Parse colors
            if (hasColors)
            {
                lines.colors = new float[vertexCount * 3];
                for (int i = 0, len = vertexCount * 3; i < len; i++)
                {
                    lines.colors[i] = pfr.GetFloat32();
                }
            }

            // Parse indices
            for (int i = 0, len = indexCount; i < len; i++)
            {
                lines.indices[i] = pfr.GetUint16();
            }

            // TODO: Parse polyline bounds

            return lines;
        }

        private static ISvfPoints ParsePoints(PackFileReader pfr, int entryVersion)
        {
            Debug.Assert(entryVersion >= 2);

            ushort vertexCount = pfr.GetUint16();
            ushort indexCount = pfr.GetUint16();
            float pointSize = pfr.GetFloat32();
            bool hasColors = pfr.GetUint8() != 0;
            ISvfPoints points = new ISvfPoints()
            {
                isPoints = true,
                vcount = vertexCount,
                vertices = new float[vertexCount * 3],
                pointSize = pointSize
            };

            // Parse vertices
            for (int i = 0, len = vertexCount * 3; i < len; i++)
            {
                points.vertices[i] = pfr.GetFloat32();
            }

            // Parse colors
            if (hasColors)
            {
                points.colors = new float[vertexCount * 3];
                for (int i = 0, len = vertexCount * 3; i < len; i++)
                {
                    points.colors[i] = pfr.GetFloat32();
                }
            }

            return points;
        }
    }
}