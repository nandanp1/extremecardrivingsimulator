using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

[ExecuteInEditMode]
public class CurvySplineSegment : MonoBehaviour
{
	public bool SynchronizeTCB = true;

	public bool SmoothEdgeTangent;

	public bool OverrideGlobalTension;

	public bool OverrideGlobalContinuity;

	public bool OverrideGlobalBias;

	public float StartTension;

	public float StartContinuity;

	public float StartBias;

	public float EndTension;

	public float EndContinuity;

	public float EndBias;

	public Vector3[] UserValues = new Vector3[0];

	[HideInInspector]
	public Vector3[] Approximation = new Vector3[0];

	[HideInInspector]
	public float[] ApproximationDistances = new float[0];

	[HideInInspector]
	public Vector3[] ApproximationUp = new Vector3[0];

	[HideInInspector]
	public Vector3[] ApproximationT = new Vector3[0];

	public Vector3 HandleIn = new Vector3(-1f, 0f, 0f);

	public Vector3 HandleOut = new Vector3(1f, 0f, 0f);

	public float HandleScale = 1f;

	public bool FreeHandles;

	[SerializeField]
	private CurvyConnection mConnection;

	public List<CurvySplineSegment> ConnectedBy = new List<CurvySplineSegment>();

	private Transform mTransform;

	private Vector3 mPosition;

	private Quaternion mRotation;

	private CurvySpline mSpline;

	private float mStepSize;

	private int mControlPointIndex = -1;

	private int mSegmentIndex = -1;

	public Vector3 HandleInPosition
	{
		get
		{
			return Position + Transform.TransformDirection(HandleIn);
		}
		set
		{
			HandleIn = Transform.InverseTransformDirection(value - Position);
			if (!FreeHandles)
			{
				HandleOut = HandleIn * -1f;
			}
		}
	}

	public Vector3 HandleOutPosition
	{
		get
		{
			return Position + Transform.TransformDirection(HandleOut);
		}
		set
		{
			HandleOut = Transform.InverseTransformDirection(value - Position);
			if (!FreeHandles)
			{
				HandleIn = HandleOut * -1f;
			}
		}
	}

	public Transform Transform
	{
		get
		{
			if (!mTransform)
			{
				mTransform = base.transform;
			}
			return mTransform;
		}
	}

	public float Length { get; private set; }

	public float Distance { get; private set; }

	public Vector3 Position
	{
		get
		{
			return Transform.position;
		}
		set
		{
			Transform.position = value;
		}
	}

	public bool IsValidSegment
	{
		get
		{
			switch (Spline.Interpolation)
			{
			case CurvyInterpolation.Bezier:
				return NextControlPoint;
			case CurvyInterpolation.Linear:
				return (bool)Transform && (bool)NextTransform;
			case CurvyInterpolation.CatmulRom:
			case CurvyInterpolation.TCB:
			{
				CurvySplineSegment nextControlPoint = NextControlPoint;
				return (bool)Transform && (bool)PreviousTransform && (bool)nextControlPoint && nextControlPoint.Spline == Spline && (bool)NextControlPoint.NextTransform;
			}
			default:
				return false;
			}
		}
	}

	public bool IsFirstSegment
	{
		get
		{
			return !PreviousSegment || (Spline.Closed && PreviousSegment == Spline[Spline.Count - 1]);
		}
	}

	public bool IsLastSegment
	{
		get
		{
			return !NextSegment || (Spline.Closed && NextSegment == Spline[0]);
		}
	}

	public bool IsFirstControlPoint
	{
		get
		{
			return ControlPointIndex == 0;
		}
	}

	public bool IsFirstVisibleControlPoint
	{
		get
		{
			return SegmentIndex == 0;
		}
	}

	public bool IsLastVisibleControlPoint
	{
		get
		{
			return this == Spline.LastVisibleControlPoint;
		}
	}

