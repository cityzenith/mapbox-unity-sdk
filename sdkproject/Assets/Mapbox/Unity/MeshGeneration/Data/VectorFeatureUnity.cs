namespace Mapbox.Unity.MeshGeneration.Data
{
	using Mapbox.VectorTile;
	using System.Collections.Generic;
	using Mapbox.VectorTile.Geometry;
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Utils;

	public class VectorFeatureUnity
	{
		public VectorTileFeature Data;
		public Dictionary<string, object> Properties;
		public List<List<Vector3>> Points = new List<List<Vector3>>();
		public Vector2d[] boundPoints;

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
				Point2d<float> upperLeft = firstPoint.Value;
				Point2d<float> lowerRight = firstPoint.Value;

				_geomCount = _geom.Count;

				for (int i = 0; i < _geomCount; i++)
				{
					_pointCount = _geom[i].Count;
					_newPoints = new List<Vector3>(_pointCount);
					for (int j = 0; j < _pointCount; j++)
					{
						var point = _geom[i][j];
						_newPoints.Add(new Vector3((float)(point.X / layerExtent * _rectSizex - (_rectSizex / 2)) * tile.TileScale, 0, (float)((layerExtent - point.Y) / layerExtent * _rectSizey - (_rectSizey / 2)) * tile.TileScale));

						if (upperLeft.X > point.X)
							upperLeft.X = point.X;
						if (upperLeft.Y > point.Y)
							upperLeft.Y = point.Y;
						if (lowerRight.X < point.X)
							lowerRight.X = point.X;
						if (lowerRight.Y < point.Y)
							lowerRight.Y = point.Y;
					}
					Points.Add(_newPoints);
				}

				CanonicalTileId tileId = tile.CanonicalTileId;

				LatLng ll_ul = upperLeft.ToLngLat((ulong)tileId.Z, (ulong)tileId.X, (ulong)tileId.Y, feature.Layer.Extent);
				LatLng ll_lr = lowerRight.ToLngLat((ulong)tileId.Z, (ulong)tileId.X, (ulong)tileId.Y, feature.Layer.Extent);

				boundPoints = new Vector2d[] { new Vector2d(ll_ul.Lat, ll_ul.Lng),
											   new Vector2d(ll_ul.Lat, ll_lr.Lng),
											   new Vector2d(ll_lr.Lat, ll_ul.Lng),
											   new Vector2d(ll_lr.Lat, ll_lr.Lng)};
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

		private Point2d<float>? GetFirstPoint()
		{
			_geomCount = _geom.Count;

			for (int i = 0; i < _geomCount; i++)
			{
				_pointCount = _geom[i].Count;
				for (int j = 0; j < _pointCount; j++)
				{
					return _geom[i][j];
				}
			}

			return null;
		}
	}
}
