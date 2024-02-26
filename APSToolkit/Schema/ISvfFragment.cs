namespace APSToolkit.Schema;

/// <summary>
/// Fragment represents a single scene object,
/// linking together material, geometry, and database IDs,
/// and providing world transform and bounding box on top of that.
/// </summary>
public struct ISvfFragment
{
    public bool visible { get; set; }
    public int materialID { get; set; }
    public int geometryID { get; set; }
    public int dbID { get; set; }
    public ISvfTransform? transform { get; set; }
    public float[] bbox { get; set; }
}
