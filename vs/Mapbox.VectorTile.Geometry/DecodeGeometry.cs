using Mapbox.VectorTile.Contants;
using System;
using System.Collections.Generic;

namespace Mapbox.VectorTile.Geometry
{
    public static class DecodeGeometry
    {
        public static List<List<Point2d<long>>> GetGeometry(ulong extent, GeomType geomType, List<uint> geometryCommands, float scale = 1f)
        {
            List<List<Point2d<long>>> list = new List<List<Point2d<long>>>();
            List<Point2d<long>> list2 = new List<Point2d<long>>();
            long num = 0L;
            long num2 = 0L;
            int count = geometryCommands.Count;
            for (int i = 0; i < count; i++)
            {
                uint num3 = geometryCommands[i];
                Commands commands = (Commands)(num3 & 7);
                uint num4 = num3 >> 3;
                if (commands == Commands.MoveTo || commands == Commands.LineTo)
                {
                    for (int j = 0; j < num4; j++)
                    {
                        Point2d<long> point2d = zigzagDecode(geometryCommands[i + 1], geometryCommands[i + 2]);
                        num += point2d.X;
                        num2 += point2d.Y;
                        i += 2;
                        if (commands == Commands.MoveTo && list2.Count > 0)
                        {
                            list.Add(list2);
                            list2 = new List<Point2d<long>>();
                        }
                        Point2d<long> point2d2 = default(Point2d<long>);
                        point2d2.X = num;
                        point2d2.Y = num2;
                        Point2d<long> item = point2d2;
                        list2.Add(item);
                    }
                }
                if (commands == Commands.ClosePath && geomType == GeomType.POLYGON && list2.Count > 0)
                {
                    list2.Add(list2[0]);
                }
            }
            if (list2.Count > 0)
            {
                list.Add(list2);
            }
            return list;
        }

        public static List<List<Point2d<T>>> Scale<T>(List<List<Point2d<long>>> inGeom, float scale = 1f)
        {
            List<List<Point2d<T>>> list = new List<List<Point2d<T>>>();
            foreach (List<Point2d<long>> item in inGeom)
            {
                List<Point2d<T>> list2 = new List<Point2d<T>>();
                foreach (Point2d<long> item2 in item)
                {
                    float value = (float)item2.X * scale;
                    float value2 = (float)item2.Y * scale;
                    if (typeof(T) == typeof(int))
                    {
                        int num = Convert.ToInt32(value);
                        int num2 = Convert.ToInt32(value2);
                        list2.Add(new Point2d<T>((T)(object)num, (T)(object)num2));
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        long num3 = Convert.ToInt64(value);
                        long num4 = Convert.ToInt64(value2);
                        list2.Add(new Point2d<T>((T)(object)num3, (T)(object)num4));
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        float num5 = Convert.ToSingle(value);
                        float num6 = Convert.ToSingle(value2);
                        list2.Add(new Point2d<T>((T)(object)num5, (T)(object)num6));
                    }
                }
                list.Add(list2);
            }
            return list;
        }

        private static Point2d<long> zigzagDecode(long x, long y)
        {
            Point2d<long> result = default(Point2d<long>);
            result.X = (x >> 1 ^ -(x & 1));
            result.Y = (y >> 1 ^ -(y & 1));
            return result;
        }
    }
}
