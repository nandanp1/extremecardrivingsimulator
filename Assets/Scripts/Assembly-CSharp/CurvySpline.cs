using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CurvySpline : CurvySplineBase
{
	public const string Version = "1.61f";

	public CurvyInterpolation Interpolation = CurvyInterpolation.CatmulRom;

	public bool Closed;

	public bool AutoEndTangents = true;

	public CurvyInitialUpDefinition InitialUpVector;

	public CurvyOrientation Orientation = CurvyOrientation.Tangent;

	public bool SetControlPointRotation;

	public CurvyOrientationSwirl Swirl;

	public float SwirlTurns;

	public int Granularity = 20;

	public bool AutoRefresh = true;

	public bool AutoRefreshLength = true;

	public bool AutoRefreshOrientation = true;

	public int UserValueSize;

	public float Tension;

	public float Continuity;

	public float Bias;

	public static Color GizmoColor = Color.red;

	public static Color GizmoSelectionColor = Color.white;

	public static float GizmoControlPointSize = 0.15f;

	public static float GizmoOrientationLength = 1f;

	public static CurvySplineGizmos Gizmos = CurvySplineGizmos.Curve | CurvySplineGizmos.Orientation | CurvySplineGizmos.Network;

	public bool ShowGizmos = true;

	private List<CurvySplineSegment> mControlPoints = new List<CurvySplineSegment>();

	private List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>();

	private float mStepSize;

	public int Count
	{
		get
		{
			return mSegments.Count;
		}
	}

	public int ControlPointCount
	{
		get
		{
			return mControlPoints.Count;
		}
	}

	public CurvySplineSegment this[int idx]
	{
		get
		{
			return (idx <= -1 || idx >= mSegments.Count) ? null : mSegments[idx];
		}
	}

	public List<CurvySplineSegment> Segments
	{
		get
		{
			return mSegments;
		}
	}

	public List<CurvySplineSegment> ControlPoints
	{
		get
		{
			return mControlPoints;
		}
	}

	public CurvySplineSegment FirstVisibleControlPoint
	{
		get
		{
			return (Count <= 0) ? null : this[0];
		}
	}

	public CurvySplineSegment LastVisibleControlPoint
	{
		get
		{
			return (Count <= 0) ? null : this[Count - 1].NextControlPoint;
		}
	}

	public override bool IsContinuous
	{
		get
		{
			return true;
		}
	}

	public override bool IsClosed
	{
		get
		{
			return Closed;
		}
	}

	private int ApproximationPointCount
	{
		get
		{
			return Count * (Granularity + 1);
		}
	}

	public static CurvySpline Create()
	{
		return new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
	}

	public static CurvySpline Create(CurvySpline takeOptionsFrom)
	{
		CurvySpline curvySpline = Create();
		if ((bool)takeOptionsFrom)
		{
			curvySpline.Interpolation = takeOptionsFrom.Interpolation;
			curvySpline.Closed = takeOptionsFrom.Closed;
			curvySpline.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
			curvySpline.Granularity = takeOptionsFrom.Granularity;
			curvySpline.Orientation = takeOptionsFrom.Orientation;
			curvySpline.InitialUpVector = takeOptionsFrom.InitialUpVector;
			curvySpline.Swirl = takeOptionsFrom.Swirl;
			curvySpline.SwirlTurns = takeOptionsFrom.SwirlTurns;
			curvySpline.AutoRefresh = takeOptionsFrom.AutoRefresh;
			curvySpline.AutoRefreshLength = takeOptionsFrom.AutoRefreshLength;
			curvySpline.AutoRefreshOrientation = takeOptionsFrom.AutoRefreshOrientation;
			curvySpline.UserValueSize = takeOptionsFrom.UserValueSize;
		}
		return curvySpline;
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
		}
	}

	private void OnDisable()
	{
		mIsInitialized = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < ControlPointCount; i++)
		{
			ControlPoints[i].ClearConnections();
		}
	}

	private void Start()
	{
		Refresh(AutoRefreshLength, AutoRefreshOrientation, true);
	}

	private void Update()
	{
		if (Application.isPlaying && !base.IsInitialized)
		{
			RefreshImmediately(AutoRefreshLength, AutoRefreshOrientation, false);
		}
		if (!Application.isPlaying || AutoRefresh)
		{
			bool flag = false;
			for (int i = 0; i < mControlPoints.Count; i++)
			{
				if (mControlPoints[i] != null && (mControlPoints[i]._PositionHasChanged() || mControlPoints[i]._RotationHasChanged()))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (!Application.isPlaying)
				{
					Refresh(true, true, false);
				}
				else
				{
					Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
				}
			}
		}
		if (mNeedLengthRefresh || mNeedOrientationRefresh)
		{
			RefreshImmediately(mNeedLengthRefresh, mNeedOrientationRefresh, mSkipRefreshIfInitialized);
		}
	}

	public override Vector3 Interpolate(float tf)
	{
		return Interpolate(tf, Interpolation);
	}

	public override Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.Interpolate(localF, interpolation);
	}

	public override Vector3 InterpolateFast(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.InterpolateFast(localF);
	}

	public override Vector3 InterpolateUserValue(float tf, int index)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.InterpolateUserValue(localF, index);
	}

	public override Vector3 InterpolateScale(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.InterpolateScale(localF);
	}

	public override Vector3 GetOrientationUpFast(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.GetOrientationUpFast(localF);
	}

	public override Quaternion GetOrientationFast(float tf, bool inverse)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.GetOrientationFast(localF, inverse);
	}

	public override Vector3 GetTangent(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.GetTangent(localF);
	}

	public override Vector3 GetTangent(float tf, Vector3 position)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.GetTangent(localF, ref position);
	}

	public override Vector3 GetTangentFast(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.GetTangentFast(localF);
	}

	public override float TFToDistance(float tf)
	{
		float localF;
		CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
		return curvySplineSegment.Distance + curvySplineSegment.LocalFToDistance(localF);
	}

	public override CurvySplineSegment TFToSegment(float tf)
	{
		float localF;
		return TFToSegment(tf, out localF);
	}

	public override CurvySplineSegment TFToSegment(float tf, out float localF)
	{
		tf = Mathf.Clamp01(tf);
		localF = 0f;
		if (Count == 0)
		{
			return null;
		}
		float num = tf * (float)Count;
		int num2 = (int)num;
		localF = num - (float)num2;
		if (num2 == Count)
		{
			num2--;
			localF = 1f;
		}
		return this[num2];
	}

	public override float DistanceToTF(float distance)
	{
		if (base.Length > 0f && distance >= base.Length)
		{
			return 1f;
		}
		float localDistance;
		CurvySplineSegment curvySplineSegment = DistanceToSegment(distance, out localDistance);
		return (!curvySplineSegment) ? 0f : SegmentToTF(curvySplineSegment, curvySplineSegment.DistanceToLocalF(localDistance));
	}

	public CurvySplineSegment DistanceToSegment(float distance)
	{
		float localDistance;
		return DistanceToSegment(distance, out localDistance);
	}

	public CurvySplineSegment DistanceToSegment(float distance, out float localDistance)
	{
		distance = Mathf.Clamp(distance, 0f, base.Length);
		localDistance = 0f;
		CurvySplineSegment curvySplineSegment = mSegments[0];
		if (distance == base.Length)
		{
			curvySplineSegment = this[Count - 1];
			localDistance = curvySplineSegment.Distance + curvySplineSegment.Length;
			return curvySplineSegment;
		}
		while ((bool)curvySplineSegment && curvySplineSegment.Distance + curvySplineSegment.Length < distance)
		{
			curvySplineSegment = NextSegment(curvySplineSegment);
		}
		if (curvySplineSegment == null)
		{
			curvySplineSegment = this[Count - 1];
		}
		localDistance = distance - curvySplineSegment.Distance;
		return curvySplineSegment;
	}

	public List<CurvyConnection> GetConnectionsWithin(float tf, int direction, float fDistance, int minMatchesNeeded, bool skipCurrent, params string[] tags)
	{
		List<CurvyConnection> list = new List<CurvyConnection>();
		int num = 0;
		int num2 = -1;
		float num3 = fDistance * (float)direction;
		float localF;
		float localF2;
		if (num3 >= 0f)
		{
			num = TFToSegment(tf, out localF).ControlPointIndex;
			num2 = TFToSegment(tf + num3, out localF2).ControlPointIndex;
			if (localF > 0f)
			{
				num++;
			}
			if (localF2 == 1f)
			{
				num2 = Mathf.Min(ControlPointCount - 1, num2 + 1);
			}
			if (num == num2 && localF == 0f && skipCurrent)
			{
				return list;
			}
		}
		else
		{
			num = TFToSegment(tf + num3, out localF).ControlPointIndex;
			num2 = TFToSegment(tf, out localF2).ControlPointIndex;
			if (num == num2)
			{
				if (localF > 0f)
				{
					return list;
				}
			}
			else
			{
				if (localF > 0f)
				{
					num++;
				}
				if (localF2 == 0f && skipCurrent)
				{
					return list;
				}
			}
		}
		for (int i = num; i <= num2; i++)
		{
			list.AddRange(ControlPoints[i].GetAllConnections(minMatchesNeeded, tags));
		}
		return list;
	}

	public Vector3 MoveConnection(ref CurvySpline spline, ref float tf, ref int direction, float fDistance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
	{
		List<CurvyConnection> connectionsWithin = GetConnectionsWithin(tf, direction, fDistance, minMatchesNeeded, true, tags);
		if (connectionsWithin.Count > 0)
		{
			CurvyConnection curvyConnection = ((connectionsWithin.Count != 1) ? CurvyConnection.GetBestMatchingConnection(connectionsWithin, tags) : connectionsWithin[0]);
			CurvySplineSegment fromSpline = curvyConnection.GetFromSpline(this);
			float num = SegmentToTF(fromSpline);
			fDistance -= num - tf;
			CurvySplineSegment counterpart = curvyConnection.GetCounterpart(fromSpline);
			tf = counterpart.LocalFToTF(0f);
			spline = counterpart.Spline;
			return spline.MoveConnection(ref spline, ref tf, ref direction, fDistance, clamping, minMatchesNeeded, tags);
		}
		return spline.Move(ref tf, ref direction, fDistance, clamping);
	}

	public Vector3 MoveConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float fDistance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
	{
		List<CurvyConnection> connectionsWithin = GetConnectionsWithin(tf, direction, fDistance, minMatchesNeeded, true, tags);
		if (connectionsWithin.Count > 0)
		{
			CurvyConnection curvyConnection = ((connectionsWithin.Count != 1) ? CurvyConnection.GetBestMatchingConnection(connectionsWithin, tags) : connectionsWithin[0]);
			CurvySplineSegment fromSpline = curvyConnection.GetFromSpline(this);
			float num = SegmentToTF(fromSpline);
			fDistance -= num - tf;
			CurvySplineSegment counterpart = curvyConnection.GetCounterpart(fromSpline);
			tf = counterpart.LocalFToTF(0f);
			spline = counterpart.Spline;
			return spline.MoveConnectionFast(ref spline, ref tf, ref direction, fDistance, clamping, minMatchesNeeded, tags);
		}
		return MoveFast(ref tf, ref direction, fDistance, clamping);
	}

	public Vector3 MoveByConnection(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
	{
		return MoveByConnection(ref spline, ref tf, ref direction, distance, clamping, minMatchesNeeded, 0.002f, tags);
	}

	public Vector3 MoveByConnection(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, float stepSize, params string[] tags)
	{
		return MoveConnection(ref spline, ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping, minMatchesNeeded, tags);
	}

	public Vector3 MoveByConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, params string[] tags)
	{
		return MoveByConnectionFast(ref spline, ref tf, ref direction, distance, clamping, minMatchesNeeded, 0.002f, tags);
	}

	public Vector3 MoveByConnectionFast(ref CurvySpline spline, ref float tf, ref int direction, float distance, CurvyClamping clamping, int minMatchesNeeded, float stepSize, params string[] tags)
	{
		return MoveConnectionFast(ref spline, ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping, minMatchesNeeded, tags);
	}

	public CurvySplineSegment Add()
	{
		return Add(null, true);
	}

	public CurvySplineSegment Add(CurvySplineSegment insertAfter)
	{
		return Add(insertAfter, true);
	}

	public CurvySplineSegment[] Add(params Vector3[] controlPoints)
	{
		CurvySplineSegment[] array = new CurvySplineSegment[controlPoints.Length];
		for (int i = 0; i < controlPoints.Length; i++)
		{
			array[i] = Add(null, false);
			array[i].Position = controlPoints[i];
		}
		_RenameControlPointsByIndex();
		RefreshImmediately();
		return array;
	}

	public CurvySplineSegment Add(CurvySplineSegment insertAfter, bool refresh)
	{
		GameObject gameObject = new GameObject("NewCP", typeof(CurvySplineSegment));
		gameObject.transform.parent = base.transform;
		CurvySplineSegment component = gameObject.GetComponent<CurvySplineSegment>();
		int index = mControlPoints.Count;
		if ((bool)insertAfter)
		{
			if (insertAfter.IsValidSegment)
			{
				gameObject.transform.position = insertAfter.Interpolate(0.5f);
			}
			else if ((bool)insertAfter.NextTransform)
			{
				gameObject.transform.position = Vector3.Lerp(insertAfter.NextTransform.position, insertAfter.Transform.position, 0.5f);
			}
			index = insertAfter.ControlPointIndex + 1;
		}
		mControlPoints.Insert(index, component);
		_RenameControlPointsByIndex();
		if (refresh)
		{
			RefreshImmediately();
		}
		return component;
	}

	public CurvySplineSegment Add(bool refresh, CurvySplineSegment insertBefore)
	{
		GameObject gameObject = new GameObject("NewCP", typeof(CurvySplineSegment));
		gameObject.transform.parent = base.transform;
		CurvySplineSegment component = gameObject.GetComponent<CurvySplineSegment>();
		int index = 0;
		if ((bool)insertBefore)
		{
			if ((bool)insertBefore.PreviousSegment)
			{
				gameObject.transform.position = insertBefore.PreviousSegment.Interpolate(0.5f);
			}
			else if ((bool)insertBefore.PreviousTransform)
			{
				gameObject.transform.position = Vector3.Lerp(insertBefore.PreviousTransform.position, insertBefore.Transform.position, 0.5f);
			}
			index = Mathf.Max(0, insertBefore.ControlPointIndex);
		}
		mControlPoints.Insert(index, component);
		_RenameControlPointsByIndex();
		if (refresh)
		{
			RefreshImmediately();
		}
		return component;
	}

	public override void Clear()
	{
		foreach (CurvySplineSegment mControlPoint in mControlPoints)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(mControlPoint.gameObject);
			}
			else
			{
				Object.DestroyImmediate(mControlPoint.gameObject);
			}
		}
		mControlPoints.Clear();
		RefreshImmediately();
	}

	public void Delete(CurvySplineSegment controlPoint)
	{
		Delete(controlPoint, true);
	}

	public void Delete(CurvySplineSegment controlPoint, bool refreshSpline)
	{
		mControlPoints.Remove(controlPoint);
		controlPoint.name = "pendingDelete";
		if (Application.isPlaying)
		{
			Object.Destroy(controlPoint.gameObject);
		}
		else
		{
			Object.DestroyImmediate(controlPoint.gameObject);
		}
		_RenameControlPointsByIndex();
		if (refreshSpline)
		{
			RefreshImmediately();
		}
	}

	public override Vector3[] GetApproximation()
	{
		return GetApproximation(false);
	}

	public override Vector3[] GetApproximation(bool local)
	{
		Vector3[] array = new Vector3[Count * Granularity + 1];
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			this[i].Approximation.CopyTo(array, num);
			num += Mathf.Max(0, this[i].Approximation.Length - 1);
		}
		if (local)
		{
			Matrix4x4 worldToLocalMatrix = base.Transform.worldToLocalMatrix;
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = worldToLocalMatrix.MultiplyPoint3x4(array[j]);
			}
		}
		return array;
	}

	public override Vector3[] GetApproximationT()
	{
		Vector3[] array = new Vector3[Count * Granularity + 1];
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			this[i].ApproximationT.CopyTo(array, num);
			num += Mathf.Max(0, this[i].ApproximationT.Length - 1);
		}
		return array;
	}

	public override Vector3[] GetApproximationUpVectors()
	{
		Vector3[] array = new Vector3[Count * Granularity + 1];
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			this[i].ApproximationUp.CopyTo(array, num);
			num += Mathf.Max(0, this[i].ApproximationUp.Length - 1);
		}
		return array;
	}

	public override float GetNearestPointTF(Vector3 p)
	{
		return GetNearestPointTFExt(p, Segments.ToArray());
	}

	public float GetNearestPointTFExt(Vector3 p, params CurvySplineSegment[] segmentsToCheck)
	{
		if (Count == 0 || segmentsToCheck.Length == 0)
		{
			return -1f;
		}
		float num = float.MaxValue;
		int num2 = 0;
		CurvySplineSegment curvySplineSegment = null;
		for (int i = 0; i < segmentsToCheck.Length; i++)
		{
			for (int j = 0; j < segmentsToCheck[i].Approximation.Length; j++)
			{
				float sqrMagnitude = (segmentsToCheck[i].Approximation[j] - p).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = j;
					curvySplineSegment = segmentsToCheck[i];
				}
			}
		}
		CurvySplineSegment[] array = new CurvySplineSegment[3];
		int[] array2 = new int[3];
		array[1] = curvySplineSegment;
		array2[1] = num2;
		if (!getPreviousApproximationPoint(curvySplineSegment, num2, out array[0], out array2[0], ref segmentsToCheck))
		{
			array[0] = array[1];
			array2[0] = array2[1];
		}
		if (!getNextApproximationPoint(curvySplineSegment, num2, out array[2], out array2[2], ref segmentsToCheck))
		{
			array[2] = array[1];
			array2[2] = array2[1];
		}
		float[] array3 = new float[2];
		float[] array4 = new float[2]
		{
			LinePointDistanceSqr(array[0].Approximation[array2[0]], array[1].Approximation[array2[1]], p, out array3[0]),
			LinePointDistanceSqr(array[1].Approximation[array2[1]], array[2].Approximation[array2[2]], p, out array3[1])
		};
		if (array4[0] < array4[1])
		{
			return array[0].LocalFToTF(array[0]._getApproximationLocalF(array2[0]) + array3[0] * mStepSize);
		}
		return array[1].LocalFToTF(array[1]._getApproximationLocalF(array2[1]) + array3[1] * mStepSize);
	}

	public CurvySplineSegment NextControlPoint(CurvySplineSegment controlpoint)
	{
		if (mControlPoints.Count == 0)
		{
			return null;
		}
		int num = controlpoint.ControlPointIndex + 1;
		if (num >= mControlPoints.Count)
		{
			if (Closed)
			{
				return mControlPoints[0];
			}
			return null;
		}
		return mControlPoints[num];
	}

	public CurvySplineSegment PreviousControlPoint(CurvySplineSegment controlpoint)
	{
		if (mControlPoints.Count == 0)
		{
			return null;
		}
		int num = controlpoint.ControlPointIndex - 1;
		if (num < 0)
		{
			if (Closed)
			{
				return mControlPoints[mControlPoints.Count - 1];
			}
			return null;
		}
		return mControlPoints[num];
	}

	public CurvySplineSegment NextSegment(CurvySplineSegment segment)
	{
		if (mSegments.Count == 0 || !segment.IsValidSegment)
		{
			return null;
		}
		int num = segment.SegmentIndex + 1;
		if (num >= mSegments.Count)
		{
			return (!Closed) ? null : mSegments[0];
		}
		return mSegments[num];
	}

	public CurvySplineSegment PreviousSegment(CurvySplineSegment segment)
	{
		if (mSegments.Count == 0)
		{
			return null;
		}
		if (segment.SegmentIndex == -1)
		{
			int num = segment.ControlPointIndex - 1;
			if (num >= 0 && mControlPoints[num].SegmentIndex > -1)
			{
				return mControlPoints[num];
			}
			return null;
		}
		int num2 = segment.SegmentIndex - 1;
		if (num2 < 0)
		{
			return (!Closed) ? null : mSegments[mSegments.Count - 1];
		}
		return (num2 >= Count) ? null : mSegments[num2];
	}

	public Transform NextTransform(CurvySplineSegment controlpoint)
	{
		CurvySplineSegment curvySplineSegment = NextControlPoint(controlpoint);
		if ((bool)curvySplineSegment)
		{
			return curvySplineSegment.Transform;
		}
		if (AutoEndTangents && Interpolation != CurvyInterpolation.Linear && Interpolation != CurvyInterpolation.Bezier)
		{
			return controlpoint.Transform;
		}
		return null;
	}

	public Transform PreviousTransform(CurvySplineSegment controlpoint)
	{
		CurvySplineSegment curvySplineSegment = PreviousControlPoint(controlpoint);
		if ((bool)curvySplineSegment)
		{
			return curvySplineSegment.Transform;
		}
		if (AutoEndTangents && Interpolation != CurvyInterpolation.Linear && Interpolation != CurvyInterpolation.Bezier)
		{
			return controlpoint.Transform;
		}
		return null;
	}

	public override void Refresh()
	{
		Refresh(AutoRefreshLength, AutoRefreshOrientation, false);
	}

	public override void RefreshImmediately()
	{
		RefreshImmediately(AutoRefreshLength, AutoRefreshOrientation, false);
	}

	public override void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
	{
		if (skipIfInitialized && base.IsInitialized)
		{
			base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized);
			return;
		}
		mStepSize = 1f / (float)Granularity;
		UserValueSize = Mathf.Max(0, UserValueSize);
		mSegments.Clear();
		if (Application.isEditor && !Application.isPlaying)
		{
			_RenameControlPointsByIndex();
		}
		_RefreshHierarchy();
		for (int i = 0; i < mControlPoints.Count; i++)
		{
			if (mControlPoints[i].IsValidSegment)
			{
				mSegments.Add(mControlPoints[i]);
				mControlPoints[i]._UpdateApproximation();
			}
		}
		if (refreshOrientation)
		{
			doRefreshTangents();
			doRefreshOrientation();
			if (!Application.isPlaying && ControlPointCount > 0 && ControlPoints[ControlPointCount - 1].ConnectedBy.Count > 0)
			{
				ControlPoints[ControlPointCount - 1].ConnectedBy[0].Spline.Refresh(false, true, false);
			}
		}
		if (refreshLength)
		{
			doRefreshLength();
		}
		mIsInitialized = true;
		OnRefreshEvent(this);
		base.RefreshImmediately(refreshLength, refreshOrientation, skipIfInitialized);
	}

	public override float SegmentToTF(CurvySplineSegment segment)
	{
		return SegmentToTF(segment, 0f);
	}

	public override float SegmentToTF(CurvySplineSegment segment, float localF)
	{
		if (!segment)
		{
			return 0f;
		}
		if (segment.SegmentIndex == -1)
		{
			return (segment.ControlPointIndex > 0) ? 1 : 0;
		}
		return (float)segment.SegmentIndex / (float)Count + 1f / (float)Count * localF;
	}

	public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
	{
		double num = 3.0;
		double num2 = -3.0;
		double num3 = 3.0;
		double num4 = -6.0;
		double num5 = 3.0;
		double num6 = -3.0;
		double num7 = 3.0;
		double num8 = (double)(0f - P0.x) + num * (double)T0.x + num2 * (double)T1.x + (double)P1.x;
		double num9 = num3 * (double)P0.x + num4 * (double)T0.x + num5 * (double)T1.x;
		double num10 = num6 * (double)P0.x + num7 * (double)T0.x;
		double num11 = P0.x;
		double num12 = (double)(0f - P0.y) + num * (double)T0.y + num2 * (double)T1.y + (double)P1.y;
		double num13 = num3 * (double)P0.y + num4 * (double)T0.y + num5 * (double)T1.y;
		double num14 = num6 * (double)P0.y + num7 * (double)T0.y;
		double num15 = P0.y;
		double num16 = (double)(0f - P0.z) + num * (double)T0.z + num2 * (double)T1.z + (double)P1.z;
		double num17 = num3 * (double)P0.z + num4 * (double)T0.z + num5 * (double)T1.z;
		double num18 = num6 * (double)P0.z + num7 * (double)T0.z;
		double num19 = P0.z;
		float x = (float)(((num8 * (double)f + num9) * (double)f + num10) * (double)f + num11);
		float y = (float)(((num12 * (double)f + num13) * (double)f + num14) * (double)f + num15);
		float z = (float)(((num16 * (double)f + num17) * (double)f + num18) * (double)f + num19);
		return new Vector3(x, y, z);
	}

	public static Vector3 CatmulRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
	{
		double num = -0.5;
		double num2 = 1.5;
		double num3 = -1.5;
		double num4 = 0.5;
		double num5 = -2.5;
		double num6 = 2.0;
		double num7 = -0.5;
		double num8 = -0.5;
		double num9 = 0.5;
		double num10 = num * (double)T0.x + num2 * (double)P0.x + num3 * (double)P1.x + num4 * (double)T1.x;
		double num11 = (double)T0.x + num5 * (double)P0.x + num6 * (double)P1.x + num7 * (double)T1.x;
		double num12 = num8 * (double)T0.x + num9 * (double)P1.x;
		double num13 = P0.x;
		double num14 = num * (double)T0.y + num2 * (double)P0.y + num3 * (double)P1.y + num4 * (double)T1.y;
		double num15 = (double)T0.y + num5 * (double)P0.y + num6 * (double)P1.y + num7 * (double)T1.y;
		double num16 = num8 * (double)T0.y + num9 * (double)P1.y;
		double num17 = P0.y;
		double num18 = num * (double)T0.z + num2 * (double)P0.z + num3 * (double)P1.z + num4 * (double)T1.z;
		double num19 = (double)T0.z + num5 * (double)P0.z + num6 * (double)P1.z + num7 * (double)T1.z;
		double num20 = num8 * (double)T0.z + num9 * (double)P1.z;
		double num21 = P0.z;
		float x = (float)(((num10 * (double)f + num11) * (double)f + num12) * (double)f + num13);
		float y = (float)(((num14 * (double)f + num15) * (double)f + num16) * (double)f + num17);
		float z = (float)(((num18 * (double)f + num19) * (double)f + num20) * (double)f + num21);
		return new Vector3(x, y, z);
	}

	public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
	{
		double num = (1f - FT0) * (1f + FC0) * (1f + FB0);
		double num2 = (1f - FT0) * (1f - FC0) * (1f - FB0);
		double num3 = (1f - FT1) * (1f - FC1) * (1f + FB1);
		double num4 = (1f - FT1) * (1f + FC1) * (1f - FB1);
		double num5 = 2.0;
		double num6 = (0.0 - num) / num5;
		double num7 = (4.0 + num - num2 - num3) / num5;
		double num8 = (-4.0 + num2 + num3 - num4) / num5;
		double num9 = num4 / num5;
		double num10 = 2.0 * num / num5;
		double num11 = (-6.0 - 2.0 * num + 2.0 * num2 + num3) / num5;
		double num12 = (6.0 - 2.0 * num2 - num3 + num4) / num5;
		double num13 = (0.0 - num4) / num5;
		double num14 = (0.0 - num) / num5;
		double num15 = (num - num2) / num5;
		double num16 = num2 / num5;
		double num17 = 2.0 / num5;
		double num18 = num6 * (double)T0.x + num7 * (double)P0.x + num8 * (double)P1.x + num9 * (double)T1.x;
		double num19 = num10 * (double)T0.x + num11 * (double)P0.x + num12 * (double)P1.x + num13 * (double)T1.x;
		double num20 = num14 * (double)T0.x + num15 * (double)P0.x + num16 * (double)P1.x;
		double num21 = num17 * (double)P0.x;
		double num22 = num6 * (double)T0.y + num7 * (double)P0.y + num8 * (double)P1.y + num9 * (double)T1.y;
		double num23 = num10 * (double)T0.y + num11 * (double)P0.y + num12 * (double)P1.y + num13 * (double)T1.y;
		double num24 = num14 * (double)T0.y + num15 * (double)P0.y + num16 * (double)P1.y;
		double num25 = num17 * (double)P0.y;
		double num26 = num6 * (double)T0.z + num7 * (double)P0.z + num8 * (double)P1.z + num9 * (double)T1.z;
		double num27 = num10 * (double)T0.z + num11 * (double)P0.z + num12 * (double)P1.z + num13 * (double)T1.z;
		double num28 = num14 * (double)T0.z + num15 * (double)P0.z + num16 * (double)P1.z;
		double num29 = num17 * (double)P0.z;
		float x = (float)(((num18 * (double)f + num19) * (double)f + num20) * (double)f + num21);
		float y = (float)(((num22 * (double)f + num23) * (double)f + num24) * (double)f + num25);
		float z = (float)(((num26 * (double)f + num27) * (double)f + num28) * (double)f + num29);
		return new Vector3(x, y, z);
	}

	private void doRefreshLength()
	{
		mLength = 0f;
		if (Count > 0)
		{
			int controlPointIndex = this[0].ControlPointIndex;
			int controlPointIndex2 = this[Count - 1].ControlPointIndex;
			for (int i = controlPointIndex; i <= controlPointIndex2; i++)
			{
				mLength += ControlPoints[i]._UpdateLength();
			}
			if (++controlPointIndex2 < ControlPointCount)
			{
				ControlPoints[controlPointIndex2]._UpdateLength();
			}
		}
	}

	private void doRefreshTangents()
	{
		for (int i = 0; i < Count; i++)
		{
			this[i]._UpdateTangents();
		}
	}

	private void doRefreshOrientation()
	{
		if (Count <= 0)
		{
			return;
		}
		CurvyInitialUpDefinition initialUpVector = InitialUpVector;
		Vector3 tangent = ((initialUpVector != CurvyInitialUpDefinition.MinAxis) ? this[0].Transform.up : minAxis(this[0].GetTangent(0f)));
		Vector3.OrthoNormalize(ref this[0].ApproximationT[0], ref tangent);
		for (int i = 0; i < Count; i++)
		{
			tangent = this[i]._UpdateOrientation(tangent);
			if (SetControlPointRotation && Orientation == CurvyOrientation.Tangent)
			{
				this[i].Transform.up = this[i].ApproximationUp[0];
			}
		}
		if (!Closed && SetControlPointRotation && Orientation == CurvyOrientation.Tangent && (bool)this[Count - 1].NextControlPoint)
		{
			this[Count - 1].NextControlPoint.Transform.up = this[Count - 1].ApproximationUp[Granularity];
		}
		if (Closed && Orientation == CurvyOrientation.Tangent)
		{
			float num = AngleSigned(this[Count - 1].ApproximationUp[Granularity - 1], this[0].ApproximationUp[0], this[0].ApproximationT[0]) / (float)(Count * Granularity);
			float angleaccu = num;
			tangent = this[0].ApproximationUp[0];
			for (int j = 0; j < Count; j++)
			{
				tangent = this[j]._SmoothOrientation(tangent, ref angleaccu, num);
			}
			this[Count - 1].ApproximationUp[Granularity] = this[0].ApproximationUp[0];
		}
	}

	private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
	}

	public void _RefreshHierarchy()
	{
		int childCount = base.Transform.childCount;
		if (ControlPointCount != childCount)
		{
			mControlPoints = new List<CurvySplineSegment>(base.transform.childCount);
			for (int i = 0; i < childCount; i++)
			{
				CurvySplineSegment component = base.transform.GetChild(i).GetComponent<CurvySplineSegment>();
				if ((bool)component)
				{
					component._ReSettle();
					mControlPoints.Add(component);
				}
			}
		}
		for (int num = mControlPoints.Count - 1; num > -1; num--)
		{
			if (mControlPoints[num] == null)
			{
				mControlPoints.RemoveAt(num);
			}
		}
		mControlPoints.Sort((CurvySplineSegment a, CurvySplineSegment b) => string.Compare(a.name, b.name));
		for (int num2 = 0; num2 < mControlPoints.Count; num2++)
		{
			mControlPoints[num2]._InitializeControlPoint();
		}
	}

	public void _RenameControlPointsByIndex()
	{
		for (int i = 0; i < mControlPoints.Count; i++)
		{
			if (mControlPoints[i] != null)
			{
				string text = "CP" + string.Format("{0:000}", i);
				if (mControlPoints[i].name != text)
				{
					mControlPoints[i].name = text;
				}
				mControlPoints[i]._InitializeControlPoint();
			}
		}
	}

	private Vector3 minAxis(Vector3 v)
	{
		return (v.x < v.y) ? ((!(v.x < v.z)) ? new Vector3(0f, 0f, 1f) : new Vector3(1f, 0f, 0f)) : ((!(v.y < v.z)) ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 1f, 0f));
	}

	private Vector3 GetUpVector(Vector3 P, Vector3 P1, Vector3 LastUp)
	{
		Vector3 normalized = (P - P1).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, LastUp).normalized;
		return Vector3.Cross(normalized2, normalized).normalized;
	}

	private bool getPreviousApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
	{
		residx = idx - 1;
		res = seg;
		if (residx < 0)
		{
			res = PreviousSegment(seg);
			if ((bool)res)
			{
				residx = res.Approximation.Length - 2;
				for (int i = 0; i < validSegments.Length; i++)
				{
					if (validSegments[i] == res)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}
		return true;
	}

	private bool getNextApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
	{
		residx = idx + 1;
		res = seg;
		if (residx == seg.Approximation.Length)
		{
			res = NextSegment(seg);
			if ((bool)res)
			{
				residx = 1;
				for (int i = 0; i < validSegments.Length; i++)
				{
					if (validSegments[i] == res)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool iterateApproximationPoints(ref CurvySplineSegment seg, ref int idx)
	{
		idx++;
		if (idx == seg.Approximation.Length)
		{
			seg = NextSegment(seg);
			idx = 1;
			return seg != null && !seg.IsFirstSegment;
		}
		return true;
	}

	private float LinePointDistanceSqr(Vector3 l1, Vector3 l2, Vector3 p, out float frag)
	{
		Vector3 vector = l2 - l1;
		Vector3 lhs = p - l1;
		float num = Vector3.Dot(lhs, vector);
		if (num <= 0f)
		{
			frag = 0f;
			return (p - l1).sqrMagnitude;
		}
		float num2 = Vector3.Dot(vector, vector);
		if (num2 <= num)
		{
			frag = 1f;
			return (p - l2).sqrMagnitude;
		}
		frag = num / num2;
		Vector3 vector2 = l1 + frag * vector;
		return (p - vector2).sqrMagnitude;
	}
}
