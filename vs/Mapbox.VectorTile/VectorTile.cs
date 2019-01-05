using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mapbox.VectorTile
{
    [DebuggerDisplay("{Zoom}/{TileColumn}/{TileRow}")]
    public class VectorTile
    {
        private VectorTileReader _VTR;

        public VectorTile(byte[] data, bool validate = true)
        {
            _VTR = new VectorTileReader(data, validate);
        }

        public ReadOnlyCollection<string> LayerNames()
        {
            return _VTR.LayerNames();
        }

        public VectorTileLayer GetLayer(string layerName)
        {
            return _VTR.GetLayer(layerName);
        }
    }
}
