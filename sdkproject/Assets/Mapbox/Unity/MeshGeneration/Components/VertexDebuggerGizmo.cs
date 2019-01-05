﻿using UnityEngine;

namespace Mapbox.Unity.MeshGeneration.Components
{
	public class VertexDebuggerGizmo : MonoBehaviour
	{
		[SerializeField]
		float _radius = .2f;

		[SerializeField]
		Color _color = new Color(0, 1, 0, .5f);

		[Multiline(10)]
		public string Triangles;

		Mesh _mesh;

		protected virtual void Start()
		{
			if (!MapboxHelper.IsEditor)
				return;

			var mf = GetComponent<MeshFilter>();
			if (mf)
			{
				_mesh = mf.mesh;
				var tris = _mesh.triangles;
				Triangles = "";
				for (int i = 0; i < tris.Length; i += 3)
				{
					Triangles += tris[i] + "," + tris[i + 1] + "," + tris[i + 2] + "\r\n";
				}
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			if (!MapboxHelper.IsEditor)
				return;

			if (_mesh)
			{
				var verts = _mesh.vertices;
				for (int i = 0; i < verts.Length; i++)
				{
					Gizmos.color = _color;
					Gizmos.DrawWireCube(transform.position + verts[i] * transform.lossyScale.x, Vector3.one * _radius);
				}
			}
		}
	}
}
