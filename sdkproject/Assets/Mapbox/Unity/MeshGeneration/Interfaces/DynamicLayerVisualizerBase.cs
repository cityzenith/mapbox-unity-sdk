using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.VectorTile;
using System;

public abstract class DynamicLayerVisualizerBase : ILayerVisualizer
{
    public virtual bool IsActive
    {
        get { return Active; }
        set { Active = value; }
    }
    public bool Active = true;
    public abstract string Key { get; set; }
    public abstract void Create(VectorTileLayer layer, UnityTile tile, Action callback = null);

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
