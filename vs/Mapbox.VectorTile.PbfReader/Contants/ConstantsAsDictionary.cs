using System.Collections.Generic;

namespace Mapbox.VectorTile.Contants
{
    public static class ConstantsAsDictionary
    {
        public static readonly Dictionary<int, string> TileType = new Dictionary<int, string>
    {
        {
            3,
            "Layers"
        }
    };

        public static readonly Dictionary<int, string> LayerType = new Dictionary<int, string>
    {
        {
            15,
            "Version"
        },
        {
            1,
            "Name"
        },
        {
            2,
            "Features"
        },
        {
            3,
            "Keys"
        },
        {
            4,
            "Values"
        },
        {
            5,
            "Extent"
        }
    };

        public static readonly Dictionary<int, string> FeatureType = new Dictionary<int, string>
    {
        {
            1,
            "Id"
        },
        {
            2,
            "Tags"
        },
        {
            3,
            "Type"
        },
        {
            4,
            "Geometry"
        },
        {
            5,
            "Raster"
        }
    };

        public static readonly Dictionary<int, string> GeomType = new Dictionary<int, string>
    {
        {
            0,
            "Unknown"
        },
        {
            1,
            "Point"
        },
        {
            2,
            "LineString"
        },
        {
            3,
            "Polygon"
        }
    };
    }
}
