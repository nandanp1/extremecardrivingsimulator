using UnityEngine;

public class Skidmarks : MonoBehaviour
{
	public int maxMarks = 1024;

	public float groundOffset = 0.02f;

	public float minDistance = 0.1f;

	private int indexShift;

	private int numMarks;

	private markSection[] skidmarks;

	private bool updated;

	private Mesh m_mesh;

	private Vector3[] m_vertices;

	private Vector3[] m_normals;

	private Vector4[] m_tangents;

	private Color[] m_colors;

	private Vector2[] m_uvs;

	private int[] m_triangles;

	private void OnEnable()
	{
		maxMarks = Mathf.Max(2, maxMarks);
		skidmarks = new markSection[maxMarks];
		for (int i = 0; i < maxMarks; i++)
		{
			skidmarks[i] = new markSection();
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component.mesh == null)
		{
			component.mesh = new Mesh();
		}
		m_mesh = component.mesh;
		m_mesh.MarkDynamic();
		m_vertices = new Vector3[maxMarks * 4];
		m_normals = new Vector3[maxMarks * 4];
		m_tangents = new Vector4[maxMarks * 4];
		m_colors = new Color[maxMarks * 4];
		m_uvs = new Vector2[maxMarks * 4];
		m_triangles = new int[maxMarks * 6];
	}

	public int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, float width, int lastIndex)
	{
		if (intensity > 1f)
		{
			intensity = 1f;
		}
		if (intensity < 0f)
		{
			return -1;
		}
		markSection markSection2 = skidmarks[numMarks % maxMarks];
		markSection2.pos = pos + normal * groundOffset;
		markSection2.normal = normal;
		markSection2.intensity = intensity;
		markSection2.lastIndex = lastIndex;
		if (lastIndex != -1)
		{
			markSection markSection3 = skidmarks[lastIndex % maxMarks];
			Vector3 lhs = markSection2.pos - markSection3.pos;
			Vector3 normalized = Vector3.Cross(lhs, normal).normalized;
			markSection2.posl = markSection2.pos + normalized * width * 0.5f;
			markSection2.posr = markSection2.pos - normalized * width * 0.5f;
			markSection2.tangent = new Vector4(normalized.x, normalized.y, normalized.z, 1f);
			if (markSection3.lastIndex == -1)
			{
				markSection3.tangent = markSection2.tangent;
				markSection3.posl = markSection2.pos + normalized * width * 0.5f;
				markSection3.posr = markSection2.pos - normalized * width * 0.5f;
			}
		}
		numMarks++;
		updated = true;
		return numMarks - 1;
	}

	private void LateUpdate()
	{
		if (!updated)
		{
			return;
		}
		updated = false;
		m_mesh.Clear(false);
		int num = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
			{
				num++;
			}
		}
		int sectionCount = num;
		num = 0;
		for (int j = 0; j < numMarks && j < maxMarks; j++)
		{
			if (skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
			{
				markSection markSection2 = skidmarks[j];
				markSection markSection3 = skidmarks[markSection2.lastIndex % maxMarks];
				m_vertices[num * 4] = markSection3.posl;
				m_vertices[num * 4 + 1] = markSection3.posr;
				m_vertices[num * 4 + 2] = markSection2.posl;
				m_vertices[num * 4 + 3] = markSection2.posr;
				m_normals[num * 4] = markSection3.normal;
				m_normals[num * 4 + 1] = markSection3.normal;
				m_normals[num * 4 + 2] = markSection2.normal;
				m_normals[num * 4 + 3] = markSection2.normal;
				m_tangents[num * 4] = markSection3.tangent;
				m_tangents[num * 4 + 1] = markSection3.tangent;
				m_tangents[num * 4 + 2] = markSection2.tangent;
				m_tangents[num * 4 + 3] = markSection2.tangent;
				float num2 = Mathf.InverseLerp(numMarks - maxMarks, numMarks - maxMarks / 2, markSection2.lastIndex);
				m_colors[num * 4] = new Color(0f, 0f, 0f, markSection3.intensity * num2);
				m_colors[num * 4 + 1] = new Color(0f, 0f, 0f, markSection3.intensity * num2);
				m_colors[num * 4 + 2] = new Color(0f, 0f, 0f, markSection2.intensity * num2);
				m_colors[num * 4 + 3] = new Color(0f, 0f, 0f, markSection2.intensity * num2);
				m_uvs[num * 4] = new Vector2(0f, 0f);
				m_uvs[num * 4 + 1] = new Vector2(1f, 0f);
				m_uvs[num * 4 + 2] = new Vector2(0f, 1f);
				m_uvs[num * 4 + 3] = new Vector2(1f, 1f);
				m_triangles[num * 6] = num * 4;
				m_triangles[num * 6 + 2] = num * 4 + 1;
				m_triangles[num * 6 + 1] = num * 4 + 2;
				m_triangles[num * 6 + 3] = num * 4 + 2;
				m_triangles[num * 6 + 5] = num * 4 + 1;
				m_triangles[num * 6 + 4] = num * 4 + 3;
				num++;
			}
		}
		int vertexCount = sectionCount * 4;
		m_mesh.SetVertices(m_vertices, 0, vertexCount);
		m_mesh.SetNormals(m_normals, 0, vertexCount);
		m_mesh.SetTangents(m_tangents, 0, vertexCount);
		m_mesh.SetColors(m_colors, 0, vertexCount);
		m_mesh.SetUVs(0, m_uvs, 0, vertexCount);
		m_mesh.SetIndices(m_triangles, 0, sectionCount * 6, MeshTopology.Triangles, 0, true);
	}
}