	public bool IsLastControlPoint
	{
		get
		{
			return ControlPointIndex == Spline.ControlPointCount - 1;
		}
	}

	public CurvySplineSegment NextControlPoint
	{
		get
		{
			return Spline.NextControlPoint(this);
		}
	}

	public CurvySplineSegment PreviousControlPoint
	{
		get
		{
			return Spline.PreviousControlPoint(this);
		}
	}

	public Transform NextTransform
	{
		get
		{
			return Spline.NextTransform(this);
		}
	}

	public Transform PreviousTransform
	{
		get
		{
			return Spline.PreviousTransform(this);
		}
	}

	public CurvySplineSegment NextSegment
	{
		get
		{
			return Spline.NextSegment(this);
		}
	}

	public CurvySplineSegment PreviousSegment
	{
		get
		{
			return Spline.PreviousSegment(this);
		}
	}

	public int SegmentIndex
	{
		get
		{
			return mSegmentIndex;
		}
	}

	public int ControlPointIndex
	{
		get
		{
			return mControlPointIndex;
		}
	}

	public CurvySpline Spline
	{
		get
		{
			if (!mSpline)
			{
				mSpline = base.transform.parent.GetComponent<CurvySpline>();
			}
			return mSpline;
		}
	}

	public CurvyConnection Connection
	{
		get
		{
			if (mConnection == null)
			{
				mConnection = new CurvyConnection();
			}
			else
			{
				mConnection.Validate();
			}
			return (!mConnection.Active) ? null : mConnection;
		}
	}

	public CurvyConnection ConnectionAny
	{
		get
		{
			if (Connection != null)
			{
				return Connection;
			}
			if (ConnectedBy.Count > 0)
			{
				for (int num = ConnectedBy.Count - 1; num > -1; num--)
				{
					if (!ConnectedBy[num] || ConnectedBy[num].Connection == null || !ConnectedBy[num].Connection.Active || ConnectedBy[num].Connection.Other != this)
					{
						ConnectedBy.RemoveAt(num);
					}
				}
				if (ConnectedBy.Count > 0)
				{
					return ConnectedBy[0].Connection;
				}
			}
			return null;
		}
	}

	private Vector3 HandleInScaled
	{
		get
		{
			return Position + Transform.TransformDirection(HandleIn * HandleScale);
		}
	}

	private Vector3 HandleOutScaled
	{
		get
		{
			return Position + Transform.TransformDirection(HandleOut * HandleScale);
		}
	}

	private void OnDrawGizmos()
	{
		DoGizmos(false);
	}

	private void OnDrawGizmosSelected()
	{
		DoGizmos(true);
	}

