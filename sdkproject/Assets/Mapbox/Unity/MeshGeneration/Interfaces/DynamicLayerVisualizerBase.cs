using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.VectorTile;
using System;

namespace Mapbox.Unity.MeshGeneration.Interfaces
{
	public abstract class DynamicLayerVisualizerBase : ILayerVisualizer
	{
		public bool IsActive
		{
			get { return Active; }
			set { Active = value; }
		}
		public bool Active = true;

		public abstract string Key { get; set; }

		public abstract void Create(VectorTileLayer layer, UnityTile tile, Action<UnityTile, ILayerVisualizer> callback = null);

		public virtual void Initialize()
		{
		}

		public void UnregisterTile(UnityTile tile)
		{
			OnUnregisterTile(tile);
		}

		public virtual void OnUnregisterTile(UnityTile tile)
		{
		}
	}
}
