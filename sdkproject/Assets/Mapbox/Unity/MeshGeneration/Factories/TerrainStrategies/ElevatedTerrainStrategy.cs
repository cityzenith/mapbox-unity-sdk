using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Map;
using Mapbox.Utils;
using Debug = UnityEngine.Debug;
using System;
using Mapbox.Unity.Utilities;
using System.Collections;

namespace Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies
{
	public class MeshDataArray
	{
		public List<Vector3> Vertices = new List<Vector3>();
		public List<Vector3> Normals = new List<Vector3>();
		public List<int> Triangles = new List<int>();
		public List<int> Triangles2 = new List<int>();
		public List<Vector2> Uvs = new List<Vector2>();

		public void Cleanup()
		{
			Vertices.Clear();
			Vertices = null;
			Normals.Clear();
			Normals = null;
			Triangles.Clear();
			Triangles = null;
			Triangles2.Clear();
			Triangles2 = null;
			Uvs.Clear();
			Uvs = null;
		}
	}

	public class ElevatedTerrainStrategy : TerrainStrategy, IElevationBasedTerrainStrategy
	{
		private Dictionary<UnwrappedTileId, Mesh> _meshData;
		private MeshData _currentTileMeshData;
		private Dictionary<UnityTile, MeshDataArray> _cachedMeshDataArrays;
		private Dictionary<UnwrappedTileId, MeshDataArray> _dataArrays;
		private Dictionary<int, MeshDataArray> _meshSamples;

		private Vector3 _newDir;
		private int _vertA, _vertB, _vertC;
		private int _counter;

		public override int RequiredVertexCount
		{
			get { return _elevationOptions.modificationOptions.sampleCount * _elevationOptions.modificationOptions.sampleCount; }
		}

		public override void Initialize(ElevationLayerProperties elOptions)
		{
			base.Initialize(elOptions);

			_meshSamples = new Dictionary<int, MeshDataArray>();
			_dataArrays = new Dictionary<UnwrappedTileId, MeshDataArray>();
			_cachedMeshDataArrays = new Dictionary<UnityTile, MeshDataArray>();

			_meshData = new Dictionary<UnwrappedTileId, Mesh>();
			_currentTileMeshData = new MeshData();
			var sampleCountSquare = _elevationOptions.modificationOptions.sampleCount * _elevationOptions.modificationOptions.sampleCount;
		}

		public override void RegisterTile(UnityTile tile)
		{
			if (_elevationOptions.unityLayerOptions.addToLayer && tile.gameObject.layer != _elevationOptions.unityLayerOptions.layerId)
			{
				tile.gameObject.layer = _elevationOptions.unityLayerOptions.layerId;
			}

			if (tile.MeshFilter.sharedMesh.vertexCount != RequiredVertexCount || !_cachedMeshDataArrays.ContainsKey(tile))
			{
				Mesh mesh = tile.MeshFilter.sharedMesh;
				mesh.Clear();

				if (_meshSamples.ContainsKey(_elevationOptions.modificationOptions.sampleCount))
				{
					var newMesh = _meshSamples[_elevationOptions.modificationOptions.sampleCount];
					mesh.SetVertices(newMesh.Vertices);
					mesh.SetNormals(newMesh.Normals);
					mesh.SetTriangles(newMesh.Triangles, 0);
					mesh.SetUVs(0, newMesh.Uvs);
				}
				else
				{
					//TODO remoev tile dependency from CreateBaseMesh method
					var newMesh = CreateBaseMesh(tile, _elevationOptions.modificationOptions.sampleCount);
					_meshSamples.Add(_elevationOptions.modificationOptions.sampleCount, newMesh);
					mesh.SetVertices(newMesh.Vertices);
					mesh.SetNormals(newMesh.Normals);
					mesh.SetTriangles(newMesh.Triangles, 0);
					mesh.SetUVs(0, newMesh.Uvs);
				}

				if (!_dataArrays.ContainsKey(tile.UnwrappedTileId))
				{
					MeshDataArray meshDataArray = new MeshDataArray();
					mesh.GetVertices(meshDataArray.Vertices);
					mesh.GetNormals(meshDataArray.Normals);
					mesh.GetTriangles(meshDataArray.Triangles, 0);

					_dataArrays.Add(tile.UnwrappedTileId, meshDataArray);
				}
			}
			else
			{
				_dataArrays.Add(tile.UnwrappedTileId, _cachedMeshDataArrays[tile]);
				_cachedMeshDataArrays.Remove(tile);
			}

			tile.ElevationType = TileTerrainType.Elevated;

			GenerateTerrainMesh(tile);
		}

		public override void UnregisterTile(UnityTile tile)
		{
			_meshData.Remove(tile.UnwrappedTileId);
			if (_dataArrays.ContainsKey(tile.UnwrappedTileId))
			{
				_cachedMeshDataArrays.Add(tile, _dataArrays[tile.UnwrappedTileId]);
				_dataArrays.Remove(tile.UnwrappedTileId);
			}
		}

