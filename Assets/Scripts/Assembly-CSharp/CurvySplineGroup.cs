using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CurvySplineGroup : CurvySplineBase
{
	public List<CurvySpline> Splines = new List<CurvySpline>();

	private bool mIsContinuous;

	private bool mIsClosed;

	public float[] Distances { get; private set; }

	public int Count
	{
		get
		{
			return Splines.Count;
		}
	}

	public CurvySpline this[int idx]
	{
		get
		{
			return (idx <= -1 || idx >= Splines.Count) ? null : Splines[idx];
		}
	}

	public override bool IsContinuous
	{
		get
		{
			return mIsContinuous;
		}
	}

	public override bool IsClosed
	{
		get
		{
			return mIsContinuous && mIsClosed;
		}
	}

	public static CurvySplineGroup Create(params CurvySpline[] splines)
	{
		CurvySplineGroup component = new GameObject("Curvy Spline Group", typeof(CurvySplineGroup)).GetComponent<CurvySplineGroup>();
		component.Add(splines);
		return component;
	}

	private IEnumerator Start()
	{
		for (int i = 0; i < Count; i++)
		{
			while (!this[i].IsInitialized)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		RefreshImmediately(true, true, false);
	}

	private void OnDisable()
	{
		foreach (CurvySpline spline in Splines)
		{
			spline.OnRefresh -= OnSplineRefresh;
		}
	}

	private void Update()
	{
		for (int i = 0; i < Count; i++)
		{
			this[i].OnRefresh -= OnSplineRefresh;
			this[i].OnRefresh += OnSplineRefresh;
		}
		if (mNeedLengthRefresh || mNeedOrientationRefresh)
		{
			RefreshImmediately(mNeedLengthRefresh, mNeedOrientationRefresh, mSkipRefreshIfInitialized);
		}
	}

	public override Vector3 Interpolate(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.Interpolate(localTF);
	}

	public override Vector3 InterpolateFast(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.InterpolateFast(localTF);
	}

	public override Vector3 InterpolateUserValue(float tf, int index)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.InterpolateUserValue(localTF, index);
	}

	public override Vector3 InterpolateScale(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.InterpolateScale(localTF);
	}

	public override Vector3 GetOrientationUpFast(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.GetOrientationUpFast(localTF);
	}

	public override Quaternion GetOrientationFast(float tf, bool inverse)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.GetOrientationFast(localTF, inverse);
	}

	public override Vector3 GetTangent(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.GetTangent(localTF);
	}

	public override Vector3 GetTangent(float tf, Vector3 position)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.GetTangent(localTF, position);
	}

	public override Vector3 GetTangentFast(float tf)
	{
		float localTF;
		CurvySpline curvySpline = TFToSpline(tf, out localTF);
		return curvySpline.GetTangentFast(localTF);
	}

	public override Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
	{
		return MoveByLengthFast(ref tf, ref direction, distance, clamping);
	}

	public override Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
	{
		return MoveByLengthFast(ref tf, ref direction, distance, clamping);
	}

	public override float TFToDistance(float tf)
	{
		float localTF;
		int num = TFToSplineIndex(tf, out localTF);
		return Distances[num] + this[num].TFToDistance(localTF);
	}

	public CurvySpline TFToSpline(float tf)
	{
		float localTF;
		int num = TFToSplineIndex(tf, out localTF);
		return (num != -1) ? this[num] : null;
	}

	public CurvySpline TFToSpline(float tf, out float localTF)
	{
		int num = TFToSplineIndex(tf, out localTF);
		return (num != -1) ? this[num] : null;
	}

	public float SplineToTF(CurvySpline spline, float splineTF)
	{
		return (float)Splines.IndexOf(spline) / (float)Count + 1f / (float)Count * splineTF;
	}

	public override CurvySplineSegment TFToSegment(float tf)
	{
		float localTF;
		int num = TFToSplineIndex(tf, out localTF);
		return (num != -1) ? this[num].TFToSegment(localTF) : null;
	}

	public override CurvySplineSegment TFToSegment(float tf, out float localF)
	{
		localF = 0f;
		float localTF;
		int num = TFToSplineIndex(tf, out localTF);
		return (num != -1) ? this[num].TFToSegment(localTF, out localF) : null;
	}

	public override float SegmentToTF(CurvySplineSegment segment)
	{
		return SplineToTF(segment.Spline, 0f);
	}

	public override float SegmentToTF(CurvySplineSegment segment, float localF)
	{
		float splineTF = segment.LocalFToTF(localF);
		return SplineToTF(segment.Spline, splineTF);
	}

	public override float DistanceToTF(float distance)
	{
		float localDistance;
		CurvySpline curvySpline = DistanceToSpline(distance, out localDistance);
		return SplineToTF(curvySpline, curvySpline.DistanceToTF(localDistance));
	}

	public CurvySpline DistanceToSpline(float distance)
	{
		float localDistance;
		return DistanceToSpline(distance, out localDistance);
	}

	public CurvySpline DistanceToSpline(float distance, out float localDistance)
	{
		distance = Mathf.Clamp(distance, 0f, base.Length);
		localDistance = 0f;
		for (int i = 1; i < Count; i++)
		{
			if (Distances[i] >= distance)
			{
				localDistance = distance - Distances[i - 1];
				return this[i - 1];
			}
		}
		localDistance = distance - Distances[Count - 1];
		return this[Count - 1];
	}

	public void Add(params CurvySpline[] splines)
	{
		Splines.AddRange(splines);
		Refresh();
	}

	public void Delete(CurvySpline spline)
	{
		Splines.Remove(spline);
		Refresh();
	}

	public override void Clear()
	{
		Splines.Clear();
		Refresh();
	}

	public override Vector3[] GetApproximation(bool local)
	{
		List<Vector3[]> list = new List<Vector3[]>(Count);
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			Vector3[] array = this[i].GetApproximation(local);
			if (NextSplineConnected(i))
			{
				Array.Resize(ref array, array.Length - 1);
			}
			list.Add(array);
			num += list[i].Length;
		}
		Vector3[] array2 = new Vector3[num];
		num = 0;
		for (int j = 0; j < Count; j++)
		{
			list[j].CopyTo(array2, num);
			num += list[j].Length;
		}
		return array2;
	}

	public override Vector3[] GetApproximationT()
	{
		List<Vector3[]> list = new List<Vector3[]>(Count);
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			Vector3[] array = this[i].GetApproximationT();
			if (NextSplineConnected(i))
			{
				Array.Resize(ref array, array.Length - 1);
			}
			list.Add(array);
			num += list[i].Length;
		}
		Vector3[] array2 = new Vector3[num];
		num = 0;
		for (int j = 0; j < Count; j++)
		{
			list[j].CopyTo(array2, num);
			num += list[j].Length;
		}
		return array2;
	}

	public override Vector3[] GetApproximationUpVectors()
	{
		List<Vector3[]> list = new List<Vector3[]>(Count);
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			Vector3[] array = this[i].GetApproximationUpVectors();
			if (NextSplineConnected(i))
			{
				Array.Resize(ref array, array.Length - 1);
			}
			list.Add(array);
			num += list[i].Length;
		}
		Vector3[] array2 = new Vector3[num];
		num = 0;
		for (int j = 0; j < Count; j++)
		{
			list[j].CopyTo(array2, num);
			num += list[j].Length;
		}
		return array2;
	}

	public override float GetNearestPointTF(Vector3 p)
	{
		float splineTF = -1f;
		float num = float.MaxValue;
		int idx = -1;
		for (int i = 0; i < Count; i++)
		{
			float nearestPointTF = this[i].GetNearestPointTF(p);
			float sqrMagnitude = (p - this[i].Interpolate(nearestPointTF)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				idx = i;
				splineTF = nearestPointTF;
				num = sqrMagnitude;
			}
		}
		return SplineToTF(this[idx], splineTF);
	}

	public override void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
	{
		_RemoveEmptySplines();
		mLength = 0f;
		Distances = new float[Count];
		for (int i = 0; i < Count; i++)
		{
			this[i].OnRefresh -= OnSplineRefresh;
			this[i].RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized);
			this[i].OnRefresh += OnSplineRefresh;
			Distances[i] = mLength;
			mLength += this[i].Length;
		}
		OnRefreshEvent(this);
		mIsInitialized = Count > 0;
		base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized);
	}

	private void doRefreshLength()
	{
		mLength = 0f;
		Distances = new float[Count];
		for (int i = 0; i < Count; i++)
		{
			Distances[i] = mLength;
			mLength += this[i].Length;
		}
		OnRefreshEvent(this);
	}

	private bool NextSplineConnected(int idx)
	{
		Mathf.Clamp(idx, 0, Count - 1);
		int idx2 = ((idx != Count - 1) ? (idx + 1) : 0);
		return idx < Count - 1 && Mathf.Abs((this[idx].InterpolateFast(1f) - this[idx2].InterpolateFast(0f)).sqrMagnitude) <= 0f;
	}

	private void OnSplineRefresh(CurvySplineBase spl)
	{
		if (!Splines.Contains((CurvySpline)spl))
		{
			spl.OnRefresh -= OnSplineRefresh;
		}
		else
		{
			if (!mIsInitialized)
			{
				return;
			}
			doRefreshLength();
			mIsContinuous = true;
			for (int i = 0; i < Count - 1; i++)
			{
				if (!NextSplineConnected(i))
				{
					mIsContinuous = false;
					break;
				}
			}
			mIsClosed = Count > 1 && NextSplineConnected(Count - 1);
		}
	}

	public void _RemoveEmptySplines()
	{
		if (Splines.Count <= 0)
		{
			return;
		}
		for (int num = Splines.Count - 1; num > -1; num--)
		{
			if (Splines[num] == null)
			{
				Splines.RemoveAt(num);
			}
		}
	}

	private int TFToSplineIndex(float tf, out float localTF)
	{
		tf = Mathf.Clamp01(tf);
		localTF = 0f;
		if (Count == 0)
		{
			return -1;
		}
		float num = tf * (float)Count;
		int num2 = (int)num;
		localTF = num - (float)num2;
		if (num2 == Count)
		{
			num2--;
			localTF = 1f;
		}
		return num2;
	}
}