	private void DoGizmos(bool selected)
	{
		if (CurvySpline.Gizmos == CurvySplineGizmos.None)
		{
			return;
		}
		bool flag = CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Curve);
		bool flag2 = CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Network);
		CurvyConnection connectionAny = ConnectionAny;
		if (connectionAny != null && (flag || flag2))
		{
			switch (connectionAny.Sync)
			{
			case CurvyConnection.SyncMode.NoSync:
				Gizmos.color = new Color(0f, 0f, 0f, 0.6f);
				break;
			case CurvyConnection.SyncMode.SyncPosAndRot:
				Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
				break;
			case CurvyConnection.SyncMode.SyncRot:
				Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
				break;
			case CurvyConnection.SyncMode.SyncPos:
				Gizmos.color = CurvySpline.GizmoColor;
				break;
			}
			Gizmos.DrawWireSphere(Transform.position, CurvyUtility.GetHandleSize(Transform.position) * CurvySpline.GizmoControlPointSize * 1.4f);
		}
		if (!flag2 && flag)
		{
			Gizmos.color = ((!selected) ? CurvySpline.GizmoColor : CurvySpline.GizmoSelectionColor);
			Gizmos.DrawSphere(Transform.position, CurvyUtility.GetHandleSize(Transform.position) * 0.7f * CurvySpline.GizmoControlPointSize);
		}
		if (!IsValidSegment)
		{
			return;
		}
		if (flag || flag2)
		{
			Gizmos.color = ((!selected) ? CurvySpline.GizmoColor : CurvySpline.GizmoSelectionColor);
			Vector3 vector = Transform.position;
			for (float num = 0.05f; num < 1f; num += 0.05f)
			{
				Vector3 vector2 = Interpolate(num);
				Gizmos.DrawLine(vector, vector2);
				vector = vector2;
			}
			Gizmos.DrawLine(vector, NextTransform.position);
		}
		if (Approximation.Length > 0 && CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Approximation))
		{
			Gizmos.color = Color.gray;
			Vector3[] approximation = Approximation;
			foreach (Vector3 center in approximation)
			{
				Gizmos.DrawCube(center, new Vector3(0.1f, 0.1f, 0.1f));
			}
		}
		if (Spline.Orientation != CurvyOrientation.None && ApproximationUp.Length > 0 && CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Orientation))
		{
			Gizmos.color = Color.yellow;
			for (int j = 0; j < ApproximationUp.Length; j++)
			{
				Gizmos.DrawLine(Approximation[j], Approximation[j] + ApproximationUp[j] * CurvySpline.GizmoOrientationLength);
			}
		}
		if (ApproximationT.Length > 0 && CurvySpline.Gizmos.HasFlag(CurvySplineGizmos.Tangents))
		{
			Gizmos.color = new Color(0f, 0.7f, 0f);
			for (int k = 0; k < ApproximationT.Length; k++)
			{
				Gizmos.DrawLine(Approximation[k], Approximation[k] + ApproximationT[k]);
			}
		}
	}

	private void OnEnable()
	{
		mPosition = Transform.position;
		mRotation = Transform.rotation;
	}

	private void OnDestroy()
	{
		ClearConnections();
	}

	public void ClearConnections()
	{
		ConnectTo(null);
		for (int i = 0; i < ConnectedBy.Count; i++)
		{
			if ((bool)ConnectedBy[i])
			{
				ConnectedBy[i].ConnectTo(null);
			}
		}
		ConnectedBy.Clear();
	}

	public void ConnectTo(CurvySplineSegment targetCP)
	{
		ConnectTo(targetCP, CurvyConnection.HeadingMode.Auto, CurvyConnection.SyncMode.NoSync);
	}

	public void ConnectTo(CurvySplineSegment targetCP, CurvyConnection.HeadingMode heading, CurvyConnection.SyncMode sync, params string[] tags)
	{
		CurvyConnection connection = Connection;
		if (connection != null)
		{
			Connection.Disconnect();
		}
		if ((bool)targetCP)
		{
			mConnection.Set(this, targetCP, heading, sync, tags);
		}
	}

	public void Delete()
	{
		Spline.Delete(this);
	}

	public List<CurvySplineSegment> GetAllConnectionsControlPoints()
	{
		List<CurvySplineSegment> list = new List<CurvySplineSegment>();
		List<CurvyConnection> allConnections = GetAllConnections();
		if (allConnections.Count > 0)
		{
			list.Add(this);
		}
		for (int i = 0; i < allConnections.Count; i++)
		{
			if (!list.Contains(allConnections[i].Owner))
			{
				list.Add(allConnections[i].Owner);
			}
			if (!list.Contains(allConnections[i].Other))
			{
				list.Add(allConnections[i].Other);
			}
		}
		return list;
	}

	public List<CurvyConnection> GetAllConnections()
	{
		List<CurvyConnection> list = new List<CurvyConnection>();
		if (Connection != null)
		{
			list.Add(Connection);
			List<CurvySplineSegment> connectedBy = Connection.Other.ConnectedBy;
			for (int i = 0; i < connectedBy.Count; i++)
			{
				if (!list.Contains(connectedBy[i].Connection))
				{
					list.Add(connectedBy[i].Connection);
				}
			}
		}
		for (int j = 0; j < ConnectedBy.Count; j++)
		{
			list.Add(ConnectedBy[j].Connection);
		}
		return list;
	}

	public List<CurvyConnection> GetAllConnections(int minMatchesNeeded, params string[] tags)
	{
		List<CurvyConnection> list = new List<CurvyConnection>();
		CurvyConnection connection = Connection;
		if (connection != null && connection.MatchingTags(tags).Count >= minMatchesNeeded)
		{
			list.Add(connection);
		}
		for (int num = ConnectedBy.Count - 1; num > -1; num--)
		{
			if (ConnectedBy[num].Connection != null)
			{
				if (ConnectedBy[num].Connection.Other != this)
				{
					ConnectedBy.RemoveAt(num);
				}
				else if (ConnectedBy[num].Connection.MatchingTags(tags).Count >= minMatchesNeeded)
				{
					list.Add(ConnectedBy[num].Connection);
				}
			}
			else
			{
				ConnectedBy.RemoveAt(num);
			}
		}
		return list;
	}

	public Vector3 Interpolate(float localF)
	{
		return Interpolate(localF, Spline.Interpolation);
	}

	public Vector3 Interpolate(float localF, CurvyInterpolation interpolation)
	{
		localF = Mathf.Clamp01(localF);
		switch (interpolation)
		{
		case CurvyInterpolation.Bezier:
			return CurvySpline.Bezier(HandleOutScaled, Transform.position, NextTransform.position, NextControlPoint.HandleInScaled, localF);
		case CurvyInterpolation.CatmulRom:
			return CurvySpline.CatmulRom(GetPrevPosition(), Transform.position, GetNextPosition(), GetNextNextPosition(), localF);
		case CurvyInterpolation.TCB:
		{
			float fT = StartTension;
			float fT2 = EndTension;
			float fC = StartContinuity;
			float fC2 = EndContinuity;
			float fB = StartBias;
			float fB2 = EndBias;
			if (!OverrideGlobalTension)
			{
				fT = (fT2 = Spline.Tension);
			}
			if (!OverrideGlobalContinuity)
			{
				fC = (fC2 = Spline.Continuity);
			}
			if (!OverrideGlobalBias)
			{
				fB = (fB2 = Spline.Bias);
			}
			return CurvySpline.TCB(PreviousTransform.position, Transform.position, NextTransform.position, NextControlPoint.NextTransform.position, localF, fT, fC, fB, fT2, fC2, fB2);
		}
		default:
			return Vector3.Lerp(Transform.position, NextTransform.position, localF);
		}
	}

	private Vector3 GetPrevPosition()
	{
		CurvySplineSegment previousControlPoint = PreviousControlPoint;
		if ((bool)previousControlPoint)
		{
			return previousControlPoint.Position;
		}
		CurvyConnection connectionAny = ConnectionAny;
		CurvySplineSegment curvySplineSegment = ((connectionAny == null) ? null : connectionAny.GetCounterpart(this));
		if (curvySplineSegment != null)
		{
			if (!curvySplineSegment.Spline.IsInitialized)
			{
				curvySplineSegment.Spline._RefreshHierarchy();
			}
			CurvyConnection.HeadingMode headingMode = connectionAny.Heading;
			if (headingMode == CurvyConnection.HeadingMode.Auto)
			{
				headingMode = ((!curvySplineSegment.PreviousControlPoint) ? CurvyConnection.HeadingMode.Plus : ((!curvySplineSegment.NextControlPoint) ? CurvyConnection.HeadingMode.Minus : CurvyConnection.HeadingMode.Sharp));
			}
			switch (headingMode)
			{
			case CurvyConnection.HeadingMode.Minus:
			{
				Transform previousTransform = curvySplineSegment.PreviousTransform;
				if ((bool)previousTransform)
				{
					return previousTransform.position;
				}
				break;
			}
			case CurvyConnection.HeadingMode.Plus:
			{
				Transform nextTransform = curvySplineSegment.NextTransform;
				if ((bool)nextTransform)
				{
					return nextTransform.position;
				}
				break;
			}
			}
		}
		if ((bool)PreviousTransform)
		{
			return PreviousTransform.position;
		}
		return Position;
	}

	public Vector3 GetNextPosition()
	{
		CurvySplineSegment nextControlPoint = NextControlPoint;
		if ((bool)nextControlPoint)
		{
			return nextControlPoint.Position;
		}
		CurvyConnection connectionAny = ConnectionAny;
		CurvySplineSegment curvySplineSegment = ((connectionAny == null) ? null : connectionAny.GetCounterpart(this));
		if (curvySplineSegment != null)
		{
			if (!curvySplineSegment.Spline.IsInitialized)
			{
				curvySplineSegment.Spline._RefreshHierarchy();
			}
			CurvyConnection.HeadingMode headingMode = connectionAny.Heading;
			if (headingMode == CurvyConnection.HeadingMode.Auto)
			{
				headingMode = ((!curvySplineSegment.NextControlPoint) ? CurvyConnection.HeadingMode.Minus : ((!curvySplineSegment.PreviousControlPoint) ? CurvyConnection.HeadingMode.Plus : CurvyConnection.HeadingMode.Sharp));
			}
			switch (headingMode)
			{
			case CurvyConnection.HeadingMode.Plus:
			{
				Transform nextTransform = curvySplineSegment.NextTransform;
				if ((bool)nextTransform)
				{
					return nextTransform.position;
				}
				break;
			}
			case CurvyConnection.HeadingMode.Minus:
			{
				Transform previousTransform = curvySplineSegment.PreviousTransform;
				if ((bool)previousTransform)
				{
					return previousTransform.position;
				}
				break;
			}
			}
		}
		if ((bool)NextTransform)
		{
			return NextTransform.position;
		}
		return Position;
	}

	private Vector3 GetNextNextPosition()
	{
		CurvySplineSegment nextControlPoint = NextControlPoint;
		if ((bool)nextControlPoint)
		{
			return nextControlPoint.GetNextPosition();
		}
		if ((bool)NextTransform)
		{
			return NextTransform.position;
		}
		return Position;
	}

	public Vector3 InterpolateFast(float localF)
	{
		float frag;
		int approximationIndex = getApproximationIndex(localF, out frag);
		return Vector3.Lerp(Approximation[approximationIndex], Approximation[approximationIndex + 1], frag);
	}

	public Vector3 InterpolateUserValue(float localF, int index)
	{
		if (index >= Spline.UserValueSize || NextControlPoint == null)
		{
			return Vector3.zero;
		}
		return Vector3.Lerp(UserValues[index], NextControlPoint.UserValues[index], localF);
	}

	public Vector3 InterpolateScale(float localF)
	{
		Transform nextTransform = NextTransform;
		return (!nextTransform) ? Transform.lossyScale : Vector3.Lerp(Transform.lossyScale, nextTransform.lossyScale, localF);
	}

	public Vector3 GetTangent(float localF)
	{
		localF = Mathf.Clamp01(localF);
		Vector3 position = Interpolate(localF);
		return GetTangent(localF, ref position);
	}

	public Vector3 GetTangent(float localF, ref Vector3 position)
	{
		float num = localF + 0.01f;
		if (num > 1f)
		{
			if ((bool)NextSegment)
			{
				return (NextSegment.Interpolate(num - 1f) - position).normalized;
			}
			num = localF - 0.01f;
			return (position - Interpolate(num)).normalized;
		}
		return (Interpolate(num) - position).normalized;
	}

	public Vector3 GetTangentFast(float localF)
	{
		float frag;
		int approximationIndex = getApproximationIndex(localF, out frag);
		return Vector3.Lerp(ApproximationT[approximationIndex], ApproximationT[approximationIndex + 1], frag);
	}

	public Quaternion GetOrientationFast(float localF)
	{
		return GetOrientationFast(localF, false);
	}

	public Quaternion GetOrientationFast(float localF, bool inverse)
	{
		Vector3 tangentFast = GetTangentFast(localF);
		if (tangentFast != Vector3.zero)
		{
			if (inverse)
			{
				tangentFast *= -1f;
			}
			return Quaternion.LookRotation(tangentFast, GetOrientationUpFast(localF));
		}
		return Quaternion.identity;
	}

	public Vector3 GetOrientationUpFast(float localF)
	{
		float frag;
		int approximationIndex = getApproximationIndex(localF, out frag);
		return Vector3.Lerp(ApproximationUp[approximationIndex], ApproximationUp[approximationIndex + 1], frag);
	}

	public float DistanceToLocalF(float localDistance)
	{
		localDistance = Mathf.Clamp(localDistance, 0f, Length);
		if (ApproximationDistances.Length == 0 || localDistance == 0f)
		{
			return 0f;
		}
		if (Mathf.Approximately(localDistance, Length))
		{
			return 1f;
		}
		int num = ApproximationDistances.Length - 1;
		while (ApproximationDistances[num] > localDistance)
		{
			num--;
		}
		float num2 = (localDistance - ApproximationDistances[num]) / (ApproximationDistances[num + 1] - ApproximationDistances[num]);
		float num3 = _getApproximationLocalF(num);
		float num4 = _getApproximationLocalF(num + 1);
		return num3 + (num4 - num3) * num2;
	}

	public float LocalFToDistance(float localF)
	{
		localF = Mathf.Clamp01(localF);
		float frag;
		int approximationIndex = getApproximationIndex(localF, out frag);
		if (ApproximationDistances.Length > 0)
		{
			float num = ApproximationDistances[approximationIndex + 1] - ApproximationDistances[approximationIndex];
			return ApproximationDistances[approximationIndex] + num * frag;
		}
		return 0f;
	}

	public float LocalFToTF(float localF)
	{
		return Spline.SegmentToTF(this, localF);
	}

	public bool SnapToFitSplineLength(float newSplineLength, float stepSize)
	{
		if (stepSize == 0f || Mathf.Approximately(newSplineLength, Spline.Length))
		{
			return true;
		}
		float length = Spline.Length;
		Vector3 position = Transform.position;
		Vector3 vector = Transform.up * stepSize;
		Transform.position += vector;
		Spline.Refresh(true, false, false);
		bool flag = Spline.Length > length;
		int num = 30000;
		Transform.position = position;
		if (newSplineLength > length)
		{
			if (!flag)
			{
				vector *= -1f;
			}
			while (Spline.Length < newSplineLength)
			{
				num--;
				length = Spline.Length;
				Transform.position += vector;
				Spline.Refresh(true, false, false);
				if (length > Spline.Length)
				{
					return false;
				}
				if (num == 0)
				{
					Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please report this as an error!");
					return false;
				}
			}
		}
		else
		{
			if (flag)
			{
				vector *= -1f;
			}
			while (Spline.Length > newSplineLength)
			{
				num--;
				length = Spline.Length;
				Transform.position += vector;
				Spline.Refresh(true, false, false);
				if (length < Spline.Length)
				{
					return false;
				}
				if (num == 0)
				{
					Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please report this as an error!");
					return false;
				}
			}
		}
		return true;
	}

	public override string ToString()
	{
		if ((bool)Spline)
		{
			return Spline.name + "." + base.name;
		}
		return base.ToString();
	}

	public float _getApproximationLocalF(int idx)
	{
		return (float)idx * mStepSize;
	}

	private int getApproximationIndex(float localF, out float frag)
	{
		localF = Mathf.Clamp01(localF);
		if (localF == 1f)
		{
			frag = 1f;
			return Approximation.Length - 2;
		}
		float num = localF / mStepSize;
		int num2 = (int)num;
		frag = num - (float)num2;
		return num2;
	}

	public void _InitializeControlPoint()
	{
		mSegmentIndex = -1;
		mStepSize = 1f / (float)Spline.Granularity;
		mControlPointIndex = Spline.ControlPoints.IndexOf(this);
		Approximation = new Vector3[0];
		ApproximationDistances = new float[0];
		ApproximationUp = new Vector3[0];
		ApproximationT = new Vector3[0];
		if (UserValues.Length != Spline.UserValueSize)
		{
			Array.Resize(ref UserValues, Spline.UserValueSize);
		}
	}

	public void _UpdateApproximation()
	{
		mSegmentIndex = Spline.Segments.IndexOf(this);
		mStepSize = 1f / (float)Spline.Granularity;
		Approximation = new Vector3[Spline.Granularity + 1];
		ApproximationUp = new Vector3[Spline.Granularity + 1];
		ApproximationT = new Vector3[Spline.Granularity + 1];
		Approximation[0] = Position;
		Approximation[Spline.Granularity] = NextTransform.position;
		int num = Approximation.Length - 1;
		for (int i = 1; i < num; i++)
		{
			Approximation[i] = Interpolate((float)i * mStepSize);
		}
	}

	public float _UpdateLength()
	{
		CurvySplineSegment previousControlPoint = PreviousControlPoint;
		Distance = ((!previousControlPoint || !previousControlPoint.IsValidSegment || IsFirstSegment) ? 0f : (previousControlPoint.Distance + previousControlPoint.Length));
		Length = 0f;
		if (IsValidSegment)
		{
			ApproximationDistances = new float[Approximation.Length];
			int num = ApproximationDistances.Length;
			for (int i = 1; i < num; i++)
			{
				ApproximationDistances[i] = ApproximationDistances[i - 1] + (Approximation[i] - Approximation[i - 1]).magnitude;
			}
			Length = ApproximationDistances[ApproximationDistances.Length - 1];
		}
		return Length;
	}

	public void _UpdateTangents()
	{
		if (IsValidSegment)
		{
			mStepSize = 1f / (float)Spline.Granularity;
			int num = ApproximationT.Length;
			for (int i = 0; i < num; i++)
			{
				ApproximationT[i] = GetTangent((float)i * mStepSize, ref Approximation[i]);
			}
			if (SmoothEdgeTangent && !IsFirstSegment)
			{
				PreviousSegment.ApproximationT[Spline.Granularity] = Vector3.Lerp(PreviousSegment.ApproximationT[Spline.Granularity - 1], ApproximationT[0], 0.5f);
				ApproximationT[0] = PreviousSegment.ApproximationT[Spline.Granularity];
			}
			if (IsLastSegment && (bool)NextSegment && Spline[0].SmoothEdgeTangent)
			{
				ApproximationT[Spline.Granularity] = Vector3.Lerp(ApproximationT[Spline.Granularity - 1], Spline[0].ApproximationT[0], 0.5f);
				Spline[0].ApproximationT[0] = ApproximationT[Spline.Granularity];
			}
		}
	}

	private Vector3 ParallelTransportFrame(ref Vector3 lastUp, ref Vector3 T1, ref Vector3 T2, float swirlangle)
	{
		Vector3 vector = Vector3.Cross(T1, T2);
		float num = Mathf.Atan2(vector.magnitude, Vector3.Dot(T1, T2));
		Quaternion quaternion = Quaternion.AngleAxis(57.29578f * num, vector.normalized);
		if (Spline.Swirl != CurvyOrientationSwirl.None)
		{
			return quaternion * Quaternion.AngleAxis(swirlangle, T2) * lastUp;
		}
		return quaternion * lastUp;
	}

	public Vector3 _UpdateOrientation(Vector3 lastUpVector)
	{
		float swirlangle = 0f;
		switch (Spline.Swirl)
		{
		case CurvyOrientationSwirl.Segment:
			swirlangle = Spline.SwirlTurns * 360f / (float)Spline.Granularity;
			break;
		case CurvyOrientationSwirl.Spline:
			swirlangle = Spline.SwirlTurns * 360f / (float)Spline.Count / (float)Spline.Granularity;
			break;
		}
		ApproximationUp[0] = lastUpVector;
		if (Spline.Orientation == CurvyOrientation.Tangent)
		{
			int num = Approximation.Length;
			for (int i = 1; i < num; i++)
			{
				lastUpVector = ParallelTransportFrame(ref lastUpVector, ref ApproximationT[i - 1], ref ApproximationT[i], swirlangle);
				ApproximationUp[i] = lastUpVector;
			}
		}
		else if (Spline.Orientation == CurvyOrientation.ControlPoints)
		{
			int num2 = Approximation.Length;
			for (int i = 0; i < num2; i++)
			{
				ApproximationUp[i] = Vector3.Lerp(Transform.up, NextTransform.up, (float)i * mStepSize);
			}
			lastUpVector = ApproximationUp[Spline.Granularity];
		}
		return lastUpVector;
	}

	public Vector3 _SmoothOrientation(Vector3 lastUpVector, ref float angleaccu, float angle)
	{
		ApproximationUp[0] = lastUpVector;
		for (int i = 1; i < ApproximationUp.Length; i++)
		{
			ApproximationUp[i] = Quaternion.AngleAxis(angleaccu, ApproximationT[i]) * ApproximationUp[i];
			angleaccu += angle;
		}
		return ApproximationUp[ApproximationUp.Length - 1];
	}

	public bool _PositionHasChanged()
	{
		if (!Transform)
		{
			return true;
		}
		bool flag = Transform.position != mPosition;
		if (flag)
		{
			if (Connection != null && Connection.Active && (Connection.Sync == CurvyConnection.SyncMode.SyncPos || Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot))
			{
				Connection.Other.Position = Position;
			}
			for (int i = 0; i < ConnectedBy.Count; i++)
			{
				if (ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPos || ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
				{
					ConnectedBy[i].Position = Position;
				}
			}
		}
		mPosition = Transform.position;
		return flag;
	}

	public bool _RotationHasChanged()
	{
		if (!Transform)
		{
			return true;
		}
		bool flag = Transform.rotation != mRotation;
		if (flag)
		{
			if (Connection != null && Connection.Active && (Connection.Sync == CurvyConnection.SyncMode.SyncRot || Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot))
			{
				Connection.Other.Transform.rotation = Transform.rotation;
			}
			for (int i = 0; i < ConnectedBy.Count; i++)
			{
				if (ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncRot || ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
				{
					ConnectedBy[i].Transform.rotation = Transform.rotation;
				}
			}
		}
		mRotation = Transform.rotation;
		return flag;
	}

	public void SyncConnections()
	{
		if (Connection.Active)
		{
			if (Connection.Sync == CurvyConnection.SyncMode.SyncPos || Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
			{
				Position = Connection.Other.Position;
			}
			if (Connection.Sync == CurvyConnection.SyncMode.SyncRot || Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
			{
				Transform.rotation = Connection.Other.Transform.rotation;
			}
			Connection.RefreshSplines();
		}
		for (int i = 0; i < ConnectedBy.Count; i++)
		{
			if (ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPos || ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
			{
				ConnectedBy[i].Position = Position;
				ConnectedBy[i].Connection.RefreshSplines();
			}
			if (ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncRot || ConnectedBy[i].Connection.Sync == CurvyConnection.SyncMode.SyncPosAndRot)
			{
				ConnectedBy[i].Transform.rotation = Transform.rotation;
				ConnectedBy[i].Connection.RefreshSplines();
			}
		}
	}

	public void _ReSettle()
	{
		mSpline = null;
		mControlPointIndex = -1;
		mSegmentIndex = -1;
	}
}