		public override void DataErrorOccurred(UnityTile t, TileErrorEventArgs e)
		{
			ResetToFlatMesh(t);
		}

		public override void PostProcessTile(UnityTile tile)
		{

		}

		#region mesh gen

		private MeshDataArray CreateBaseMesh(UnityTile tile, int sampleCount)
		{
			//TODO use arrays instead of lists
			var mesh = new MeshDataArray();

			for (float y = 0; y < sampleCount; y++)
			{
				var yrat = y / (sampleCount - 1);
				for (float x = 0; x < sampleCount; x++)
				{
					var xrat = x / (sampleCount - 1);

					var xx = Mathd.Lerp(tile.Rect.Min.x, tile.Rect.Max.x, xrat);
					var yy = Mathd.Lerp(tile.Rect.Min.y, tile.Rect.Max.y, yrat);

					mesh.Vertices.Add(new Vector3(
						(float) (xx - tile.Rect.Center.x) * tile.TileScale,
						0,
						(float) (yy - tile.Rect.Center.y) * tile.TileScale));
					mesh.Normals.Add(Mapbox.Unity.Constants.Math.Vector3Up);
					mesh.Uvs.Add(new Vector2(x * 1f / (sampleCount - 1), 1 - (y * 1f / (sampleCount - 1))));
				}
			}

			int vertA, vertB, vertC;
			for (int y = 0; y < sampleCount - 1; y++)
			{
				for (int x = 0; x < sampleCount - 1; x++)
				{
					vertA = (y * sampleCount) + x;
					vertB = (y * sampleCount) + x + sampleCount + 1;
					vertC = (y * sampleCount) + x + sampleCount;
					mesh.Triangles.Add(vertA);
					mesh.Triangles.Add(vertB);
					mesh.Triangles.Add(vertC);

					vertA = (y * sampleCount) + x;
					vertB = (y * sampleCount) + x + 1;
					vertC = (y * sampleCount) + x + sampleCount + 1;
					mesh.Triangles.Add(vertA);
					mesh.Triangles.Add(vertB);
					mesh.Triangles.Add(vertC);
				}
			}

			return mesh;
		}

		private void GenerateTerrainMesh(UnityTile tile)
		{
			List<Vector3> verts = _dataArrays[tile.UnwrappedTileId].Vertices;
			List<Vector3> _normals = _dataArrays[tile.UnwrappedTileId].Normals;

			var _sampleCount = _elevationOptions.modificationOptions.sampleCount;
			var hd = tile.HeightData;
			var ts = tile.TileScale;
			int index = 0;
			Vector3 vect;
			float invSampleCount = 1f / (_sampleCount - 1);

			for (float y = 0; y < _sampleCount; y++)
			{
				for (float x = 0; x < _sampleCount; x++)
				{
					index = (int)(y * _sampleCount + x);
					vect = verts[index];
					vect.y = hd[((int)((1 - y * invSampleCount) * 255) * 256) + ((int)(x * invSampleCount * 255))] * ts;
					verts[index] = vect;

					_normals[index] = Mapbox.Unity.Constants.Math.Vector3Zero;
				}
			}
			
			FixStitches(tile.UnwrappedTileId, verts, _normals);

			Mesh mesh = tile.MeshFilter.sharedMesh;

			mesh.SetVertices(verts);
			mesh.RecalculateNormals();

			mesh.RecalculateBounds();

			if (!_meshData.ContainsKey(tile.UnwrappedTileId))
			{
				_meshData.Add(tile.UnwrappedTileId, mesh);
			}

			if (_elevationOptions.colliderOptions.addCollider)
			{
				var meshCollider = tile.Collider as MeshCollider;
				if (meshCollider)
				{
					meshCollider.sharedMesh = mesh;
				}
			}

			verts = null;
			_normals = null;
		}

		private void ResetToFlatMesh(UnityTile tile)
		{
			if (tile.MeshFilter.sharedMesh.vertexCount == 0)
			{
				CreateBaseMesh(tile, _elevationOptions.modificationOptions.sampleCount);
			}
			else
			{
				tile.MeshFilter.sharedMesh.GetVertices(_currentTileMeshData.Vertices);
				tile.MeshFilter.sharedMesh.GetNormals(_currentTileMeshData.Normals);

				_counter = _currentTileMeshData.Vertices.Count;
				for (int i = 0; i < _counter; i++)
				{
					_currentTileMeshData.Vertices[i].Set(
						_currentTileMeshData.Vertices[i].x,
						0,
						_currentTileMeshData.Vertices[i].z);
					_currentTileMeshData.Normals[i] = Mapbox.Unity.Constants.Math.Vector3Up;
				}

				tile.MeshFilter.sharedMesh.SetVertices(_currentTileMeshData.Vertices);
				tile.MeshFilter.sharedMesh.SetNormals(_currentTileMeshData.Normals);

				tile.MeshFilter.sharedMesh.RecalculateBounds();
			}
		}

