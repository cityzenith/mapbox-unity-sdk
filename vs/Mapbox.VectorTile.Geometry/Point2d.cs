using System;
using System.Globalization;

namespace Mapbox.VectorTile.Geometry
{
    public struct Point2d<T>
    {
        public T X;

        public T Y;

        public Point2d(T x, T y)
        {
            X = x;
            Y = y;
        }

        public LatLng ToLngLat(ulong z, ulong x, ulong y, ulong extent, bool checkLatLngMax = false)
        {
            double num = (double)extent * Math.Pow(2.0, (double)z);
            double num2 = (double)extent * (double)x;
            double num3 = (double)extent * (double)y;
            double num4 = Convert.ToDouble(Y);
            double num5 = Convert.ToDouble(X);
            double num6 = 180.0 - (num4 + num3) * 360.0 / num;
            double num7 = (num5 + num2) * 360.0 / num - 180.0;
            double num8 = 114.59155902616465 * Math.Atan(Math.Exp(num6 * 3.1415926535897931 / 180.0)) - 90.0;
            if (checkLatLngMax)
            {
                if (num7 < -180.0 || num7 > 180.0)
                {
                    throw new ArgumentOutOfRangeException("Longitude out of range");
                }
                if (num8 < -85.051128779806589 || num8 > 85.051128779806589)
                {
                    throw new ArgumentOutOfRangeException("Latitude out of range");
                }
            }
            LatLng result = default(LatLng);
            result.Lat = num8;
            result.Lng = num7;
            return result;
        }

        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "{0}/{1}", X, Y);
        }
    }
}
