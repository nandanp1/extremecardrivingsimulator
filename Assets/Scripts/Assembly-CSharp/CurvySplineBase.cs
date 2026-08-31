using System.Collections.Generic;
using UnityEngine;

public class CurvySplineBase : MonoBehaviour
{
	public delegate void RefreshEvent(CurvySplineBase sender);

	protected bool mNeedLengthRefresh;

	protected bool mNeedOrientationRefresh;

	protected bool mSkipRefreshIfInitialized;

	protected float mLength;

	protected Transform mTransform;

	protected bool mIsInitialized;

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

	public bool IsInitialized
	{
		get
		{
			return mIsInitialized;
		}
	}

	public float Length
	{
		get
		{
			return mLength;
		}
	}

	public virtual bool IsContinuous
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsClosed
	{
		get
		{
			return false;
		}
	}

	public event RefreshEvent OnRefresh;

	protected void OnRefreshEvent(CurvySplineBase sender)
	{
		if (OnRefresh != null)
		{
			OnRefresh(sender);
		}
	}

	public virtual Vector3 Interpolate(float tf)
	{
		return Vector3.zero;
	}

	public virtual Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
	{
		return Vector3.zero;
	}

	public virtual Vector3 InterpolateFast(float tf)
	{
		return Vector3.zero;
	}

	public virtual Vector3 InterpolateUserValue(float tf, int index)
	{
		return Vector3.zero;
	}

	public virtual Vector3 InterpolateScale(float tf)
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetOrientationUpFast(float tf)
	{
		return Vector3.zero;
	}

	public virtual Quaternion GetOrientationFast(float tf)
	{
		return GetOrientationFast(tf, false);
	}

	public virtual Quaternion GetOrientationFast(float tf, bool inverse)
	{
		return Quaternion.identity;
	}

	public virtual Vector3 GetTangent(float tf)
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetTangent(float tf, Vector3 position)
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetTangentFast(float tf)
	{
		return Vector3.zero;
	}

	public virtual float TFToDistance(float tf)
	{
		return 0f;
	}

	public virtual float DistanceToTF(float distance)
	{
		return 0f;
	}

	public virtual CurvySplineSegment TFToSegment(float tf)
	{
		return null;
	}

	public virtual CurvySplineSegment TFToSegment(float tf, out float localF)
	{
		localF = 0f;
		return null;
	}

	public virtual float SegmentToTF(CurvySplineSegment segment)
	{
		return SegmentToTF(segment, 0f);
	}

	public virtual float SegmentToTF(CurvySplineSegment segment, float localF)
	{
		return 0f;
	}

	public virtual float GetNearestPointTF(Vector3 p)
	{
		return 0f;
	}

	public virtual void Clear()
	{
	}

	public virtual Vector3[] GetApproximation()
	{
		return GetApproximation(false);
	}

	public virtual Vector3[] GetApproximation(bool local)
	{
		return new Vector3[0];
	}

	public virtual Vector3[] GetApproximationT()
	{
		return new Vector3[0];
	}

	public virtual Vector3[] GetApproximationUpVectors()
	{
		return new Vector3[0];
	}

	public virtual Vector3[] GetPolygonByAngle(float angle, float minDistance)
	{
		if (Mathf.Approximately(angle, 0f))
		{
			Debug.LogError("CurvySplineBase.GetPolygonByAngle: angle must be greater than 0!");
			return new Vector3[0];
		}
		List<Vector3> list = new List<Vector3>();
		float tf = 0f;
		int direction = 1;
		float num = minDistance * minDistance;
		list.Add(Interpolate(0f));
		while (tf < 1f)
		{
			Vector3 vector = MoveByAngle(ref tf, ref direction, angle, CurvyClamping.Clamp);
			if ((vector - list[list.Count - 1]).sqrMagnitude >= num)
			{
				list.Add(vector);
			}
		}
		return list.ToArray();
	}

	public virtual Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
	{
		tf += fDistance * (float)direction;
		ClampTF(ref tf, ref direction, clamping);
		return Interpolate(tf);
	}

	public virtual Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
	{
		tf += fDistance * (float)direction;
		ClampTF(ref tf, ref direction, clamping);
		return InterpolateFast(tf);
	}

