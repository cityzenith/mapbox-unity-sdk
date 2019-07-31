using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Mapbox.Unity
{
	public enum ExternalProvider
	{
		MapBox,
		MapTiler,
		Bing
	}

	public sealed class MapboxExternalOptions
	{
		public static ExternalProvider TerrainProvider { get; private set; }

		public static ExternalProvider ImageProvider { get; private set; }

		public static string BingApiKey { get; private set; }

		public static string MapTilerMapsKey { get; private set; }

		public static void SetBingTerrainEndpointData(string accessKey)
		{
			TerrainProvider = ExternalProvider.Bing;
			BingApiKey = accessKey;
		}

		public static void SetMapTilerTerrainEndpointData(string accessKey)
		{
			TerrainProvider = ExternalProvider.MapTiler;
			MapTilerMapsKey = accessKey;
		}

		public static void SetMapTilerMapImageEndpointData(string accessKey)
		{
			ImageProvider = ExternalProvider.MapTiler;
			MapTilerMapsKey = accessKey;
		}
	}
}
