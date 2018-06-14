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

		public bool ContainsLatLon(Vector2d coord)
		{
			////first check tile
			var coordinateTileId = Conversions.LatitudeLongitudeToTileId(
				coord.x, coord.y, Tile.InitialZoom);
			if (!coordinateTileId.Canonical.Equals(Tile.CanonicalTileId))
			{
				return false;
			}

			var point = Conversions.LatitudeLongitudeToVectorTilePosition(coord, Tile.InitialZoom);
			var output = PointInPolygon(new Point2d<float>(point.x, point.y), _geom);

			return output;
		}

		/// <summary>
		/// Method to check if a point is contained inside a polygon, ignores vertical axis (y axis),
		/// </summary>
		/// <returns><c>true</c>, if point lies inside the constructed polygon, <c>false</c> otherwise.</returns>
		/// <param name="polyPoints">Polygon points.</param>
		/// <param name="p">The point that is to be tested.</param>
		private bool PointInPolygon(Point2d<float> coord, List<List<Point2d<float>>> poly)
		{
			var point = new InternalClipper.IntPoint(coord.X, coord.Y);
			List<InternalClipper.IntPoint> polygon = new List<InternalClipper.IntPoint>();

			foreach (var vert in poly[0])
			{
				polygon.Add(new InternalClipper.IntPoint(vert.X, vert.Y));
			}

			//then check the actual polygon
			int result = InternalClipper.Clipper.PointInPolygon(point, polygon);
			return (result == 1) ? true : false;
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
