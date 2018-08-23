namespace Mapbox.Unity.MeshGeneration.Data
{
	using Mapbox.VectorTile;
	using System.Collections.Generic;
	using Mapbox.VectorTile.Geometry;
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using Mapbox.VectorTile.Geometry.InteralClipperLib;

	public class VectorFeatureUnity
	{
		public VectorTileFeature Data;
		public Dictionary<string, object> Properties;
		public List<List<Vector3>> Points = new List<List<Vector3>>();
		public Bounds bounds;
		public UnityTile Tile;

		private double _rectSizex;
		private double _rectSizey;
		private int _geomCount;
		private int _pointCount;
		private List<Vector3> _newPoints = new List<Vector3>();
		private List<List<Point2d<float>>> _geom;

		public VectorFeatureUnity()
		{
			Points = new List<List<Vector3>>();
		}

		public VectorFeatureUnity(VectorTileFeature feature, UnityTile tile, float layerExtent, bool buildingsWithUniqueIds = false)
		{
			Data = feature;
			Properties = Data.GetProperties();
			Points.Clear();
			Tile = tile;

			//this is a temp hack until we figure out how streets ids works
			if (buildingsWithUniqueIds == true) //ids from building dataset is big ulongs 
			{
				_geom = feature.Geometry<float>(); //and we're not clipping by passing no parameters
			}
			else //streets ids, will require clipping
			{
				_geom = feature.Geometry<float>(0); //passing zero means clip at tile edge
			}

			_rectSizex = tile.Rect.Size.x;
			_rectSizey = tile.Rect.Size.y;


			Point2d<float>? firstPoint = GetFirstPoint();

			if (firstPoint.HasValue)
			{
				Point2d<float> lowerLeft = firstPoint.Value;
				Point2d<float> upperRight = firstPoint.Value;

				_geomCount = _geom.Count;

				for (int i = 0; i < _geomCount; i++)
				{
					_pointCount = _geom[i].Count;
					_newPoints = new List<Vector3>(_pointCount);
					for (int j = 0; j < _pointCount; j++)
					{
						var point = _geom[i][j];
						_newPoints.Add(new Vector3((float)(point.X / layerExtent * _rectSizex - (_rectSizex / 2)) * tile.TileScale, 0, (float)((layerExtent - point.Y) / layerExtent * _rectSizey - (_rectSizey / 2)) * tile.TileScale));

						if (lowerLeft.X > point.X)
							lowerLeft.X = point.X;
						if (lowerLeft.Y < point.Y)
							lowerLeft.Y = point.Y;
						if (upperRight.X < point.X)
							upperRight.X = point.X;
						if (upperRight.Y > point.Y)
							upperRight.Y = point.Y;
					}
					Points.Add(_newPoints);
				}

				CanonicalTileId tileId = tile.CanonicalTileId;

				LatLng ll_ll = lowerLeft.ToLngLat((ulong)tileId.Z, (ulong)tileId.X, (ulong)tileId.Y, feature.Layer.Extent);
				LatLng ll_ur = upperRight.ToLngLat((ulong)tileId.Z, (ulong)tileId.X, (ulong)tileId.Y, feature.Layer.Extent);

				bounds = new Bounds();
				bounds.SetMinMax(new Vector3((float)ll_ll.Lng, -0.0001f, (float)ll_ll.Lat),
									new Vector3((float)ll_ur.Lng, 0.0001f, (float)ll_ur.Lat));
			}
			else
			{
				_geomCount = _geom.Count;

				for (int i = 0; i < _geomCount; i++)
				{
					_pointCount = _geom[i].Count;
					_newPoints = new List<Vector3>(_pointCount);
					for (int j = 0; j < _pointCount; j++)
					{
						var point = _geom[i][j];
						_newPoints.Add(new Vector3((float)(point.X / layerExtent * _rectSizex - (_rectSizex / 2)) * tile.TileScale, 0, (float)((layerExtent - point.Y) / layerExtent * _rectSizey - (_rectSizey / 2)) * tile.TileScale));
					}
					Points.Add(_newPoints);
				}
			}
		}

		public VectorFeatureUnity(VectorTileFeature feature, List<List<Point2d<float>>> geom, UnityTile tile, float layerExtent, bool buildingsWithUniqueIds = false)
		{
			Data = feature;
			Properties = Data.GetProperties();
			Points.Clear();
			Tile = tile;
			_geom = geom;

			_rectSizex = tile.Rect.Size.x;
			_rectSizey = tile.Rect.Size.y;

			_geomCount = _geom.Count;
			for (int i = 0; i < _geomCount; i++)
			{
				_pointCount = _geom[i].Count;
				_newPoints = new List<Vector3>(_pointCount);
				for (int j = 0; j < _pointCount; j++)
				{
					var point = _geom[i][j];
					_newPoints.Add(new Vector3((float)(point.X / layerExtent * _rectSizex - (_rectSizex / 2)) * tile.TileScale, 0, (float)((layerExtent - point.Y) / layerExtent * _rectSizey - (_rectSizey / 2)) * tile.TileScale));
				}
				Points.Add(_newPoints);
			}
		}

		public bool ContainsLatLon(Vector2d coord)
		{
			////first check tile
			var coordinateTileId = Conversions.LatitudeLongitudeToTileId(
				coord.x, coord.y, Tile.CurrentZoom);

			if (Points.Count > 0)
			{
				var from = Conversions.LatLonToMeters(coord.x, coord.y);

				var to = new Vector2d((Points[0][0].x / Tile.TileScale) + Tile.Rect.Center.x, (Points[0][0].z / Tile.TileScale) + Tile.Rect.Center.y);
				var dist = Vector2d.Distance(from, to);
				if (Mathd.Abs(dist) < 50)
				{
					return true;
				}
			}


			//Debug.Log("Distance -> " + dist);
			{
				if ((!coordinateTileId.Canonical.Equals(Tile.CanonicalTileId)))
				{
					return false;
				}

				//then check polygon
				var point = Conversions.LatitudeLongitudeToVectorTilePosition(coord, Tile.CurrentZoom);
				var output = PolygonUtils.PointInPolygon(new Point2d<float>(point.x, point.y), _geom);

				return output;
			}

		}

	}
}