	public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping)
	{
		return MoveBy(ref tf, ref direction, distance, clamping, 0.002f);
	}

	public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
	{
		return Move(ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping);
	}

	public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
	{
		return MoveByFast(ref tf, ref direction, distance, clamping, 0.002f);
	}

	public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
	{
		return MoveFast(ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping);
	}

	public virtual Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
	{
		float num = TFToDistance(tf);
		float num2 = ((clamping == CurvyClamping.Clamp) ? (distance * (float)direction) : (distance % Length * (float)direction));
		float num3 = num + num2;
		if (num3 > Length || num3 < 0f)
		{
			switch (clamping)
			{
			case CurvyClamping.Clamp:
				tf = ((!(num2 < 0f)) ? 1 : 0);
				break;
			case CurvyClamping.PingPong:
				num3 = ((!(num2 < 0f)) ? (num3 - (num2 - Length)) : (num3 * -1f));
				direction *= -1;
				tf = DistanceToTF(num3);
				break;
			case CurvyClamping.Loop:
				num3 = ((!(num3 < 0f)) ? (num3 - Length) : (num3 + Length));
				tf = DistanceToTF(num3);
				break;
			}
		}
		else
		{
			tf = DistanceToTF(num3);
		}
		return InterpolateFast(tf);
	}

	public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping)
	{
		return MoveByAngle(ref tf, ref direction, angle, clamping, 0.002f);
	}

	public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
	{
		if (clamping == CurvyClamping.PingPong)
		{
			Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
			return Vector3.zero;
		}
		stepSize = Mathf.Max(0.0001f, stepSize);
		float num = tf;
		Vector3 vector = Interpolate(tf);
		Vector3 tangent = GetTangent(tf, vector);
		Vector3 vector2 = Vector3.zero;
		int num2 = 10000;
		while (num2-- > 0)
		{
			tf += stepSize * (float)direction;
			if (tf > 1f)
			{
				if (clamping != CurvyClamping.Loop)
				{
					tf = 1f;
					return Interpolate(1f);
				}
				tf--;
			}
			else if (tf < 0f)
			{
				if (clamping != CurvyClamping.Loop)
				{
					tf = 0f;
					return Interpolate(0f);
				}
				tf++;
			}
			vector2 = Interpolate(tf);
			Vector3 to = vector2 - vector;
			float num3 = Vector3.Angle(tangent, to);
			if (num3 >= angle)
			{
				tf = num + (tf - num) * angle / num3;
				return Interpolate(tf);
			}
		}
		return vector2;
	}

	public virtual Vector3 MoveByAngleFast(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
	{
		if (clamping == CurvyClamping.PingPong)
		{
			Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
			return Vector3.zero;
		}
		stepSize = Mathf.Max(0.0001f, stepSize);
		float num = tf;
		Vector3 vector = InterpolateFast(tf);
		Vector3 tangentFast = GetTangentFast(tf);
		Vector3 vector2 = Vector3.zero;
		int num2 = 10000;
		while (num2-- > 0)
		{
			tf += stepSize * (float)direction;
			if (tf > 1f)
			{
				if (clamping != CurvyClamping.Loop)
				{
					tf = 1f;
					return InterpolateFast(1f);
				}
				tf--;
			}
			else if (tf < 0f)
			{
				if (clamping != CurvyClamping.Loop)
				{
					tf = 0f;
					return InterpolateFast(0f);
				}
				tf++;
			}
			vector2 = InterpolateFast(tf);
			Vector3 to = vector2 - vector;
			float num3 = Vector3.Angle(tangentFast, to);
			if (num3 >= angle)
			{
				tf = num + (tf - num) * angle / num3;
				return InterpolateFast(tf);
			}
		}
		return vector2;
	}

	public virtual Vector3 GetExtrusionPoint(float tf, float radius, float angle)
	{
		Vector3 vector = Interpolate(tf);
		Vector3 tangent = GetTangent(tf, vector);
		Quaternion quaternion = Quaternion.AngleAxis(angle, tangent);
		return vector + quaternion * GetOrientationUpFast(tf) * radius;
	}

	public virtual Vector3 GetExtrusionPointFast(float tf, float radius, float angle)
	{
		Vector3 vector = InterpolateFast(tf);
		Vector3 tangentFast = GetTangentFast(tf);
		Quaternion quaternion = Quaternion.AngleAxis(angle, tangentFast);
		return vector + quaternion * GetOrientationUpFast(tf) * radius;
	}

	public virtual Vector3 GetTangentByDistance(float distance)
	{
		return GetTangent(DistanceToTF(distance));
	}

	public virtual Vector3 GetTangentByDistanceFast(float distance)
	{
		return GetTangentFast(DistanceToTF(distance));
	}

	public virtual Vector3 InterpolateByDistance(float distance)
	{
		return Interpolate(DistanceToTF(distance));
	}

	public virtual Vector3 InterpolateByDistanceFast(float distance)
	{
		return InterpolateFast(DistanceToTF(distance));
	}

	public float ExtrapolateDistanceToTF(float tf, float distance, float stepSize)
	{
		stepSize = Mathf.Max(0.0001f, stepSize);
		Vector3 vector = Interpolate(tf);
		float num = ((tf != 1f) ? Mathf.Min(1f, tf + stepSize) : Mathf.Min(1f, tf - stepSize));
		stepSize = Mathf.Abs(num - tf);
		Vector3 vector2 = Interpolate(num);
		float magnitude = (vector2 - vector).magnitude;
		if (magnitude != 0f)
		{
			return 1f / magnitude * stepSize * distance;
		}
		return 0f;
	}

	public float ExtrapolateDistanceToTFFast(float tf, float distance, float stepSize)
	{
		stepSize = Mathf.Max(0.0001f, stepSize);
		Vector3 vector = InterpolateFast(tf);
		float num = ((tf != 1f) ? Mathf.Min(1f, tf + stepSize) : Mathf.Min(1f, tf - stepSize));
		stepSize = Mathf.Abs(num - tf);
		Vector3 vector2 = InterpolateFast(num);
		float magnitude = (vector2 - vector).magnitude;
		if (magnitude != 0f)
		{
			return 1f / magnitude * stepSize * distance;
		}
		return 0f;
	}

	public virtual void Destroy()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public virtual Bounds GetBounds(bool local)
	{
		Vector3[] approximation = GetApproximation(local);
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < approximation.Length; i++)
		{
			vector.x = Mathf.Min(vector.x, approximation[i].x);
			vector.y = Mathf.Min(vector.y, approximation[i].y);
			vector.z = Mathf.Min(vector.z, approximation[i].z);
			vector2.x = Mathf.Max(vector2.x, approximation[i].x);
			vector2.y = Mathf.Max(vector2.y, approximation[i].y);
			vector2.z = Mathf.Max(vector2.z, approximation[i].z);
		}
		return new Bounds(vector + (vector2 - vector) / 2f, vector2 - vector);
	}

	public virtual void Refresh()
	{
		Refresh(true, true, false);
	}

	public virtual void Refresh(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
	{
		mNeedLengthRefresh = refreshLength;
		mNeedOrientationRefresh = refreshOrientation;
		if (mSkipRefreshIfInitialized)
		{
			mSkipRefreshIfInitialized = skipIfInitialized;
		}
	}

	public virtual void RefreshImmediately()
	{
		RefreshImmediately(true, true, false);
	}

	public virtual void RefreshImmediately(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
	{
		mNeedLengthRefresh = false;
		mNeedOrientationRefresh = false;
		mSkipRefreshIfInitialized = true;
	}

	protected virtual bool ClampTF(ref float tf, ref int dir, CurvyClamping clamping)
	{
		switch (clamping)
		{
		case CurvyClamping.Loop:
			if (tf < 0f)
			{
				tf = 1f - Mathf.Abs(tf % 1f);
				return true;
			}
			if (tf > 1f)
			{
				tf %= 1f;
				return true;
			}
			break;
		case CurvyClamping.PingPong:
			if (tf < 0f)
			{
				tf = tf % 1f * -1f;
				dir *= -1;
				return true;
			}
			if (tf > 1f)
			{
				tf = 1f - tf % 1f;
				dir *= -1;
				return true;
			}
			break;
		default:
			tf = Mathf.Clamp01(tf);
			break;
		}
		return false;
	}
}
