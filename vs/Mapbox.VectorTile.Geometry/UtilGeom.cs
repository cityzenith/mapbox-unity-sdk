using Mapbox.VectorTile.Geometry.InteralClipperLib;
using System.Collections.Generic;

namespace Mapbox.VectorTile.Geometry
{
    public static class UtilGeom
    {
        public static List<List<Point2d<long>>> ClipGeometries(List<List<Point2d<long>>> geoms, GeomType geomType, long extent, uint bufferSize, float scale)
        {
            List<List<Point2d<long>>> list = new List<List<Point2d<long>>>();
            if (geomType == GeomType.POINT)
            {
                foreach (List<Point2d<long>> geom in geoms)
                {
                    List<Point2d<long>> list2 = new List<Point2d<long>>();
                    foreach (Point2d<long> item2 in geom)
                    {
                        if (item2.X >= 0L - (long)bufferSize && item2.Y >= 0L - (long)bufferSize && item2.X <= extent + bufferSize && item2.Y <= extent + bufferSize)
                        {
                            list2.Add(item2);
                        }
                    }
                    if (list2.Count > 0)
                    {
                        list.Add(list2);
                    }
                }
                return list;
            }
            bool closed = true;
            if (geomType == GeomType.LINESTRING)
            {
                closed = false;
            }
            List<List<InternalClipper.IntPoint>> list3 = new List<List<InternalClipper.IntPoint>>();
            List<List<InternalClipper.IntPoint>> list4 = new List<List<InternalClipper.IntPoint>>(1);
            List<List<InternalClipper.IntPoint>> list5 = new List<List<InternalClipper.IntPoint>>();
            list4.Add(new List<InternalClipper.IntPoint>(4));
            list4[0].Add(new InternalClipper.IntPoint(0L - (long)bufferSize, 0L - (long)bufferSize));
            list4[0].Add(new InternalClipper.IntPoint(extent + bufferSize, 0L - (long)bufferSize));
            list4[0].Add(new InternalClipper.IntPoint(extent + bufferSize, extent + bufferSize));
            list4[0].Add(new InternalClipper.IntPoint(0L - (long)bufferSize, extent + bufferSize));
            foreach (List<Point2d<long>> geom2 in geoms)
            {
                List<InternalClipper.IntPoint> list6 = new List<InternalClipper.IntPoint>();
                foreach (Point2d<long> item3 in geom2)
                {
                    list6.Add(new InternalClipper.IntPoint(item3.X, item3.Y));
                }
                list3.Add(list6);
            }
            InternalClipper.Clipper clipper = new InternalClipper.Clipper(0);
            clipper.AddPaths(list3, InternalClipper.PolyType.ptSubject, closed);
            clipper.AddPaths(list4, InternalClipper.PolyType.ptClip, true);
            bool flag = false;
            if (geomType == GeomType.LINESTRING)
            {
                InternalClipper.PolyTree polytree = new InternalClipper.PolyTree();
                flag = clipper.Execute(InternalClipper.ClipType.ctIntersection, polytree, InternalClipper.PolyFillType.pftNonZero, InternalClipper.PolyFillType.pftNonZero);
                if (flag)
                {
                    list5 = InternalClipper.Clipper.PolyTreeToPaths(polytree);
                }
            }
            else
            {
                flag = clipper.Execute(InternalClipper.ClipType.ctIntersection, list5, InternalClipper.PolyFillType.pftNonZero, InternalClipper.PolyFillType.pftNonZero);
            }
            if (flag)
            {
                list = new List<List<Point2d<long>>>();
                foreach (List<InternalClipper.IntPoint> item4 in list5)
                {
                    List<Point2d<long>> list7 = new List<Point2d<long>>();
                    Point2d<long> item;
                    foreach (InternalClipper.IntPoint item5 in item4)
                    {
                        List<Point2d<long>> list8 = list7;
                        item = new Point2d<long>
                        {
                            X = item5.X,
                            Y = item5.Y
                        };
                        list8.Add(item);
                    }
                    if (geomType == GeomType.POLYGON)
                    {
                        item = list7[0];
                        if (!item.Equals(list7[list7.Count - 1]))
                        {
                            list7.Insert(0, list7[list7.Count - 1]);
                        }
                    }
                    list.Add(list7);
                }
                return list;
            }
            return geoms;
        }
    }
}
