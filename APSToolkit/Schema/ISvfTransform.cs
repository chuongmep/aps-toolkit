using System.Numerics;

namespace APSToolkit.Schema;

public struct ISvfTransform
{
    public System.Numerics.Vector3 t { get; set; }
    public System.Numerics.Vector3 s { get; set; }
    public Quaternion q { get; set; }
    public double[] matrix { get; set; }
}
