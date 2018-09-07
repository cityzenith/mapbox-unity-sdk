namespace Mapbox.Unity.MeshGeneration.Interfaces
{
	using Mapbox.VectorTile;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using System;

	/// <summary>
	/// Layer visualizers contains sytling logic and processes features
	/// </summary>
	public abstract class LayerVisualizerBase : ScriptableObject, ILayerVisualizer
	{
		public bool IsActive
		{
			get { return Active; }
			set { Active = value; }
		}

		public bool Active = true;
		public abstract string Key { get; set; }
		//public event Action FeaturePreProcessEvent;
		//public event Action FeaturePostProcessEvent;
		public abstract void Create(VectorTileLayer layer, UnityTile tile, Action<UnityTile, LayerVisualizerBase> callback = null);

		public virtual void Create(VectorTileLayer layer, UnityTile tile, Action<UnityTile, ILayerVisualizer> callback = null)
		{
			Action<UnityTile, LayerVisualizerBase> innerCallback = (t, l) => callback(t, l);

			Create(layer, tile, innerCallback);
		}

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
