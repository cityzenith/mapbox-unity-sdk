//-----------------------------------------------------------------------
// <copyright file="TileResource.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Mapbox.Map
{
	using Platform;
	using System;
	using Mapbox.Unity.Telemetry;
	using Mapbox.Utils.Bing;

	public sealed class TileResource : IResource
	{
		readonly string _query;
		readonly bool _isCustom;

		internal TileResource(string query)
		{
			_query = query;
		}

		internal TileResource(string query, bool isCustom)
		{
			_query = query;
			_isCustom = isCustom;
		}

		public static TileResource MakeRaster(CanonicalTileId id, string styleUrl)
		{
			return new TileResource(string.Format("{0}/{1}", MapUtils.NormalizeStaticStyleURL(styleUrl ?? "mapbox://styles/mapbox/satellite-v9"), id));
		}

		internal static TileResource MakeRetinaRaster(CanonicalTileId id, string styleUrl)
		{
			return new TileResource(string.Format("{0}/{1}@2x", MapUtils.NormalizeStaticStyleURL(styleUrl ?? "mapbox://styles/mapbox/satellite-v9"), id));
		}

		public static TileResource MakeClassicRaster(CanonicalTileId id, string mapId)
		{
			return new TileResource(string.Format("{0}/{1}.png", MapUtils.MapIdToUrl(mapId ?? "mapbox.satellite"), id));
		}

		internal static TileResource MakeClassicRetinaRaster(CanonicalTileId id, string mapId)
		{
			return new TileResource(string.Format("{0}/{1}@2x.png", MapUtils.MapIdToUrl(mapId ?? "mapbox.satellite"), id));
		}

		public static TileResource MakeRawPngRaster(CanonicalTileId id, string mapId)
		{
			return new TileResource(string.Format("{0}/{1}.pngraw", MapUtils.MapIdToUrl(mapId ?? "mapbox.terrain-rgb"), id));
		}

		public static TileResource MakeMapTilerVector(CanonicalTileId id, string url)
		{
			string[] parts = url.Split('?');

			return new TileResource(string.Format("{0}/{1}.pbf?{2}", parts[0], id, parts[1]), true);
		}

		public static TileResource MakeVector(CanonicalTileId id, string mapId)
		{
			return new TileResource(string.Format("{0}/{1}.vector.pbf", MapUtils.MapIdToUrl(mapId ?? "mapbox.mapbox-streets-v7"), id));
		}

		internal static TileResource MakeStyleOptimizedVector(CanonicalTileId id, string mapId, string optimizedStyleId, string modifiedDate)
		{
			return new TileResource(string.Format("{0}/{1}.vector.pbf?style={2}@{3}", MapUtils.MapIdToUrl(mapId ?? "mapbox.mapbox-streets-v7"), id, optimizedStyleId, modifiedDate));
		}

		public static TileResource MakeMapTileResource(CanonicalTileId id, string rasterbaseUrl, bool hiRes = false)
		{
			string url = null;

			if (rasterbaseUrl.StartsWith("bing:"))
			{
				string quadKey = TileSystem.TileXYToQuadKey(id.X, id.Y, id.Z);
				url = rasterbaseUrl.Replace("bing:", "http:").Replace("{quadkey}", quadKey);
			}
			else
			{
				string hiResStr = hiRes ? "@2x" : "";
				string[] parts = rasterbaseUrl.Split('?');
				url = string.Format("{0}/{1}{2}.{3}?{4}", parts[0], id, hiResStr, parts[1], parts[2]);
			}

			return new TileResource(url, true);
		}

		public static TileResource MakeCustomRaster(CanonicalTileId id, string rasterbaseUrl)
		{
			return new TileResource(string.Format("{0}/{1}.jpg?key=vvhLTPlyirABPb7kwM6Y", rasterbaseUrl, id), true);
		}

		public static TileResource MakeCustomRawPngRaster(CanonicalTileId id, string rasterbaseUrl)
		{
			return new TileResource(string.Format("{0}/{1}.png?key=vvhLTPlyirABPb7kwM6Y", rasterbaseUrl, id), true);
		}

		public string GetUrl()
		{
			var uriBuilder = new UriBuilder(_query);

			if (!_isCustom)
			{
				if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
				{
					uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + TelemetryFactory.EventQuery;
				}
				else
				{
					uriBuilder.Query = TelemetryFactory.EventQuery;
				}
			}

			//return uriBuilder.ToString();
			return uriBuilder.Uri.ToString();
		}
	}
}
