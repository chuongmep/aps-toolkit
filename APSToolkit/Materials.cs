// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved

using System.Diagnostics;
using APSToolkit.Schema;
using Newtonsoft.Json;

namespace APSToolkit
{
    public class Materials
    {
        /// <summary>
        /// Parses materials from a binary buffer, typically stored in a file called 'Materials.json.gz',
        /// which is referenced in the SVF manifest as an asset of type 'ProteinMaterials'.
        /// </summary>
        /// <param name="buffer">Binary buffer containing material information.</param>
        /// <returns>
        /// An array of instances representing parsed materials, or null if there are none or they are not supported.
        /// </returns>
        /// <remarks>
        /// This method reads material information from a binary buffer, such as the one stored in a 'Materials.json.gz' file.
        /// - The buffer parameter should contain binary data representing compressed material information.
        /// - The method decompresses the buffer and parses the JSON content to extract material details.
        /// - Parsed materials are returned as an array of ISvfMaterial instances.
        /// - If the material definition is unsupported, a debug message is logged.
        /// - The returned array may contain null values for unsupported or unparseable materials.
        /// </remarks>
        public static ISvfMaterial?[] ParseMaterials(byte[] buffer)
        {
            List<ISvfMaterial?> materials = new List<ISvfMaterial?>();

            buffer = PackFileReader.DecompressBuffer(buffer);

            if (Buffer.ByteLength(buffer) > 0)
            {
                string json = System.Text.Encoding.Default.GetString(buffer);
                ISvfMaterials svfMat = JsonConvert.DeserializeObject<ISvfMaterials>(json);
                foreach (var item in svfMat.materials)
                {
                    ISvfMaterialGroup group = item.Value;
                    ISvfMaterial material = group.materials[group.userassets[0]];
                    switch (material.definition)
                    {
                        case "SimplePhong":
                            materials.Add(ParseSimplePhongMaterial(group));
                            break;
                        default:
                            Debug.WriteLine("Unsupported material definition " + material.definition);
                            break;
                    }
                }
            }

            return materials.ToArray();
        }

        /// <summary>
        /// Parses a SimplePhong material from the provided material group.
        /// </summary>
        /// <param name="group">The material group containing SimplePhong material properties.</param>
        /// <returns>An instance of ISvfMaterial representing the parsed SimplePhong material.</returns>
        private static ISvfMaterial ParseSimplePhongMaterial(ISvfMaterialGroup group)
        {
            ISvfMaterial result = new ISvfMaterial();
            ISvfMaterial material = group.materials[group.userassets[0]];

            result.diffuse = ParseColorProperty(material, "generic_diffuse", new float[4] { 0, 0, 0, 1 });
            result.specular = ParseColorProperty(material, "generic_specular", new float[4] { 0, 0, 0, 1 });
            result.ambient = ParseColorProperty(material, "generic_ambient", new float[4] { 0, 0, 0, 1 });
            result.emissive = ParseColorProperty(material, "generic_emissive", new float[4] { 0, 0, 0, 1 });

            result.glossiness = ParseScalarProperty(material, "generic_glossiness", 30);
            result.reflectivity = ParseScalarProperty(material, "generic_reflectivity_at_0deg", 0);
            result.opacity = 1.0f - ParseScalarProperty(material, "generic_transparency", 0);

            result.metal = ParseBooleanProperty(material, "generic_is_metal", false);

            if (material.textures.Count > 0)
            {
                ISvfMaterialMaps maps = new ISvfMaterialMaps();
                ISvfMaterialMap? diffuse = ParseTextureProperty(material, group, "generic_diffuse");
                if (diffuse != null)
                {
                    maps.diffuse = diffuse;
                }

                ISvfMaterialMap? specular = ParseTextureProperty(material, group, "generic_specular");
                if (specular != null)
                {
                    maps.specular = specular;
                }

                ISvfMaterialMap? alpha = ParseTextureProperty(material, group, "generic_alpha");
                if (alpha != null)
                {
                    maps.alpha = alpha;
                }

                ISvfMaterialMap? bump = ParseTextureProperty(material, group, "generic_bump");
                if (bump != null)
                {
                    if (ParseBooleanProperty(material, "generic_bump_is_normal", false))
                    {
                        maps.normal = bump;
                    }
                    else
                    {
                        maps.bump = bump;
                    }
                }

                result.maps = maps;
            }

            return result;
        }


        private static bool ParseBooleanProperty(ISvfMaterial material, string prop, bool defaultValue)
        {
            if (material.properties.booleans != null && material.properties.booleans.ContainsKey(prop))
            {
                return material.properties.booleans[prop];
            }
            else
            {
                return defaultValue;
            }
        }


        private static float ParseScalarProperty(ISvfMaterial material, string prop, float defaultValue)
        {
            if (material.properties.scalars != null && material.properties.scalars.ContainsKey(prop))
            {
                return material.properties.scalars[prop].values[0];
            }
            else
            {
                return defaultValue;
            }
        }

        private static float[] ParseColorProperty(ISvfMaterial material, string prop, float[] defaultValue)
        {
            if (material.properties.colors != null && material.properties.colors.ContainsKey(prop))
            {
                var color = material.properties.colors[prop].values[0];
                return new float[4] { color.r, color.g, color.b, color.a };
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Parses a texture property from the material and material group.
        /// </summary>
        /// <param name="material">The material containing texture information.</param>
        /// <param name="group">The material group containing related materials.</param>
        /// <param name="prop">The property name to parse.</param>
        /// <returns>An instance of ISvfMaterialMap representing the parsed texture property or null if not found.</returns>
        private static ISvfMaterialMap? ParseTextureProperty(ISvfMaterial material, ISvfMaterialGroup group,
            string prop)
        {
            if (material.textures != null && material.textures.ContainsKey(prop))
            {
                string connection = material.textures[prop].connections[0];
                ISvfMaterial texture = group.materials[connection];
                if (texture.properties.uris.ContainsKey("unifiedbitmap_Bitmap"))
                {
                    string uri = texture.properties.uris["unifiedbitmap_Bitmap"].values[0];
                    float textureUScale = 0.0f, textureVScale = 0.0f;

                    if (texture.properties.scalars != null
                        && texture.properties.scalars.ContainsKey("texture_UScale")
                        && texture.properties.scalars.ContainsKey("texture_VScale"))
                    {
                        textureUScale = texture.properties.scalars["texture_UScale"].values[0];
                        textureVScale = texture.properties.scalars["texture_VScale"].values[0];
                    }

                    if (uri != null)
                    {
                        return new ISvfMaterialMap()
                        {
                            uri = uri,
                            scale = new ISvfMaterialMapScale()
                            {
                                texture_UScale = textureUScale != 0 ? textureUScale : 1.0f,
                                texture_VScale = textureVScale != 0 ? textureVScale : 1.0f,
                            }
                        };
                    }
                }
            }

            return null;
        }
    }
}