using System.Collections.Generic;
using System.Diagnostics;

namespace Mapbox.VectorTile
{
    [DebuggerDisplay("Layer {Name}")]
    public class VectorTileLayer
    {
        public byte[] Data
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            set;
        }

        public ulong Version
        {
            get;
            set;
        }

        public ulong Extent
        {
            get;
            set;
        }

        private List<byte[]> _FeaturesData
        {
            get;
            set;
        }

        public List<object> Values
        {
            get;
            set;
        }

        public List<string> Keys
        {
            get;
            set;
        }

        public VectorTileLayer()
        {
            _FeaturesData = new List<byte[]>();
            Keys = new List<string>();
            Values = new List<object>();
        }

        public VectorTileLayer(byte[] data)
            : this()
        {
            Data = data;
        }

        public int FeatureCount()
        {
            return _FeaturesData.Count;
        }

        public VectorTileFeature GetFeature(int feature, uint? clipBuffer = default(uint?), float scale = 1f)
        {
            return VectorTileReader.GetFeature(this, _FeaturesData[feature], true, clipBuffer, scale);
        }

        public void AddFeatureData(byte[] data)
        {
            _FeaturesData.Add(data);
        }
    }
}
