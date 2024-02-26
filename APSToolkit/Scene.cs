using APSToolkit.Schema;

namespace APSToolkit;

public class Scene : IScene
{
    private ISvfContent svf;

    public Scene(ISvfContent _svf)
    {
        svf = _svf;
    }

    public override Metadata GetMetadata()
    {
        return new Metadata() { Key = this.svf.metadata.metadata };
    }

    public override int GetNodeCount()
    {
        return this.svf.fragments.Length;
    }

    public override INode GetNode(int id)
    {
        ISvfFragment frag = this.svf.fragments[id];
        ObjectNode node = new ObjectNode()
        {
            Kind = NodeKind.Object,
            Dbid = frag.dbID,
            Geometry = frag.geometryID,
            Material = frag.materialID
        };

        if (frag.transform != null)
        {
            if (frag.transform?.matrix != null)
            {
                double[] matrix = frag.transform?.matrix;
                System.Numerics.Vector3 t = (System.Numerics.Vector3)frag.transform?.t;
                node.Transform = new MatrixTransform()
                {
                    Kind = TransformKind.Matrix,
                    Elements = new List<double>()
                    {
                        matrix[0], matrix[3], matrix[6], 0,
                        matrix[1], matrix[4], matrix[7], 0,
                        matrix[2], matrix[5], matrix[8], 0,
                        t.X, t.Y, t.Z, 1
                    }
                };
            }
            else
            {
                node.Transform = new DecomposedTransform()
                {
                    Rotation = new Quat()
                    {
                        X = (float)frag.transform?.q.X,
                        Y = (float)frag.transform?.q.Y,
                        Z = (float)frag.transform?.q.Z,
                        W = (float)frag.transform?.q.W
                    },
                    Scale = new Vec3()
                    {
                        X = (float)frag.transform?.s.X,
                        Y = (float)frag.transform?.s.Y,
                        Z = (float)frag.transform?.s.Z
                    },
                    Translation = new Vec3()
                    {
                        X = (float)frag.transform?.t.X,
                        Y = (float)frag.transform?.t.Y,
                        Z = (float)frag.transform?.t.Z
                    }
                };
                // if (frag.transform?.q != null)
                // {
                //     node.transform.rotation = frag.transform.q;
                // }

                // if (frag.transform?.s != null)
                // {
                //     node.transform.scale = frag.transform.s;
                // }

                // if (frag.transform?.t != null)
                // {
                //     node.transform.translation = frag.transform.t;
                // }
            }
        }

        return node;
    }

    public override int GetGeometryCount()
    {
        return this.svf.geometries.Length;
    }

    public override IGeometry GetGeometry(int id)
    {
        ISvfGeometryMetadata meta = this.svf.geometries[id];
        IMeshPack mesh = this.svf.meshpacks[meta.packID][meta.entityID];
        if (mesh != null)
        {
            if (mesh is ISvfLines)
            {
                ISvfLines svfLines = (ISvfLines)mesh;
                LineGeometry geom = new LineGeometry()
                {
                    Kind = GeometryKind.Lines,
                    GetIndices = () => svfLines.indices,
                    GetVertices = () => svfLines.vertices,
                    GetColors = () => svfLines.colors
                };
                return geom;
            }
            else if (mesh is ISvfPoints)
            {
                ISvfPoints svfPoints = (ISvfPoints)mesh;
                PointGeometry geom = new PointGeometry()
                {
                    Kind = GeometryKind.Points,
                    GetVertices = () => svfPoints.vertices,
                    GetColors = () => svfPoints.colors
                };
                return geom;
            }
            else
            {
                ISvfMesh svfMesh = (ISvfMesh)mesh;
                MeshGeometry geom = new MeshGeometry()
                {
                    Kind = GeometryKind.Mesh,
                    GetIndices = () => svfMesh.indices,
                    GetVertices = () => svfMesh.vertices,
                    GetNormals = () => svfMesh.normals,
                    GetColors = () => svfMesh.colors,
                    GetUvChannelCount = () => svfMesh.uvcount,
                    GetUvs = (channel) => svfMesh.uvmaps[channel].uvs
                };
                return geom;
            }
        }

        return new EmptyGeometry();
    }

    public override int GetMaterialCount()
    {
        return this.svf.materials.Length;
    }

    public override IMaterial GetMaterial(int id)
    {
        ISvfMaterial? _mat = this.svf.materials[id];
        PhysicalMaterial mat = new PhysicalMaterial()
        {
            Kind = MaterialKind.Physical,
            Diffuse = new Vec3() { X = 0, Y = 0, Z = 0 },
            Metallic = (bool)_mat?.metal ? 1.0f : 0.0f,
            Opacity = _mat?.opacity ?? 1.0f,
            Roughness = (_mat?.glossiness != null)
                ? (1.0f - _mat?.glossiness / 255.0f)
                : 1.0f, // TODO: how to map glossiness to roughness properly?
            Scale = new Vec2()
            {
                X = _mat?.maps?.diffuse?.scale.texture_UScale ?? 1.0f,
                Y = _mat?.maps?.diffuse?.scale.texture_VScale ?? 1.0f
            }
        };
        if (_mat?.diffuse != null)
        {
            mat.Diffuse = new Vec3()
            {
                X = (float)_mat?.diffuse[0],
                Y = (float)_mat?.diffuse[1],
                Z = (float)_mat?.diffuse[2]
            };
        }

        if (_mat?.maps?.diffuse != null)
        {
            mat.Maps = mat.Maps ?? null;
            mat.Maps = new Maps() { Diffuse = _mat?.maps?.diffuse?.uri };
        }

        return mat;
    }

    public override byte[] GetImage(string uri)
    {
        return this.svf.images[uri];
    }
}