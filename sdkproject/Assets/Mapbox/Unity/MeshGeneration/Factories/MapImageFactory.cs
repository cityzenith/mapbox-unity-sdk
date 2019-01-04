namespace Mapbox.Unity.MeshGeneration.Factories
{
	using Mapbox.Map;
	using Mapbox.Unity.Map;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Enums;
	using System.Collections.Generic;
	using UnityEngine;

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
		private List<UnityTile> existingTiles = new List<UnityTile>();

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
				{
					ReloadTiles();
				}
			}
		}

		public float MapTransparency
		{
			get { return _mapTransparency; }
			set
			{
				_mapTransparency = value;
				UpdateTransparency();
			}
		}
		private float _mapTransparency;

		public bool PreloadLowRes { get; set; }

		public bool ReloadAtRuntime { get; set; }

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
				if (tile.RasterDataState != TilePropertyState.Unregistered)
				{
					_tilesWaitingResponse.Remove(tile);
					tile.SetRasterData(rasterTile.Data, _properties.rasterOptions.useMipMap, _properties.rasterOptions.useCompression);
				}
			}
		}

		//merge this with OnErrorOccurred?
		public virtual void OnDataError(UnityTile tile, RasterTile rasterTile, TileErrorEventArgs e)
		{
			if (tile != null)
			{
				if (tile.RasterDataState != TilePropertyState.Unregistered)
				{
					tile.RasterDataState = TilePropertyState.Error;
					_tilesWaitingResponse.Remove(tile);
					OnErrorOccurred(e);
				}

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

			UpdateTransparencyForTile(tile);
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
			if (_tilesWaitingResponse.Contains(tile))
			{
				_tilesWaitingResponse.Remove(tile);
			}

			if (existingTiles.Contains(tile))
			{
				existingTiles.Remove(tile);
			}
		}

		public override void OnPostProcess(UnityTile tile)
		{

		}
		#endregion

		private void ReloadTiles()
		{
			foreach (UnityTile existingTile in existingTiles)
			{
				ProcessTile(existingTile);
			}
		}

		private void UpdateTransparency()
		{
			foreach (UnityTile existingTile in existingTiles)
			{
				UpdateTransparencyForTile(existingTile);
			}
		}

		private void UpdateTransparencyForTile(UnityTile tile)
		{
			Color color = tile.Material.color;
			color.a = (1f - MapTransparency);
			tile.Material.color = color;
		}

		private void ProcessTile(UnityTile tile)
		{
			if (_properties.sourceType == ImagerySourceType.None)
			{
				tile.RasterDataState = TilePropertyState.None;
				return;
			}

			tile.RasterDataState = TilePropertyState.Loading;
			DataFetcher.FetchImage(tile.CanonicalTileId, MapId, tile, _properties.rasterOptions.useRetina);
		}
	}
}
