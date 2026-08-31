using System;
using UnityEngine;

[Serializable]
public class markSection
{
	public Vector3 pos;

	public Vector3 normal;

	public Vector4 tangent;

	public Vector3 posl;

	public Vector3 posr;

	public float intensity;

	public int lastIndex;

	public markSection()
	{
		pos = Vector3.zero;
		normal = Vector3.zero;
		tangent = Vector4.zero;
		posl = Vector3.zero;
		posr = Vector3.zero;
	}
}
