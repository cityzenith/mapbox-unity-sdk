using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mapbox.VectorTile.ExtensionMethods
{
    public static class VectorTileExtensions
    {
        public static string ToGeoJson(this VectorTile tile, ulong zoom, ulong tileColumn, ulong tileRow, uint? clipBuffer = default(uint?))
        {
            string format = "{{\"type\":\"FeatureCollection\",\"features\":[{0}]}}";
            string format2 = "{{\"type\":\"Feature\",\"geometry\":{{\"type\":\"{0}\",\"coordinates\":[{1}]}},\"properties\":{2}}}";
            List<string> list = new List<string>();
            foreach (string item in tile.LayerNames())
            {
                VectorTileLayer layer = tile.GetLayer(item);
                for (int i = 0; i < layer.FeatureCount(); i++)
                {
                    VectorTileFeature feature = layer.GetFeature(i, clipBuffer, 1f);
                    string text2;
                    string text3;
                    string text4;
                    if (feature.GeometryType != 0)
                    {
                        List<string> list2 = new List<string>();
                        int count = feature.Tags.Count;
                        for (int j = 0; j < count; j += 2)
                        {
                            string text = layer.Keys[feature.Tags[j]];
                            object obj = layer.Values[feature.Tags[j + 1]];
                            list2.Add(string.Format(NumberFormatInfo.InvariantInfo, "\"{0}\":\"{1}\"", text, obj));
                        }
                        text2 = string.Format(NumberFormatInfo.InvariantInfo, "{{\"id\":{0},\"lyr\":\"{1}\"{2}{3}}}", feature.Id, layer.Name, (list2.Count > 0) ? "," : "", string.Join(",", list2.ToArray()));
                        text3 = "";
                        text4 = ((Enum)(object)feature.GeometryType).Description();
                        List<List<LatLng>> list3 = feature.GeometryAsWgs84(zoom, tileColumn, tileRow, null);
                        if (list3.Count > 1)
                        {
                            switch (feature.GeometryType)
                            {
                                case GeomType.POINT:
                                    text4 = "MultiPoint";
                                    text3 = string.Join(",", (from g in list3.SelectMany((List<LatLng> g) => g)
                                                              select string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray());
                                    break;
                                case GeomType.LINESTRING:
                                    {
                                        text4 = "MultiLineString";
                                        List<string> list5 = new List<string>();
                                        foreach (List<LatLng> item2 in list3)
                                        {
                                            list5.Add("[" + string.Join(",", (from g in item2
                                                                              select string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()) + "]");
                                        }
                                        text3 = string.Join(",", list5.ToArray());
                                        break;
                                    }
                                case GeomType.POLYGON:
                                    {
                                        text4 = "MultiPolygon";
                                        List<string> list4 = new List<string>();
                                        foreach (List<LatLng> item3 in list3)
                                        {
                                            list4.Add("[" + string.Join(",", (from g in item3
                                                                              select string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()) + "]");
                                        }
                                        text3 = "[" + string.Join(",", list4.ToArray()) + "]";
                                        break;
                                    }
                            }
                            goto IL_04a5;
                        }
                        if (list3.Count == 1)
                        {
                            switch (feature.GeometryType)
                            {
                                case GeomType.POINT:
                                    {
                                        NumberFormatInfo invariantInfo = NumberFormatInfo.InvariantInfo;
                                        object[] obj2 = new object[2];
                                        LatLng latLng = list3[0][0];
                                        obj2[0] = latLng.Lng;
                                        latLng = list3[0][0];
                                        obj2[1] = latLng.Lat;
                                        text3 = string.Format(invariantInfo, "{0},{1}", obj2);
                                        break;
                                    }
                                case GeomType.LINESTRING:
                                    text3 = string.Join(",", (from g in list3[0]
                                                              select string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray());
                                    break;
                                case GeomType.POLYGON:
                                    text3 = "[" + string.Join(",", (from g in list3[0]
                                                                    select string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()) + "]";
                                    break;
                            }
                            goto IL_04a5;
                        }
                    }
                    continue;
                    IL_04a5:
                    list.Add(string.Format(NumberFormatInfo.InvariantInfo, format2, text4, text3, text2));
                }
            }
            return string.Format(NumberFormatInfo.InvariantInfo, format, string.Join(",", list.ToArray()));
        }
    }
}
