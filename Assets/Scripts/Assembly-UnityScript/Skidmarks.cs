using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Skidmarks : MonoBehaviour
{
	public int maxMarks;

	public float markWidth;

	public float groundOffset;

	public float minDistance;

	private int indexShift;

	private int numMarks;

	private markSection[] skidmarks;

	private bool updated;

	public Skidmarks()
	{
		maxMarks = 1024;
		markWidth = 0.275f;
		groundOffset = 0.02f;
		minDistance = 0.1f;
	}

	public virtual void Start()
	{
		if (transform.position != new Vector3(0f, 0f, 0f))
		{
			transform.position = new Vector3(0f, 0f, 0f);
		}
	}

	public virtual void Awake()
	{
		skidmarks = new markSection[maxMarks];
		for (int i = 0; i < maxMarks; i++)
		{
			skidmarks[i] = new markSection();
		}
		if (((MeshFilter)GetComponent(typeof(MeshFilter))).mesh == null)
		{
			((MeshFilter)GetComponent(typeof(MeshFilter))).mesh = new Mesh();
		}
	}

	public virtual int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex)
	{
		if (!(intensity <= 1f))
		{
			intensity = 1f;
		}
		int result;
		if (!(intensity >= 0f))
		{
			result = -1;
		}
		else
		{
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
				markSection2.posl = markSection2.pos + normalized * markWidth * 0.5f;
				markSection2.posr = markSection2.pos - normalized * markWidth * 0.5f;
				markSection2.tangent = new Vector4(normalized.x, normalized.y, normalized.z, 1f);
				if (markSection3.lastIndex == -1)
				{
					markSection3.tangent = markSection2.tangent;
					markSection3.posl = markSection2.pos + normalized * markWidth * 0.5f;
					markSection3.posr = markSection2.pos - normalized * markWidth * 0.5f;
				}
			}
			numMarks++;
			updated = true;
			result = numMarks - 1;
		}
		return result;
	}

	public virtual void LateUpdate()
	{
		if (!updated)
		{
			return;
		}
		updated = false;
		Mesh mesh = ((MeshFilter)GetComponent(typeof(MeshFilter))).mesh;
		mesh.Clear();
		int num = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
			{
				num++;
			}
		}
		Vector3[] array = new Vector3[num * 4];
		Vector3[] array2 = new Vector3[num * 4];
		Vector4[] array3 = new Vector4[num * 4];
		Color[] array4 = new Color[num * 4];
		Vector2[] array5 = new Vector2[num * 4];
		int[] array6 = new int[num * 6];
		num = 0;
		for (int j = 0; j < numMarks && j < maxMarks; j++)
		{
			if (skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
			{
				markSection markSection2 = skidmarks[j];
				markSection markSection3 = skidmarks[markSection2.lastIndex % maxMarks];
				array[num * 4 + 0] = markSection3.posl;
				array[num * 4 + 1] = markSection3.posr;
				array[num * 4 + 2] = markSection2.posl;
				array[num * 4 + 3] = markSection2.posr;
				array2[num * 4 + 0] = markSection3.normal;
				array2[num * 4 + 1] = markSection3.normal;
				array2[num * 4 + 2] = markSection2.normal;
				array2[num * 4 + 3] = markSection2.normal;
				array3[num * 4 + 0] = markSection3.tangent;
				array3[num * 4 + 1] = markSection3.tangent;
				array3[num * 4 + 2] = markSection2.tangent;
				array3[num * 4 + 3] = markSection2.tangent;
				array4[num * 4 + 0] = new Color(0f, 0f, 0f, markSection3.intensity);
				array4[num * 4 + 1] = new Color(0f, 0f, 0f, markSection3.intensity);
				array4[num * 4 + 2] = new Color(0f, 0f, 0f, markSection2.intensity);
				array4[num * 4 + 3] = new Color(0f, 0f, 0f, markSection2.intensity);
				array5[num * 4 + 0] = new Vector2(0f, 0f);
				array5[num * 4 + 1] = new Vector2(1f, 0f);
				array5[num * 4 + 2] = new Vector2(0f, 1f);
				array5[num * 4 + 3] = new Vector2(1f, 1f);
				array6[num * 6 + 0] = num * 4 + 0;
				array6[num * 6 + 2] = num * 4 + 1;
				array6[num * 6 + 1] = num * 4 + 2;
				array6[num * 6 + 3] = num * 4 + 2;
				array6[num * 6 + 5] = num * 4 + 1;
				array6[num * 6 + 4] = num * 4 + 3;
				num++;
			}
		}
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.tangents = array3;
		mesh.triangles = array6;
		mesh.colors = array4;
		mesh.uv = array5;
	}

	public virtual void Main()
	{
	}
}
