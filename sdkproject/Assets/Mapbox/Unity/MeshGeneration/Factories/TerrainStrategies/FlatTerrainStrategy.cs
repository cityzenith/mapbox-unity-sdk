using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

namespace Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies
{
	public class FlatTerrainStrategy : TerrainStrategy
	{
		MeshDataArray _cachedQuad;

		public override int RequiredVertexCount
		{
			get { return 4; }
		}

		public override void Initialize(ElevationLayerProperties elOptions)
		{
			_elevationOptions = elOptions;
		}

		public override void RegisterTile(UnityTile tile)
		{
			if (_elevationOptions.unityLayerOptions.addToLayer && tile.gameObject.layer != _elevationOptions.unityLayerOptions.layerId)
			{
				tile.gameObject.layer = _elevationOptions.unityLayerOptions.layerId;
			}

			if (tile.RasterDataState != Enums.TilePropertyState.Loaded ||
			    tile.MeshFilter.sharedMesh.vertexCount != RequiredVertexCount)
			{
				if (_elevationOptions.sideWallOptions.isActive)
				{
					var firstMat = tile.MeshRenderer.materials[0];
					tile.MeshRenderer.materials = new Material[2]
					{
						firstMat,
						_elevationOptions.sideWallOptions.wallMaterial
					};
				}
			}

			if ((int)tile.ElevationType != (int)ElevationLayerType.FlatTerrain)
			{
				tile.MeshFilter.sharedMesh.Clear();
				// HACK: This is here in to make the system trigger a finished state.
				GetQuad(tile, _elevationOptions.sideWallOptions.isActive);
				tile.ElevationType = TileTerrainType.Flat;
			}
		}

		private void GetQuad(UnityTile tile, bool buildSide)
		{
			if (_cachedQuad != null)
			{
				var mesh = tile.MeshFilter.sharedMesh;
				mesh.SetVertices(_cachedQuad.Vertices);
				mesh.SetNormals(_cachedQuad.Normals);
				mesh.SetTriangles(_cachedQuad.Triangles, 0);
				if (buildSide)
					mesh.SetTriangles(_cachedQuad.Triangles2, 1);
				mesh.SetUVs(0, _cachedQuad.Uvs);
			}
			else
			{
				if (buildSide)
				{
					BuildQuadWithSides(tile);
				}
				else
				{
					BuildQuad(tile);
				}
			}
		}

		private void BuildQuad(UnityTile tile)
		{
			var unityMesh = tile.MeshFilter.sharedMesh;
			_cachedQuad = new MeshDataArray();

			_cachedQuad.Vertices.Add(tile.TileScale * ((tile.Rect.Min - tile.Rect.Center).ToVector3xz()));
			_cachedQuad.Vertices.Add(tile.TileScale * (new Vector3((float)(tile.Rect.Max.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Min.y - tile.Rect.Center.y))));
			_cachedQuad.Vertices.Add(tile.TileScale * ((tile.Rect.Max - tile.Rect.Center).ToVector3xz()));
			_cachedQuad.Vertices.Add(tile.TileScale * (new Vector3((float)(tile.Rect.Min.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Max.y - tile.Rect.Center.y))));
			_cachedQuad.Normals.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			_cachedQuad.Normals.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			_cachedQuad.Normals.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			_cachedQuad.Normals.Add(Mapbox.Unity.Constants.Math.Vector3Up);

			unityMesh.SetVertices(_cachedQuad.Vertices);
			unityMesh.SetNormals(_cachedQuad.Normals);

			_cachedQuad.Triangles = new List<int>() { 0, 1, 2, 0, 2, 3 };
			unityMesh.SetTriangles(_cachedQuad.Triangles, 0);

			_cachedQuad.Uvs.Add(new Vector2(0, 1));
			_cachedQuad.Uvs.Add(new Vector2(1, 1));
			_cachedQuad.Uvs.Add(new Vector2(1, 0));
			_cachedQuad.Uvs.Add(new Vector2(0, 0));

			unityMesh.SetUVs(0, _cachedQuad.Uvs);
		}

		private void BuildQuadWithSides(UnityTile tile)
		{
			var unityMesh = tile.MeshFilter.sharedMesh;
			_cachedQuad = new MeshDataArray();

			var verts = _cachedQuad.Vertices;
			var norms = _cachedQuad.Normals;

			verts.Add(tile.TileScale * ((tile.Rect.Min - tile.Rect.Center).ToVector3xz()));
			verts.Add(tile.TileScale * (new Vector3((float)(tile.Rect.Max.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Min.y - tile.Rect.Center.y))));
			verts.Add(tile.TileScale * ((tile.Rect.Max - tile.Rect.Center).ToVector3xz()));
			verts.Add(tile.TileScale * (new Vector3((float)(tile.Rect.Min.x - tile.Rect.Center.x), 0, (float)(tile.Rect.Max.y - tile.Rect.Center.y))));
			norms.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			norms.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			norms.Add(Mapbox.Unity.Constants.Math.Vector3Up);
			norms.Add(Mapbox.Unity.Constants.Math.Vector3Up);

			//verts goes
			//01
			//32
			unityMesh.subMeshCount = 2;
			Vector3 norm = Mapbox.Unity.Constants.Math.Vector3Up;
			for (int i = 0; i < 4; i++)
			{
				verts.Add(verts[i]);
				verts.Add(verts[i + 1]);
				verts.Add(verts[i] + new Vector3(0, -_elevationOptions.sideWallOptions.wallHeight, 0));
				verts.Add(verts[i + 1] + new Vector3(0, -_elevationOptions.sideWallOptions.wallHeight, 0));

				norm = Vector3.Cross(verts[4 * (i + 1) + 1] - verts[4 * (i + 1) + 2], verts[4 * (i + 1)] - verts[4 * (i + 1) + 1]).normalized;
				norms.Add(norm);
				norms.Add(norm);
				norms.Add(norm);
				norms.Add(norm);
			}

			unityMesh.SetVertices(_cachedQuad.Vertices);
			unityMesh.SetNormals(_cachedQuad.Normals);

			var trilist = new List<int>(6) { 0, 1, 2, 0, 2, 3 };
			unityMesh.SetTriangles(trilist, 0);
			_cachedQuad.Triangles = trilist;

			trilist = new List<int>(8);

			for (int i = 0; i < 4; i++)
			{
				trilist.Add(4 * (i + 1));
				trilist.Add(4 * (i + 1) + 2);
				trilist.Add(4 * (i + 1) + 1);

				trilist.Add(4 * (i + 1) + 1);
				trilist.Add(4 * (i + 1) + 2);
				trilist.Add(4 * (i + 1) + 3);
			}

			unityMesh.SetTriangles(trilist, 1);
			_cachedQuad.Triangles2 = trilist;

			var uvlist = _cachedQuad.Uvs;
			uvlist.Add(new Vector2(0, 1));
			uvlist.Add(new Vector2(1, 1));
			uvlist.Add(new Vector2(1, 0));
			uvlist.Add(new Vector2(0, 0));

			for (int i = 4; i < 20; i += 4)
			{
				uvlist.Add(new Vector2(1, 1));
				uvlist.Add(new Vector2(0, 1));
				uvlist.Add(new Vector2(1, 0));
				uvlist.Add(new Vector2(0, 0));
			}

			unityMesh.SetUVs(0, _cachedQuad.Uvs);
		}
	}
}
