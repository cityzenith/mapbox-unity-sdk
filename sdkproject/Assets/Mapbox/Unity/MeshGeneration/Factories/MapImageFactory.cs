namespace Mapbox.Unity.MeshGeneration.Factories
{
	using System;
	using Mapbox.Map;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Enums;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using System.Collections.Generic;

	public enum MapImageType
	{
		BasicMapboxStyle,
		Custom,
		None
	}

	/// <summary>
	/// Uses raster image services to create materials & textures for terrain
	/// </summary>
	[CreateAssetMenu(menuName = "Mapbox/Factories/Image Factory")]
	public class MapImageFactory : AbstractTileFactory
	{
		[SerializeField]
		ImageryLayerProperties _properties;
		protected ImageDataFetcher DataFetcher;
		public string MapId
		{
			get
			{
				return _properties.sourceOptions.Id;
			}

			set
			{
				_properties.sourceOptions.Id = value;

				if (ReloadAtRuntime)
					ReloadTiles();
			}
		}

		public bool PreloadLowRes { get; set; }

		public bool ReloadAtRuntime { get; set; }

		private List<UnityTile> existingTiles = new List<UnityTile>();

		#region UnityMethods
		public virtual void OnDestroy()
		{
			if (DataFetcher != null)
			{
				DataFetcher.DataRecieved -= OnImageRecieved;
				DataFetcher.FetchingError -= OnDataError;
			}
		}
		#endregion

		#region DataFetcherEvents
		private void OnImageRecieved(UnityTile tile, RasterTile rasterTile)
		{
			if (tile != null)
			{
				Progress--;
				tile.SetRasterData(rasterTile.Data, _properties.rasterOptions.useMipMap, _properties.rasterOptions.useCompression);
				tile.RasterDataState = TilePropertyState.Loaded;
			}
		}

		//merge this with OnErrorOccurred?
		public virtual void OnDataError(UnityTile tile, TileErrorEventArgs e)
		{
			if (tile != null)
			{
				Progress--;
				tile.RasterDataState = TilePropertyState.Error;
				OnErrorOccurred(e);
			}
		}
		#endregion

		#region AbstractFactoryOverrides
		public override void OnInitialized()
		{
			DataFetcher = ScriptableObject.CreateInstance<ImageDataFetcher>();
			DataFetcher.DataRecieved += OnImageRecieved;
			DataFetcher.FetchingError += OnDataError;
		}

		public override void SetOptions(LayerProperties options)
		{
			_properties = (ImageryLayerProperties)options;
		}

		public override void OnRegistered(UnityTile tile)
		{
			if (ReloadAtRuntime)
				existingTiles.Add(tile);

			ProcessTile(tile);
		}

		/// <summary>
		/// Method to be called when a tile error has occurred.
		/// </summary>
		/// <param name="e"><see cref="T:Mapbox.Map.TileErrorEventArgs"/> instance/</param>
		public override void OnErrorOccurred(TileErrorEventArgs e)
		{
		}

		public override void OnUnregistered(UnityTile tile)
		{
			if (existingTiles.Contains(tile))
				existingTiles.Remove(tile);
		}

		private void ProcessTile(UnityTile tile)
		{
			if (_properties.sourceType == ImagerySourceType.None)
			{
				Progress++;
				Progress--;
				return;
			}

			tile.RasterDataState = TilePropertyState.Loading;
			Progress++;

			if (PreloadLowRes)
			{
				Progress++;
				DataFetcher.FetchImage(tile.CanonicalTileId, MapId, tile, _properties.rasterOptions.useRetina, true);
			}

			DataFetcher.FetchImage(tile.CanonicalTileId, MapId, tile, _properties.rasterOptions.useRetina);
		}

		private void ReloadTiles()
		{
			foreach (UnityTile tile in existingTiles)
				ProcessTile(tile);
		}


		#endregion
	}
}
