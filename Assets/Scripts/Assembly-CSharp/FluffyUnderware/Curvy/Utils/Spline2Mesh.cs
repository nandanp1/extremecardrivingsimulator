using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.ThirdParty.poly2tri;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class Spline2Mesh
	{
		public SplinePolyLine Outline;

		public List<SplinePolyLine> Holes = new List<SplinePolyLine>();

		public Vector2 UVTiling = Vector2.one;

		public Vector2 UVOffset = Vector2.zero;

		public bool SuppressUVMapping;

		public bool UV2;

		public string MeshName = string.Empty;

		public bool VertexLineOnly;

		private bool mUseMeshBounds;

		private Vector2 mNewBounds;

		private Polygon p2t;

		private Mesh mMesh;

		public string Error { get; private set; }

		public bool Apply(out Mesh result)
		{
			p2t = null;
			mMesh = null;
			Error = string.Empty;
			if (triangulate() && buildMesh())
			{
				if (!SuppressUVMapping)
				{
					uvmap();
				}
				result = mMesh;
				return true;
			}
			if ((bool)mMesh)
			{
				mMesh.RecalculateNormals();
			}
			result = mMesh;
			return false;
		}

		public void SetBounds(bool useMeshBounds, Vector2 newSize)
		{
			mUseMeshBounds = useMeshBounds;
			mNewBounds = newSize;
		}

		private bool triangulate()
		{
			if (Outline == null || Outline.Spline == null)
			{
				Error = "Missing Outline Spline";
				return false;
			}
			if (!polyLineIsValid(Outline))
			{
				Error = Outline.Spline.name + ": Angle must be >0";
				return false;
			}
			Vector3[] vertices = Outline.getVertices();
			if (vertices.Length < 3)
			{
				Error = Outline.Spline.name + ": At least 3 Vertices needed!";
				return false;
			}
			p2t = new Polygon(vertices);
			if (VertexLineOnly)
			{
				return true;
			}
			for (int i = 0; i < Holes.Count; i++)
			{
				if (Holes[i].Spline == null)
				{
					Error = "Missing Hole Spline";
					return false;
				}
				if (!polyLineIsValid(Holes[i]))
				{
					Error = Holes[i].Spline.name + ": Angle must be >0";
					return false;
				}
				Vector3[] vertices2 = Holes[i].getVertices();
				if (vertices2.Length < 3)
				{
					Error = Holes[i].Spline.name + ": At least 3 Vertices needed!";
					return false;
				}
				p2t.AddHole(new Polygon(vertices2));
			}
			try
			{
				P2T.Triangulate(p2t);
				return true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
			return false;
		}

		private bool polyLineIsValid(SplinePolyLine pl)
		{
			return (pl != null && pl.VertexMode == SplinePolyLine.VertexCalculation.ByApproximation) || !Mathf.Approximately(0f, pl.Angle);
		}

		private bool buildMesh()
		{
			mMesh = new Mesh();
			mMesh.name = MeshName;
			if (VertexLineOnly)
			{
				mMesh.vertices = Outline.getVertices();
			}
			else
			{
				List<Vector3> list = new List<Vector3>();
				List<int> list2 = new List<int>();
				for (int i = 0; i < p2t.Triangles.Count; i++)
				{
					DelaunayTriangle delaunayTriangle = p2t.Triangles[i];
					for (int j = 0; j < 3; j++)
					{
						if (!list.Contains(delaunayTriangle.Points[j].V3))
						{
							list.Add(delaunayTriangle.Points[j].V3);
						}
					}
					list2.Add(list.IndexOf(delaunayTriangle.Points[2].V3));
					list2.Add(list.IndexOf(delaunayTriangle.Points[1].V3));
					list2.Add(list.IndexOf(delaunayTriangle.Points[0].V3));
				}
				mMesh.vertices = list.ToArray();
				mMesh.triangles = list2.ToArray();
			}
			mMesh.RecalculateBounds();
			return true;
		}

		private void uvmap()
		{
			Bounds bounds = mMesh.bounds;
			Vector2 vector = bounds.size;
			if (!mUseMeshBounds)
			{
				vector = mNewBounds;
			}
			Vector3[] vertices = mMesh.vertices;
			Vector2[] array = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				float num = UVOffset.x + (vertices[i].x - bounds.min.x) / vector.x;
				float num2 = UVOffset.y + (vertices[i].y - bounds.min.y) / vector.y;
				num *= UVTiling.x;
				num2 *= UVTiling.y;
				array[i] = new Vector2(num, num2);
			}
			mMesh.uv = array;
			mMesh.uv2 = ((!UV2) ? new Vector2[0] : new Vector2[array.Length]);
			mMesh.RecalculateNormals();
		}
	}
}
