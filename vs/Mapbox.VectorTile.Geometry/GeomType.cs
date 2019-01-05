using System.ComponentModel;

namespace Mapbox.VectorTile.Geometry
{
    public enum GeomType
    {
        UNKNOWN,
        [Description("Point")]
        POINT,
        [Description("LineString")]
        LINESTRING,
        [Description("Polygon")]
        POLYGON
    }
}
