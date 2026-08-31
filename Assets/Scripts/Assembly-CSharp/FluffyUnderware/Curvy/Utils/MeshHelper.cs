using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class MeshHelper
	{
		public static void CalculateMeshTangents(Mesh mesh)
		{
			int[] triangles = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector3[] normals = mesh.normals;
			int num = triangles.Length;
			int num2 = vertices.Length;
			Vector3[] array = new Vector3[num2];
			Vector3[] array2 = new Vector3[num2];
			Vector4[] array3 = new Vector4[num2];
			for (long num3 = 0L; num3 < num; num3 += 3)
			{
				long num4 = triangles[num3];
				long num5 = triangles[num3 + 1];
				long num6 = triangles[num3 + 2];
				Vector3 vector = vertices[num4];
				Vector3 vector2 = vertices[num5];
				Vector3 vector3 = vertices[num6];
				Vector2 vector4 = uv[num4];
				Vector2 vector5 = uv[num5];
				Vector2 vector6 = uv[num6];
				float num7 = vector2.x - vector.x;
				float num8 = vector3.x - vector.x;
				float num9 = vector2.y - vector.y;
				float num10 = vector3.y - vector.y;
				float num11 = vector2.z - vector.z;
				float num12 = vector3.z - vector.z;
				float num13 = vector5.x - vector4.x;
				float num14 = vector6.x - vector4.x;
				float num15 = vector5.y - vector4.y;
				float num16 = vector6.y - vector4.y;
				float num17 = num13 * num16 - num14 * num15;
				float num18 = ((num17 != 0f) ? (1f / num17) : 0f);
				Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num18, (num16 * num9 - num15 * num10) * num18, (num16 * num11 - num15 * num12) * num18);
				Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num18, (num13 * num10 - num14 * num9) * num18, (num13 * num12 - num14 * num11) * num18);
				array[num4] += vector7;
				array[num5] += vector7;
				array[num6] += vector7;
				array2[num4] += vector8;
				array2[num5] += vector8;
				array2[num6] += vector8;
			}
			for (long num19 = 0L; num19 < num2; num19++)
			{
				Vector3 normal = normals[num19];
				Vector3 tangent = array[num19];
				Vector3.OrthoNormalize(ref normal, ref tangent);
				array3[num19].x = tangent.x;
				array3[num19].y = tangent.y;
				array3[num19].z = tangent.z;
				array3[num19].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array2[num19]) < 0f)) ? 1f : (-1f));
			}
			mesh.tangents = array3;
		}

		private static Edge getNextEdge(int curVI, ref List<Edge> pool, out int nextVI)
		{
			foreach (Edge item in pool)
			{
				if (item.vertexIndex[0] == curVI)
				{
					nextVI = item.vertexIndex[1];
					return item;
				}
				if (item.vertexIndex[1] == curVI)
				{
					nextVI = item.vertexIndex[0];
					return item;
				}
			}
			nextVI = -1;
			Debug.LogError("Curvy Mesh Builder: Open Edge Loop detected! Please check your StartMesh!");
			return null;
		}

		internal static EdgeLoop[] BuildEdgeLoops(Edge[] manifoldEdges)
		{
			List<EdgeLoop> list = new List<EdgeLoop>();
			if (manifoldEdges.Length == 0)
			{
				return list.ToArray();
			}
			List<Edge> pool = new List<Edge>(manifoldEdges);
			List<int> list2 = new List<int>();
			list2.Add(pool[0].vertexIndex[0]);
			list2.Add(pool[0].vertexIndex[1]);
			int num = pool[0].vertexIndex[1];
			pool.RemoveAt(0);
			while (pool.Count > 0)
			{
				int nextVI;
				Edge nextEdge = getNextEdge(num, ref pool, out nextVI);
				if (nextEdge == null)
				{
					return new EdgeLoop[0];
				}
				list2.Add(nextVI);
				num = nextVI;
				pool.Remove(nextEdge);
				if (num == list2[0])
				{
					list.Add(new EdgeLoop(list2));
					list2.Clear();
					if (pool.Count > 0)
					{
						list2.Add(pool[0].vertexIndex[0]);
						list2.Add(pool[0].vertexIndex[1]);
						num = pool[0].vertexIndex[1];
						pool.RemoveAt(0);
					}
				}
			}
			if (list2.Count > 0)
			{
				list.Add(new EdgeLoop(list2));
			}
			return list.ToArray();
		}

		internal static Edge[] BuildManifoldEdges(Mesh mesh)
		{
			Edge[] array = BuildEdges(mesh.vertexCount, mesh.triangles);
			ArrayList arrayList = new ArrayList();
			Edge[] array2 = array;
			foreach (Edge edge in array2)
			{
				if (edge.faceIndex[0] == edge.faceIndex[1])
				{
					arrayList.Add(edge);
				}
			}
			return arrayList.ToArray(typeof(Edge)) as Edge[];
		}

		internal static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
		{
			int num = triangleArray.Length;
			int[] array = new int[vertexCount + num];
			int num2 = triangleArray.Length / 3;
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = -1;
			}
			Edge[] array2 = new Edge[num];
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				int num4 = triangleArray[j * 3 + 2];
				for (int k = 0; k < 3; k++)
				{
					int num5 = triangleArray[j * 3 + k];
					if (num4 < num5)
					{
						Edge edge = new Edge();
						edge.vertexIndex[0] = num4;
						edge.vertexIndex[1] = num5;
						edge.faceIndex[0] = j;
						edge.faceIndex[1] = j;
						array2[num3] = edge;
						int num6 = array[num4];
						if (num6 == -1)
						{
							array[num4] = num3;
						}
						else
						{
							while (true)
							{
								int num7 = array[vertexCount + num6];
								if (num7 == -1)
								{
									break;
								}
								num6 = num7;
							}
							array[vertexCount + num6] = num3;
						}
						array[vertexCount + num3] = -1;
						num3++;
					}
					num4 = num5;
				}
			}
			for (int l = 0; l < num2; l++)
			{
				int num8 = triangleArray[l * 3 + 2];
				for (int m = 0; m < 3; m++)
				{
					int num9 = triangleArray[l * 3 + m];
					if (num8 > num9)
					{
						bool flag = false;
						for (int num10 = array[num9]; num10 != -1; num10 = array[vertexCount + num10])
						{
							Edge edge2 = array2[num10];
							if (edge2.vertexIndex[1] == num8 && edge2.faceIndex[0] == edge2.faceIndex[1])
							{
								array2[num10].faceIndex[1] = l;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Edge edge3 = new Edge();
							edge3.vertexIndex[0] = num8;
							edge3.vertexIndex[1] = num9;
							edge3.faceIndex[0] = l;
							edge3.faceIndex[1] = l;
							array2[num3] = edge3;
							num3++;
						}
					}
					num8 = num9;
				}
			}
			Edge[] array3 = new Edge[num3];
			for (int n = 0; n < num3; n++)
			{
				array3[n] = array2[n];
			}
			return array3;
		}

		public static Mesh CreateLineMesh(float width)
		{
			if (width <= 0f)
			{
				return null;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[2]
			{
				new Vector3((0f - width) / 2f, 0f, 0f),
				new Vector3(width / 2f, 0f, 0f)
			};
			mesh.uv = new Vector2[2]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f)
			};
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Mesh CreateNgonMesh(int n, float radius, float hollowPercent)
		{
			if (n < 3)
			{
				return null;
			}
			Mesh mesh = new Mesh();
			mesh.name = "Ngon";
			float num = (float)Math.PI * 2f / (float)n;
			Vector3[] array;
			Vector2[] array2;
			int[] array3;
			if (hollowPercent == 0f)
			{
				array = new Vector3[n + 1];
				array2 = new Vector2[n + 1];
				array3 = new int[n * 3];
				array[0] = new Vector3(0f, 0f, 0f);
				array2[0] = new Vector2(0.5f, 0.5f);
				for (int i = 0; i < n; i++)
				{
					array[i + 1] = new Vector3(Mathf.Sin((float)i * num) * radius, Mathf.Cos((float)i * num) * radius, 0f);
					array2[i + 1] = new Vector2((1f + Mathf.Sin((float)i * num)) * 0.5f, (1f + Mathf.Cos((float)i * num)) * 0.5f);
				}
				for (int j = 0; j < n; j++)
				{
					array3[j * 3] = 0;
					array3[j * 3 + 1] = j + 1;
					array3[j * 3 + 2] = j + 2;
				}
				array3[n * 3 - 1] = 1;
			}
			else
			{
				array = new Vector3[n * 2];
				array2 = new Vector2[n * 2];
				array3 = new int[n * 6];
				for (int k = 0; k < n; k++)
				{
					array[k] = new Vector3(Mathf.Sin((float)k * num) * radius, Mathf.Cos((float)k * num) * radius, 0f);
					array[k + n] = new Vector3(Mathf.Sin((float)k * num) * radius * hollowPercent, Mathf.Cos((float)k * num) * radius * hollowPercent, 0f);
					array2[k] = new Vector2((1f + Mathf.Sin((float)k * num)) * 0.5f, (1f + Mathf.Cos((float)k * num)) * 0.5f);
					array2[k + n] = new Vector2((1f + Mathf.Sin((float)k * num) * hollowPercent) * 0.5f, (1f + Mathf.Cos((float)k * num) * hollowPercent) * 0.5f);
				}
				int num2 = 0;
				for (int l = 0; l < n - 1; l++)
				{
					array3[num2] = l;
					array3[num2 + 1] = l + 1;
					array3[num2 + 2] = l + n;
					array3[num2 + 3] = l + n;
					array3[num2 + 4] = l + 1;
					array3[num2 + 5] = l + n + 1;
					num2 += 6;
				}
				array3[num2] = n - 1;
				array3[num2 + 1] = 0;
				array3[num2 + 2] = n - 1 + n;
				array3[num2 + 3] = n - 1 + n;
				array3[num2 + 4] = 0;
				array3[num2 + 5] = n;
			}
			mesh.vertices = array;
			mesh.triangles = array3;
			mesh.uv = array2;
			return mesh;
		}

		public static Mesh CreateRectangleMesh(float width, float height, float hollowPercent)
		{
			if (width <= 0f || height < 0f)
			{
				return null;
			}
			Mesh mesh = new Mesh();
			float num = width / 2f;
			float num2 = height / 2f;
			if (hollowPercent <= 0f)
			{
				mesh.vertices = new Vector3[4]
				{
					new Vector3(0f - num, num2, 0f),
					new Vector3(num, num2, 0f),
					new Vector3(num, 0f - num2, 0f),
					new Vector3(0f - num, 0f - num2, 0f)
				};
				mesh.uv = new Vector2[4]
				{
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f),
					new Vector2(0f, 0f)
				};
				mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
			}
			else
			{
				float num3 = num * hollowPercent;
				float num4 = num2 * hollowPercent;
				float num5 = hollowPercent * 0.5f;
				mesh.vertices = new Vector3[8]
				{
					new Vector3(0f - num, num2, 0f),
					new Vector3(num, num2, 0f),
					new Vector3(num, 0f - num2, 0f),
					new Vector3(0f - num, 0f - num2, 0f),
					new Vector3(0f - num3, num4, 0f),
					new Vector3(num3, num4, 0f),
					new Vector3(num3, 0f - num4, 0f),
					new Vector3(0f - num3, 0f - num4, 0f)
				};
				mesh.uv = new Vector2[8]
				{
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f),
					new Vector2(0f, 0f),
					new Vector2(0.5f - num5, 0.5f + num5),
					new Vector2(0.5f + num5, 0.5f + num5),
					new Vector2(0.5f + num5, 0.5f - num5),
					new Vector2(0.5f - num5, 0.5f - num5)
				};
				mesh.triangles = new int[24]
				{
					0, 1, 5, 5, 4, 0, 5, 1, 6, 1,
					2, 6, 2, 7, 6, 7, 2, 3, 3, 4,
					7, 3, 0, 4
				};
			}
			return mesh;
		}

		[Obsolete("Use Spline2Mesh class instead")]
		public static Mesh CreateSplineMesh(CurvySpline spline, int ignoreAxis, bool close, float angleDiff)
		{
			float tf = 0f;
			int direction = 1;
			List<Vector3> list = new List<Vector3>();
			list.Add(spline.Transform.worldToLocalMatrix.MultiplyPoint3x4(spline.Interpolate(0f)));
			while (tf < 1f)
			{
				list.Add(spline.Transform.worldToLocalMatrix.MultiplyPoint3x4(spline.MoveByAngle(ref tf, ref direction, angleDiff, CurvyClamping.Clamp, 0.005f)));
			}
			return buildSplineMesh(list.ToArray(), ignoreAxis, !close);
		}

		[Obsolete("Use Spline2Mesh class instead")]
		public static Mesh CreateSplineMesh(CurvySpline spline, int ignoreAxis, bool close)
		{
			Vector3[] array = spline.GetApproximation(true);
			if (spline.Closed)
			{
				Array.Resize(ref array, array.Length - 1);
			}
			return buildSplineMesh(array, ignoreAxis, !close);
		}

		private static Mesh buildSplineMesh(Vector3[] vertices, int ignoreAxis, bool noTrisAtAll)
		{
			int[] triangles = ((!noTrisAtAll) ? new Triangulator(vertices, ignoreAxis).Triangulate() : new int[0]);
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			Vector2[] array = new Vector2[vertices.Length];
			Bounds bounds = mesh.bounds;
			switch (ignoreAxis)
			{
			case 0:
			{
				float x = bounds.min.y;
				float x2 = bounds.size.y;
				float y = bounds.min.z;
				float y2 = bounds.size.z;
				for (int j = 0; j < vertices.Length; j++)
				{
					array[j] = new Vector2((vertices[j].y - x) / x2, (vertices[j].z - y) / y2);
				}
				break;
			}
			case 1:
			{
				float x = bounds.min.x;
				float x2 = bounds.size.x;
				float y = bounds.min.z;
				float y2 = bounds.size.z;
				for (int k = 0; k < vertices.Length; k++)
				{
					array[k] = new Vector2((vertices[k].x - x) / x2, (vertices[k].z - y) / y2);
				}
				break;
			}
			default:
			{
				float x = bounds.min.x;
				float x2 = bounds.size.x;
				float y = bounds.min.y;
				float y2 = bounds.size.y;
				for (int i = 0; i < vertices.Length; i++)
				{
					array[i] = new Vector2((vertices[i].x - x) / x2, (vertices[i].y - y) / y2);
				}
				break;
			}
			}
			mesh.uv = array;
			mesh.RecalculateNormals();
			return mesh;
		}
	}
}
