using System;
using UnityEngine;

[RequireComponent(typeof(CurvySpline))]
[ExecuteInEditMode]
public class SplineShaper : MonoBehaviour
{
	public enum ModifierMode
	{
		None = 0,
		Absolute = 1,
		Relative = 2
	}

	public int Resolution = 10;

	public float Range = 1f;

	public float Radius = 5f;

	public ModifierMode RadiusModifier;

	public AnimationCurve RadiusModifierCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float Z;

	public ModifierMode ZModifier;

	public AnimationCurve ZModifierCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public string Name = string.Empty;

	public float m;

	public float n1;

	public float n2;

	public float n3;

	public float a;

	public float b;

	public float WeldThreshold = 0.1f;

	public bool AutoRefresh = true;

	public float AutoRefreshSpeed;

	private CurvySpline mSpline;

	private float mLastRefresh;

	private bool mNeedRefresh;

	public CurvySpline Spline
	{
		get
		{
			if (!mSpline)
			{
				mSpline = GetComponent<CurvySpline>();
			}
			return mSpline;
		}
	}

	public static SplineShaper Create()
	{
		GameObject gameObject = new GameObject("SplineShape", typeof(SplineShaper));
		return gameObject.GetComponent<SplineShaper>();
	}

	public void Reset()
	{
		Name = string.Empty;
		Resolution = 20;
		Range = 1f;
		Radius = 5f;
		RadiusModifier = ModifierMode.None;
		RadiusModifierCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
		Z = 0f;
		ZModifier = ModifierMode.None;
		ZModifierCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
		Z = 0f;
		m = 0f;
		n1 = 1f;
		n2 = 3f;
		n3 = 4f;
		a = 1f;
		b = 1f;
		WeldThreshold = 0.1f;
		AutoRefresh = true;
		AutoRefreshSpeed = 0f;
	}

	private void Update()
	{
		if ((bool)Spline && Spline.IsInitialized && (mNeedRefresh || (AutoRefresh && Time.realtimeSinceStartup - mLastRefresh > AutoRefreshSpeed)))
		{
			mLastRefresh = Time.realtimeSinceStartup;
			RefreshImmediately();
		}
	}

	public void Refresh()
	{
		mNeedRefresh = true;
	}

	public void RefreshImmediately()
	{
		mNeedRefresh = false;
		Resolution = Mathf.Max(2, Resolution);
		int num = mSpline.ControlPointCount;
		int num2 = 0;
		float num3 = Range * (float)Math.PI * 2f;
		float num4 = num3 / (float)Resolution;
		double num5 = 0.0;
		float num6 = WeldThreshold * WeldThreshold;
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 zero = Vector3.zero;
		float zz = base.transform.position.z + Z;
		float radius = Radius;
		for (float num7 = 0f; num7 < num3; num7 += num4)
		{
			getEvaluatedValues(num7 / num3, ref radius, ref zz);
			num5 = Math.Pow((double)Mathf.Pow(Mathf.Abs(Mathf.Cos(m * num7 / 4f) / a), n2) + Math.Pow(Mathf.Abs(Mathf.Sin(m * num7 / 4f) / b), n3), 0f - 1f / n1);
			zero.x = (float)(num5 * (double)Mathf.Cos(num7) * (double)radius);
			zero.y = (float)(num5 * (double)Mathf.Sin(num7) * (double)radius);
			zero.z = zz;
			if ((zero - vector).sqrMagnitude >= num6)
			{
				CurvySplineSegment curvySplineSegment = ((num2 >= num) ? Spline.Add(null, false) : Spline.ControlPoints[num2]);
				num2++;
				curvySplineSegment.Transform.localPosition = zero;
				vector = zero;
			}
		}
		if (num > num2)
		{
			while (num > num2)
			{
				Spline.ControlPoints[--num].Delete();
			}
		}
		Spline.Refresh();
	}

	private void getEvaluatedValues(float percent, ref float radius, ref float zz)
	{
		switch (RadiusModifier)
		{
		case ModifierMode.Absolute:
			radius = RadiusModifierCurve.Evaluate(percent);
			break;
		case ModifierMode.Relative:
			radius = Radius * RadiusModifierCurve.Evaluate(percent);
			break;
		default:
			radius = Radius;
			break;
		}
		switch (ZModifier)
		{
		case ModifierMode.Absolute:
			zz = ZModifierCurve.Evaluate(percent);
			break;
		case ModifierMode.Relative:
			zz = Z * ZModifierCurve.Evaluate(percent);
			break;
		default:
			zz = Z;
			break;
		}
	}
}
