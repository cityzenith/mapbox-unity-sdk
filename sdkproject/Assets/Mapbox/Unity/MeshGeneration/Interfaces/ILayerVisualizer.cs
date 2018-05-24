using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.VectorTile;
using System;

namespace Mapbox.Unity.MeshGeneration.Interfaces
{
	public interface ILayerVisualizer
	{
		bool IsActive { get; set; }

		string Key { get; set; }

		void Create(VectorTileLayer layer, UnityTile tile, Action callback = null);

		void Initialize();

		void UnregisterTile(UnityTile tile);

		void OnUnregisterTile(UnityTile tile);
	}
}
