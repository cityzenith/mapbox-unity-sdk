using Mapbox.VectorTile.Contants;
using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mapbox.VectorTile
{
    public class VectorTileReader
    {
        private Dictionary<string, byte[]> _Layers = new Dictionary<string, byte[]>();

        private bool _Validate;

        public VectorTileReader(byte[] data, bool validate = true)
        {
            if (data == null)
            {
                throw new Exception("Tile data cannot be null");
            }
            if (data.Length < 1)
            {
                throw new Exception("Tile data cannot be empty");
            }
            if (data[0] == 31 && data[1] == 139)
            {
                throw new Exception("Tile data is zipped");
            }
            _Validate = validate;
            layers(data);
        }

        private void layers(byte[] data)
        {
            PbfReader pbfReader = new PbfReader(data);
            string text;
            while (true)
            {
                if (pbfReader.NextByte())
                {
                    if (_Validate && !ConstantsAsDictionary.TileType.ContainsKey(pbfReader.Tag))
                    {
                        throw new Exception($"Unknown tile tag: {pbfReader.Tag}");
                    }
                    if (pbfReader.Tag == 3)
                    {
                        text = null;
                        byte[] array = pbfReader.View();
                        PbfReader pbfReader2 = new PbfReader(array);
                        while (pbfReader2.NextByte())
                        {
                            if (pbfReader2.Tag == 1)
                            {
                                ulong length = (ulong)pbfReader2.Varint();
                                text = pbfReader2.GetString(length);
                            }
                            else
                            {
                                pbfReader2.Skip();
                            }
                        }
                        if (_Validate)
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                throw new Exception("Layer missing name");
                            }
                            if (_Layers.ContainsKey(text))
                            {
                                break;
                            }
                        }
                        _Layers.Add(text, array);
                    }
                    else
                    {
                        pbfReader.Skip();
                    }
                    continue;
                }
                return;
            }
            throw new Exception($"Duplicate layer names: {text}");
        }

        public ReadOnlyCollection<string> LayerNames()
        {
            return _Layers.Keys.ToList().AsReadOnly();
        }

        public VectorTileLayer GetLayer(string name)
        {
            if (!_Layers.ContainsKey(name))
            {
                return null;
            }
            return getLayer(_Layers[name]);
        }

        private VectorTileLayer getLayer(byte[] data)
        {
            VectorTileLayer vectorTileLayer = new VectorTileLayer(data);
            PbfReader pbfReader = new PbfReader(vectorTileLayer.Data);
            while (pbfReader.NextByte())
            {
                int tag = pbfReader.Tag;
                if (_Validate && !ConstantsAsDictionary.LayerType.ContainsKey(tag))
                {
                    throw new Exception($"Unknown layer type: {tag}");
                }
                switch (tag)
                {
                    case 15:
                        {
                            ulong num6 = vectorTileLayer.Version = (ulong)pbfReader.Varint();
                            break;
                        }
                    case 1:
                        {
                            ulong length = (ulong)pbfReader.Varint();
                            vectorTileLayer.Name = pbfReader.GetString(length);
                            break;
                        }
                    case 5:
                        vectorTileLayer.Extent = (ulong)pbfReader.Varint();
                        break;
                    case 3:
                        {
                            byte[] array2 = pbfReader.View();
                            string string2 = Encoding.UTF8.GetString(array2, 0, array2.Length);
                            vectorTileLayer.Keys.Add(string2);
                            break;
                        }
                    case 4:
                        {
                            byte[] tileBuffer = pbfReader.View();
                            PbfReader pbfReader2 = new PbfReader(tileBuffer);
                            while (pbfReader2.NextByte())
                            {
                                switch (pbfReader2.Tag)
                                {
                                    case 1:
                                        {
                                            byte[] array = pbfReader2.View();
                                            string @string = Encoding.UTF8.GetString(array, 0, array.Length);
                                            vectorTileLayer.Values.Add(@string);
                                            break;
                                        }
                                    case 2:
                                        {
                                            float @float = pbfReader2.GetFloat();
                                            vectorTileLayer.Values.Add(@float);
                                            break;
                                        }
                                    case 3:
                                        {
                                            double @double = pbfReader2.GetDouble();
                                            vectorTileLayer.Values.Add(@double);
                                            break;
                                        }
                                    case 4:
                                        {
                                            long num4 = pbfReader2.Varint();
                                            vectorTileLayer.Values.Add(num4);
                                            break;
                                        }
                                    case 5:
                                        {
                                            long num3 = pbfReader2.Varint();
                                            vectorTileLayer.Values.Add(num3);
                                            break;
                                        }
                                    case 6:
                                        {
                                            long num2 = pbfReader2.Varint();
                                            vectorTileLayer.Values.Add(num2);
                                            break;
                                        }
                                    case 7:
                                        {
                                            long num = pbfReader2.Varint();
                                            vectorTileLayer.Values.Add(num == 1);
                                            break;
                                        }
                                    default:
                                        throw new Exception(string.Format(NumberFormatInfo.InvariantInfo, "NOT IMPLEMENTED valueReader.Tag:{0} valueReader.WireType:{1}", pbfReader2.Tag, pbfReader2.WireType));
                                }
                            }
                            break;
                        }
                    case 2:
                        vectorTileLayer.AddFeatureData(pbfReader.View());
                        break;
                    default:
                        pbfReader.Skip();
                        break;
                }
            }
            if (_Validate)
            {
                if (string.IsNullOrEmpty(vectorTileLayer.Name))
                {
                    throw new Exception("Layer has no name");
                }
                if (vectorTileLayer.Version == 0)
                {
                    throw new Exception($"Layer [{vectorTileLayer.Name}] has invalid version. Only version 2.x of 'Mapbox Vector Tile Specification' (https://github.com/mapbox/vector-tile-spec) is supported.");
                }
                if (2 != vectorTileLayer.Version)
                {
                    throw new Exception($"Layer [{vectorTileLayer.Name}] has invalid version: {vectorTileLayer.Version}. Only version 2.x of 'Mapbox Vector Tile Specification' (https://github.com/mapbox/vector-tile-spec) is supported.");
                }
                if (vectorTileLayer.Extent == 0)
                {
                    throw new Exception($"Layer [{vectorTileLayer.Name}] has no extent.");
                }
                if (vectorTileLayer.FeatureCount() == 0)
                {
                    throw new Exception($"Layer [{vectorTileLayer.Name}] has no features.");
                }
                if (vectorTileLayer.Values.Count != vectorTileLayer.Values.Distinct().Count())
                {
                    throw new Exception($"Layer [{vectorTileLayer.Name}]: duplicate attribute values found");
                }
            }
            return vectorTileLayer;
        }

        public static VectorTileFeature GetFeature(VectorTileLayer layer, byte[] data, bool validate = true, uint? clipBuffer = default(uint?), float scale = 1f)
        {
            PbfReader pbfReader = new PbfReader(data);
            VectorTileFeature vectorTileFeature = new VectorTileFeature(layer, clipBuffer, scale);
            bool flag = false;
            while (pbfReader.NextByte())
            {
                int tag = pbfReader.Tag;
                if (validate && !ConstantsAsDictionary.FeatureType.ContainsKey(tag))
                {
                    throw new Exception($"Layer [{layer.Name}] has unknown feature type: {tag}");
                }
                switch (tag)
                {
                    case 1:
                        vectorTileFeature.Id = (ulong)pbfReader.Varint();
                        break;
                    case 2:
                        {
                            List<uint> packedUnit = pbfReader.GetPackedUnit32();
                            Func<uint, int> selector = (uint t) => (int)t;
                            List<int> list2 = vectorTileFeature.Tags = packedUnit.Select(selector).ToList();
                            break;
                        }
                    case 3:
                        {
                            int num = (int)pbfReader.Varint();
                            if (validate && !ConstantsAsDictionary.GeomType.ContainsKey(num))
                            {
                                throw new Exception($"Layer [{layer.Name}] has unknown geometry type tag: {num}");
                            }
                            vectorTileFeature.GeometryType = (GeomType)num;
                            flag = true;
                            break;
                        }
                    case 4:
                        if (vectorTileFeature.GeometryCommands != null)
                        {
                            throw new Exception($"Layer [{layer.Name}], feature already has a geometry");
                        }
                        vectorTileFeature.GeometryCommands = pbfReader.GetPackedUnit32();
                        break;
                    default:
                        pbfReader.Skip();
                        break;
                }
            }
            if (validate)
            {
                if (!flag)
                {
                    throw new Exception($"Layer [{layer.Name}]: feature missing geometry type");
                }
                if (vectorTileFeature.GeometryCommands == null)
                {
                    throw new Exception($"Layer [{layer.Name}]: feature has no geometry");
                }
                if (vectorTileFeature.Tags.Count % 2 != 0)
                {
                    throw new Exception($"Layer [{layer.Name}]: uneven number of feature tag ids");
                }
                if (vectorTileFeature.Tags.Count > 0)
                {
                    int num2 = vectorTileFeature.Tags.Where((int key, int idx) => idx % 2 == 0).Max();
                    int num3 = vectorTileFeature.Tags.Where((int key, int idx) => (idx + 1) % 2 == 0).Max();
                    if (num2 >= layer.Keys.Count)
                    {
                        throw new Exception($"Layer [{layer.Name}]: maximum key index equal or greater number of key elements");
                    }
                    if (num3 >= layer.Values.Count)
                    {
                        throw new Exception($"Layer [{layer.Name}]: maximum value index equal or greater number of value elements");
                    }
                }
            }
            return vectorTileFeature;
        }
    }
}
