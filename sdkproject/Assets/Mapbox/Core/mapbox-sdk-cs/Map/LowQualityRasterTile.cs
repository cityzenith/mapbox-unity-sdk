namespace Mapbox.Map
{
    public class LowQualityRasterTile : RasterTile
    {
        public override TileResource MakeTileResource(string styleUrl)
        {
            return TileResource.MakeLowQualityRaster(Id, styleUrl);
        }
    }
}
