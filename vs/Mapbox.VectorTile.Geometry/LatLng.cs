using System.Globalization;

namespace Mapbox.VectorTile.Geometry
{
    public struct LatLng
    {
        public double Lat
        {
            get;
            set;
        }

        public double Lng
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "{0:0.000000}/{1:0.000000}", Lat, Lng);
        }
    }
}
