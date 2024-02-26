// Copyright (c) chuongmep.com. All rights reserved

namespace APSToolkit
{
    public class WaveFront
    {
        public List<MeshGroup> MeshGroups { get; } = new List<MeshGroup>();
        public List<Material> Materials { get; } = new List<Material>();

        public void Load(string filePath)
        {
            try
            {
                // Get the .mtl file reference from the .obj file
                string mtlFileName = null;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line) || line[0] == '#')
                            continue;

                        string[] parts = line.Split(' ');

                        if (parts[0] == "mtllib")
                        {
                            // Material library reference
                            mtlFileName = parts[1];
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(mtlFileName))
                {
                    // Load the associated .mtl file
                    LoadMaterialFile(Path.Combine(Path.GetDirectoryName(filePath), mtlFileName));
                }

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    MeshGroup currentGroup = null;
                    Material currentMaterial = null; // Track the current material for assignment

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line) || line[0] == '#')
                            continue;

                        string[] parts = line.Split(' ');
                        if (parts[0] == "g")
                        {
                            // Group statement
                            string groupName = string.Join(" ", parts, 1, parts.Length - 1).Trim();
                            currentGroup = new MeshGroup(groupName);
                            MeshGroups.Add(currentGroup);
                            currentMaterial = null; // Reset the current material
                        }
                        else if (parts[0] == "v")
                        {
                            // Vertex line
                            if (parts.Length >= 4 &&
                                float.TryParse(parts[1], out float x) &&
                                float.TryParse(parts[2], out float y) &&
                                float.TryParse(parts[3], out float z))
                            {
                                if (currentGroup != null)
                                {
                                    currentGroup.Vertices.Add(new Vector3(x, y, z));
                                }
                            }
                        }
                        else if (parts[0] == "vn")
                        {
                            // Normal line
                            if (parts.Length >= 4 &&
                                float.TryParse(parts[1], out float nx) &&
                                float.TryParse(parts[2], out float ny) &&
                                float.TryParse(parts[3], out float nz))
                            {
                                if (currentGroup != null)
                                {
                                    currentGroup.Normals.Add(new Vector3(nx, ny, nz));
                                }
                            }
                        }
                        else if (parts[0] == "usemtl")
                        {
                            // Use material statement
                            string materialName = string.Join(" ", parts, 1, parts.Length - 1).Trim();
                            currentMaterial = Materials.Find(m => m.Name == materialName);
                        }
                        else if (parts[0] == "f")
                        {
                            // Face line
                            if (parts.Length >= 4)
                            {
                                List<int> vertexIndices = new List<int>();
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    string[] vertexInfo = parts[i].Split('/');
                                    if (int.TryParse(vertexInfo[0], out int vertexIndex))
                                    {
                                        vertexIndices.Add(vertexIndex);
                                    }
                                }

                                if (vertexIndices.Count >= 3)
                                {
                                    if (currentGroup != null)
                                    {
                                        currentGroup.Faces.Add(new Face(vertexIndices[0], vertexIndices[1], vertexIndices[2]));
                                        currentGroup.Material = currentMaterial; // Assign the current material
                                    }
                                }
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading OBJ file: " + ex.Message);
            }
        }

        private void LoadMaterialFile(string mtlFilePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(mtlFilePath))
                {
                    string line;
                    Material currentMaterial = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line) || line[0] == '#')
                            continue;

                        string[] parts = line.Split(' ');

                        if (parts[0] == "newmtl")
                        {
                            // New material definition
                            string materialName = string.Join(" ", parts, 1, parts.Length - 1).Trim();
                            currentMaterial = new Material(materialName);
                            Materials.Add(currentMaterial);
                        }
                        else if (parts[0] == "Kd" && currentMaterial != null)
                        {
                            // Diffuse color (adjust based on your .mtl format)
                            if (parts.Length >= 4 &&
                                float.TryParse(parts[1], out float r) &&
                                float.TryParse(parts[2], out float g) &&
                                float.TryParse(parts[3], out float b))
                            {
                                currentMaterial.Color = new Color(r, g, b);
                            }
                        }
                        // Add more material properties parsing as needed (e.g., specular color, ambient color, etc.)
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading .mtl file: " + ex.Message);
            }
        }
    }

    public class Vector3
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Face
    {
        public int V1 { get; }
        public int V2 { get; }
        public int V3 { get; }

        public Face(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
    }

    public class Color
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public class Material
    {
        public string Name { get; }
        public int Id { get; set; }
        public Color Color { get; set; } // Adjust based on your .mtl format

        public Material(string name)
        {
            Name = name;
            if (name.Contains("."))
            {
                string[] parts = name.Split('.');
                if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                {
                    Id = id;
                }
            }
            else
            {
                Id = -1;
            }
        }
    }

    public class MeshGroup
    {
        public string Name { get; }
        public int Id { get; set; }
        public List<Vector3> Normals { get; } = new List<Vector3>();
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<Face> Faces { get; } = new List<Face>();
        public Material Material { get; set; }

        public MeshGroup(string name)
        {
            Name = name;
            if (name.Contains("."))
            {
                string[] parts = name.Split('.');
                if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                {
                    Id = id;
                }
            }
            else
            {
                Id = -1;
            }
        }
    }
}
