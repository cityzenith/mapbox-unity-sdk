namespace Mapbox.Unity.MeshGeneration.Interfaces
{
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.VectorTile;
	using System;

	public interface ILayerVisualizer
	{
		bool IsActive
		{
			get;
			set;
		}

		string Key
		{
			get;
			set;
		}

		void Create(VectorTileLayer layer, UnityTile tile, Action<UnityTile, ILayerVisualizer> callback = null);

		void Initialize();

		void UnregisterTile(UnityTile tile);

		void OnUnregisterTile(UnityTile tile);
	}
}