		/// <summary>
		/// Checkes all neighbours of the given tile and stitches the edges to achieve a smooth mesh surface.
		/// </summary>
		/// <param name="tileId">UnwrappedTileId of the tile being processed.</param>
		/// <param name="mesh"></param>
		private void FixStitches(UnwrappedTileId tileId, List<Vector3> verts, List<Vector3> normals)
		{
			var _sampleCount = _elevationOptions.modificationOptions.sampleCount;
			var meshVertCount = verts.Count;
			int index = 0, index2 = 0, i = 0;

			List<Vector3> targetVerts = null;
			List<Vector3> targetNormals = null;

			if (_dataArrays.ContainsKey(tileId.North))
			{
				targetVerts = _dataArrays[tileId.North].Vertices;
				targetNormals = _dataArrays[tileId.North].Normals;

				for (i = 0; i < _sampleCount; i++)
				{
					index = meshVertCount - _sampleCount + i;
					//just snapping the y because vertex pos is relative and we'll have to do tile pos + vertex pos for x&z otherwise
					verts[i].Set(
						verts[i].x,
						targetVerts[index].y,
						verts[i].z);

					normals[i].Set(targetNormals[index].x,
						targetNormals[index].y,
						targetNormals[index].z);
				}
			}

			if (_dataArrays.ContainsKey(tileId.South))
			{
				targetVerts = _dataArrays[tileId.South].Vertices;
				targetNormals = _dataArrays[tileId.South].Normals;

				for (i = 0; i < _sampleCount; i++)
				{
					index = meshVertCount - _sampleCount + i;

					verts[index].Set(
						verts[index].x,
						targetVerts[i].y,
						verts[index].z);

					normals[index].Set(
						targetNormals[i].x,
						targetNormals[i].y,
						targetNormals[i].z);
				}
			}

			if (_dataArrays.ContainsKey(tileId.West))
			{
				targetVerts = _dataArrays[tileId.West].Vertices;
				targetNormals = _dataArrays[tileId.West].Normals;

				for (i = 0; i < _sampleCount; i++)
				{
					index = i * _sampleCount;
					index2 = index + _sampleCount - 1;

					verts[index].Set(
						verts[index].x,
						targetVerts[index2].y,
						verts[index].z);

					normals[index].Set(
						targetNormals[index2].x,
						targetNormals[index2].y,
						targetNormals[index2].z);
				}
			}

			if (_dataArrays.ContainsKey(tileId.East))
			{
				targetVerts = _dataArrays[tileId.East].Vertices;
				targetNormals = _dataArrays[tileId.East].Normals;

				for (i = 0; i < _sampleCount; i++)
				{
					index = i * _sampleCount;
					index2 = index + _sampleCount - 1;
					verts[index2].Set(
						verts[index2].x,
						targetVerts[index].y,
						verts[index2].z);

					normals[index2].Set(
						targetNormals[index].x,
						targetNormals[index].y,
						targetNormals[index].z);
				}
			}

			if (_dataArrays.ContainsKey(tileId.NorthWest))
			{
				targetVerts = _dataArrays[tileId.NorthWest].Vertices;
				targetNormals = _dataArrays[tileId.NorthWest].Normals;

				index = meshVertCount - 1;

				verts[0].Set(
					verts[0].x,
					targetVerts[index].y,
					verts[0].z);

				normals[0].Set(
					targetNormals[index].x,
					targetNormals[index].y,
					targetNormals[index].z);
			}

			if (_dataArrays.ContainsKey(tileId.NorthEast))
			{
				index = _sampleCount - 1;
				index2 = meshVertCount - _sampleCount;

				targetVerts = _dataArrays[tileId.NorthEast].Vertices;
				targetNormals = _dataArrays[tileId.NorthEast].Normals;

				verts[index].Set(
					verts[index].x,
					targetVerts[index2].y,
					verts[index].z);

				normals[index].Set(
					targetNormals[index2].x,
					targetNormals[index2].y,
					targetNormals[index2].z);
			}

			if (_dataArrays.ContainsKey(tileId.SouthWest))
			{
				index = _sampleCount - 1;
				index2 = meshVertCount - _sampleCount;

				targetVerts = _dataArrays[tileId.SouthWest].Vertices;
				targetNormals = _dataArrays[tileId.SouthWest].Normals;

				verts[index2].Set(
					verts[index2].x,
					targetVerts[index].y,
					verts[index2].z);

				normals[index2].Set(
					targetNormals[index].x,
					targetNormals[index].y,
					targetNormals[index].z);
			}

			if (_dataArrays.ContainsKey(tileId.SouthEast))
			{
				index = meshVertCount - 1;

				targetVerts = _dataArrays[tileId.SouthEast].Vertices;
				targetNormals = _dataArrays[tileId.SouthEast].Normals;

				verts[index].Set(
					verts[index].x,
					targetVerts[0].y,
					verts[index].z);

				normals[index].Set(
					targetNormals[0].x,
					targetNormals[0].y,
					targetNormals[0].z);
			}

			targetVerts = null;
			targetNormals = null;
		}

		#endregion
	}
}
