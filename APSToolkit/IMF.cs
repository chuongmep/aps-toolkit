// Intermediate format added to follow the Forge-Convert-Utils logic
// Not used for now
// The Unity loader import from the ISvfContent

namespace APSToolkit
{
    public abstract class IScene
    {
        public abstract Metadata GetMetadata();
        public abstract int GetNodeCount();
        public abstract INode GetNode(int id);
        public abstract int GetGeometryCount();
        public abstract IGeometry GetGeometry(int id);
        public abstract int GetMaterialCount();
        public abstract IMaterial GetMaterial(int id);
        public abstract byte[] GetImage(string uri);
    }

    public struct Metadata
    {
        public Dictionary<string, object> Key { get; set; }
    }

    public struct Vec2
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public struct Vec3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public struct Quat
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }

    public enum TransformKind
    {
        Matrix,
        Decomposed
    }

    public interface ITransform
    {
    }

    public interface INode
    {
    }

    // public interface NodeID {}
    // public interface GeometryID {}
    // public interface MaterialID {}
    // public interface CameraID {}
    // public interface LightID {}

    public class MatrixTransform : ITransform
    {
        public TransformKind Kind { get; set; } = TransformKind.Matrix;
        public List<double> Elements { get; set; }
    }


    public class DecomposedTransform : ITransform
    {
        public TransformKind Kind { get; set; } = TransformKind.Decomposed;
        public Vec3? Translation { get; set; }
        public Quat? Rotation { get; set; }
        public Vec3? Scale { get; set; }
    }

    public enum NodeKind
    {
        Group,
        Object,
        Camera,
        Light
    }

    public class GroupNode : INode
    {
        public NodeKind Kind { get; set; } = NodeKind.Group;
        public int Dbid { get; set; }
        public ITransform Transform { get; set; }
        public int[] Children { get; set; }
    }

    public class ObjectNode : INode
    {
        public NodeKind Kind { get; set; } = NodeKind.Object;
        public int Dbid { get; set; }
        public ITransform Transform { get; set; }
        public int Geometry { get; set; }
        public int Material { get; set; }
    }


    public class CameraNode : INode
    {
        public NodeKind Kind { get; set; } = NodeKind.Camera;
        public ITransform Transform { get; set; }
        public int Camera { get; set; }
    }


    public class LightNode : INode
    {
        public NodeKind Kind { get; set; } = NodeKind.Light;
        public ITransform Transform { get; set; }
        public int Light { get; set; }
    }

    public interface IGeometry
    {
    }

    public enum GeometryKind
    {
        Mesh,
        Lines,
        Points,
        Empty
    }

    public class MeshGeometry : IGeometry
    {
        public GeometryKind Kind { get; set; } = GeometryKind.Mesh;
        public Func<uint[]> GetIndices { get; set; }
        public Func<float[]> GetVertices { get; set; }
        public Func<float[]> GetNormals { get; set; }
        public Func<float[]> GetColors { get; set; }
        public Func<int> GetUvChannelCount { get; set; }
        public Func<int, float[]> GetUvs { get; set; }
    }


    public class LineGeometry : IGeometry
    {
        public GeometryKind Kind { get; set; }
        public Func<ushort[]> GetIndices { get; set; }
        public Func<float[]> GetVertices { get; set; }
        public Func<float[]> GetColors { get; set; }
    }

    public class PointGeometry : IGeometry
    {
        public GeometryKind Kind { get; set; } = GeometryKind.Points;
        public Func<float[]> GetVertices { get; set; }
        public Func<float[]> GetColors { get; set; }
    }

    public class EmptyGeometry : IGeometry
    {
        public GeometryKind Kind { get; set; } = GeometryKind.Empty;
    }

    public enum MaterialKind
    {
        Physical
    }

    public interface IMaterial
    {
    }

    public class PhysicalMaterial : IMaterial
    {
        public MaterialKind Kind { get; set; } = MaterialKind.Physical;
        public Vec3 Diffuse { get; set; }
        public float? Metallic { get; set; }
        public float? Roughness { get; set; }
        public float? Opacity { get; set; }
        public Maps? Maps { get; set; }
        public Vec2 Scale { get; set; }
    }

    public struct Maps
    {
        public string Diffuse { get; set; }
    }
}