namespace Mapbox.Map
{
	public class LowQualityRasterTile : RasterTile
	{
		internal override TileResource MakeTileResource(string styleUrl)
		{
			return TileResource.MakeLowQualityRaster(Id, styleUrl);
		}
	}
}
