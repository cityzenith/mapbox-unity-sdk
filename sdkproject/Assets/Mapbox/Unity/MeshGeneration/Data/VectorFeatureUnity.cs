namespace Mapbox.Unity.MeshGeneration.Data
{
	using Mapbox.VectorTile;
	using System.Collections.Generic;
	using Mapbox.VectorTile.Geometry;
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using Mapbox.Map;

	public class VectorFeatureUnity
	{
		public VectorTileFeature Data;
		public Dictionary<string, object> Properties;
		public List<List<Vector3>> Points = new List<List<Vector3>>();
		public UnityTile Tile;
		public double[] latLong;

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
				Point2d<float> value = firstPoint.Value;
				Point2d<float> value2 = firstPoint.Value;
				_geomCount = _geom.Count;

				for (int i = 0; i < _geomCount; i++)
				{
					_pointCount = _geom[i].Count;
					_newPoints = new List<Vector3>(_pointCount);
					for (int j = 0; j < _pointCount; j++)
					{
						var point = _geom[i][j];
						_newPoints.Add(new Vector3((float)(point.X / layerExtent * _rectSizex - (_rectSizex / 2)) * tile.TileScale, 0, (float)((layerExtent - point.Y) / layerExtent * _rectSizey - (_rectSizey / 2)) * tile.TileScale));

						if (value.X > point.X)
							value.X = point.X;
						if (value.Y < point.Y)
							value.Y = point.Y;
						if (value2.X < point.X)
							value2.X = point.X;
						if (value2.Y > point.Y)
							value2.Y = point.Y;
					}
					Points.Add(_newPoints);
				}

				Point2d<float> center = new Point2d<float>((value.X + value2.X) * 0.5f, (value.Y + value2.Y) * 0.5f);
				CanonicalTileId canonicalTileId = tile.CanonicalTileId;
				LatLng ll = center.ToLngLat((ulong)canonicalTileId.Z, (ulong)canonicalTileId.X, (ulong)canonicalTileId.Y, feature.Layer.Extent);

				latLong = new double[] { ll.Lat, ll.Lng };
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

		private Point2d<float>? GetFirstPoint()
		{
			_geomCount = _geom.Count;
			for (int i = 0; i < _geomCount; i++)
			{
				_pointCount = _geom[i].Count;
				int num = 0;
				if (num < _pointCount)
				{
					return _geom[i][num];
				}
			}
			return null;
		}
	}
}
