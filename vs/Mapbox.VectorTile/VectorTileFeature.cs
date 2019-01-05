using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;

namespace Mapbox.VectorTile
{
    public class VectorTileFeature
    {
        private VectorTileLayer _layer;

        private object _cachedGeometry;

        private uint? _clipBuffer;

        private float? _scale;

        private float? _previousScale;

        public ulong Id
        {
            get;
            set;
        }

        public VectorTileLayer Layer => _layer;

        public GeomType GeometryType
        {
            get;
            set;
        }

        public List<uint> GeometryCommands
        {
            get;
            set;
        }

        public List<int> Tags
        {
            get;
            set;
        }

        public VectorTileFeature(VectorTileLayer layer, uint? clipBuffer = default(uint?), float scale = 1f)
        {
            _layer = layer;
            _clipBuffer = clipBuffer;
            _scale = scale;
            Tags = new List<int>();
        }

        public List<List<Point2d<T>>> Geometry<T>(uint? clipBuffer = default(uint?), float? scale = default(float?))
        {
            if (_clipBuffer.HasValue && !clipBuffer.HasValue)
            {
                clipBuffer = _clipBuffer;
            }
            if (_scale.HasValue && !scale.HasValue)
            {
                scale = _scale;
            }
            List<List<Point2d<T>>> list = _cachedGeometry as List<List<Point2d<T>>>;
            int num;
            if (list != null)
            {
                float? nullable = scale;
                float? previousScale = _previousScale;
                num = ((nullable.GetValueOrDefault() == previousScale.GetValueOrDefault() & nullable.HasValue == previousScale.HasValue) ? 1 : 0);
            }
            else
            {
                num = 0;
            }
            if (num != 0)
            {
                return list;
            }
            List<List<Point2d<long>>> list2 = DecodeGeometry.GetGeometry(_layer.Extent, GeometryType, GeometryCommands, scale.Value);
            if (clipBuffer.HasValue)
            {
                if (list2.Count < 2 || GeometryType != GeomType.POLYGON)
                {
                    list2 = UtilGeom.ClipGeometries(list2, GeometryType, (long)_layer.Extent, clipBuffer.Value, scale.Value);
                }
                else
                {
                    List<List<Point2d<long>>> list3 = new List<List<Point2d<long>>>();
                    int count = list2.Count;
                    for (int i = 0; i < count; i++)
                    {
                        List<Point2d<long>> list4 = list2[i];
                        List<List<Point2d<long>>> list5 = new List<List<Point2d<long>>>();
                        bool flag = signedPolygonArea(list4) >= 0f;
                        if (flag)
                        {
                            list4.Reverse();
                        }
                        list5.Add(list4);
                        list5 = UtilGeom.ClipGeometries(list5, GeometryType, (long)_layer.Extent, clipBuffer.Value, scale.Value);
                        if (list5.Count != 0)
                        {
                            foreach (List<Point2d<long>> item in list5)
                            {
                                if (flag)
                                {
                                    item.Reverse();
                                }
                                list3.Add(item);
                            }
                        }
                    }
                    list2 = list3;
                }
            }
            list = DecodeGeometry.Scale<T>(list2, scale.Value);
            _previousScale = scale;
            _cachedGeometry = list;
            return list;
        }

        private float signedPolygonArea(List<Point2d<long>> vertices)
        {
            int num = vertices.Count - 1;
            float num2 = 0f;
            for (int i = 0; i < num; i++)
            {
                num2 += (float)((vertices[i + 1].X - vertices[i].X) * (vertices[i + 1].Y + vertices[i].Y) / 2);
            }
            return num2;
        }

        public Dictionary<string, object> GetProperties()
        {
            if (Tags.Count % 2 != 0)
            {
                throw new Exception($"Layer [{_layer.Name}]: uneven number of feature tag ids");
            }
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            int count = Tags.Count;
            for (int i = 0; i < count; i += 2)
            {
                dictionary.Add(_layer.Keys[Tags[i]], _layer.Values[Tags[i + 1]]);
            }
            return dictionary;
        }

        public object GetValue(string key)
        {
            int num = _layer.Keys.IndexOf(key);
            if (-1 == num)
            {
                throw new Exception($"Key [{key}] does not exist");
            }
            int count = Tags.Count;
            for (int i = 0; i < count; i++)
            {
                if (num == Tags[i])
                {
                    return _layer.Values[Tags[i + 1]];
                }
            }
            return null;
        }
    }
}
