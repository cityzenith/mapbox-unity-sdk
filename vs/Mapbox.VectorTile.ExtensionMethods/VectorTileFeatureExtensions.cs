using Mapbox.VectorTile.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Mapbox.VectorTile.ExtensionMethods
{
    public static class VectorTileFeatureExtensions
    {
        public static List<List<LatLng>> GeometryAsWgs84(this VectorTileFeature feature, ulong zoom, ulong tileColumn, ulong tileRow, uint? clipBuffer = default(uint?))
        {
            List<List<LatLng>> list = new List<List<LatLng>>();
            foreach (List<Point2d<long>> item in feature.Geometry<long>(clipBuffer, 1f))
            {
                list.Add((from g in item
                          select g.ToLngLat(zoom, tileColumn, tileRow, feature.Layer.Extent, false)).ToList());
            }
            return list;
        }
    }
}
