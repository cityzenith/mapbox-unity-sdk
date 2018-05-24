namespace Mapbox.Unity.MeshGeneration.Data
{
	using Mapbox.VectorTile;
	using System.Collections.Generic;
	using Mapbox.VectorTile.Geometry;
	using UnityEngine;
	using Mapbox.Map;

	public class VectorFeatureUnity
	{
		public VectorTileFeature Data;
		public Dictionary<string, object> Properties;
		public List<List<Vector3>> Points = new List<List<Vector3>>();
		public double[] CenterGeom;


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

			Point2d<float> upperLeft = _geom[0][0];
			Point2d<float> lowerRight = _geom[0][0];


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

			Point2d<float> center = new Point2d<float>((upperLeft.X + lowerRight.X) * 0.5f, (upperLeft.Y + lowerRight.Y) * 0.5f);

			CanonicalTileId tileId = tile.CanonicalTileId;

			LatLng latLng = center.ToLngLat((ulong)tileId.Z, (ulong)tileId.X, (ulong)tileId.Y, feature.Layer.Extent);

			CenterGeom = new double[] { latLng.Lat, latLng.Lng };
		}
	}
}
