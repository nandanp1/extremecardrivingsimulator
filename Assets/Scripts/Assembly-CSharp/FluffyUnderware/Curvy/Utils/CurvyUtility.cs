using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class CurvyUtility
	{
		private static float mEditorDeltaTime;

		private static float mEditorLastTime;

		private static int mEditorLastFrame;

		public static float DeltaTime
		{
			get
			{
				return (!Application.isPlaying) ? mEditorDeltaTime : Time.deltaTime;
			}
		}

		public static void SetEditorTiming()
		{
			int num = Time.frameCount - mEditorLastFrame;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num2 = realtimeSinceStartup - mEditorLastTime;
			if (num > 20 || num2 > 1f)
			{
				mEditorLastFrame = Time.frameCount;
				mEditorLastTime = realtimeSinceStartup;
				mEditorDeltaTime = 0f;
			}
			else if (num > 0)
			{
				mEditorDeltaTime = num2 / (float)num;
				mEditorLastTime = realtimeSinceStartup;
				mEditorLastFrame = Time.frameCount;
			}
		}

		[Obsolete("Use IsPlanar() instead")]
		public static bool isPlanar(CurvySpline spline, out int ignoreAxis)
		{
			return IsPlanar(spline, out ignoreAxis);
		}

		[Obsolete("Use IsPlanar() instead")]
		public static bool isPlanar(CurvySpline spline, out bool xplanar, out bool yplanar, out bool zplanar)
		{
			return IsPlanar(spline, out xplanar, out yplanar, out zplanar);
		}

		[Obsolete("Use MakePlanar() instead")]
		public static void makePlanar(CurvySpline spline, int axis)
		{
			MakePlanar(spline, axis);
		}

		[Obsolete("Use CenterPivot() instead")]
		public static void centerPivot(CurvySpline spline)
		{
			CenterPivot(spline);
		}

		[Obsolete("Use SetFirstCP() instead")]
		public static void setFirstCP(CurvySplineSegment newStartCP)
		{
			SetFirstCP(newStartCP);
		}

		public static bool IsPlanar(CurvySpline spline, out int ignoreAxis)
		{
			bool xplanar;
			bool yplanar;
			bool zplanar;
			bool result = IsPlanar(spline, out xplanar, out yplanar, out zplanar);
			if (xplanar)
			{
				ignoreAxis = 0;
			}
			else if (yplanar)
			{
				ignoreAxis = 1;
			}
			else
			{
				ignoreAxis = 2;
			}
			return result;
		}

		public static bool IsPlanar(CurvySpline spline, out bool xplanar, out bool yplanar, out bool zplanar)
		{
			xplanar = true;
			yplanar = true;
			zplanar = true;
			if (spline.ControlPointCount == 0)
			{
				return false;
			}
			Vector3 position = spline.ControlPoints[0].Position;
			for (int i = 1; i < spline.ControlPointCount; i++)
			{
				if (!Mathf.Approximately(spline.ControlPoints[i].Position.x, position.x))
				{
					xplanar = false;
				}
				if (!Mathf.Approximately(spline.ControlPoints[i].Position.y, position.y))
				{
					yplanar = false;
				}
				if (!Mathf.Approximately(spline.ControlPoints[i].Position.z, position.z))
				{
					zplanar = false;
				}
				if (!xplanar && !yplanar && !zplanar)
				{
					return false;
				}
			}
			return true;
		}

		public static void MakePlanar(CurvySpline spline, int axis)
		{
			Vector3 position = spline.ControlPoints[0].Position;
			for (int i = 1; i < spline.ControlPointCount; i++)
			{
				Vector3 position2 = spline.ControlPoints[i].Position;
				switch (axis)
				{
				case 0:
					position2.x = position.x;
					break;
				case 1:
					position2.y = position.y;
					break;
				case 2:
					position2.z = position.z;
					break;
				}
				spline.ControlPoints[i].Position = position2;
			}
			spline.Refresh();
		}

		public static void CenterPivot(CurvySpline spline)
		{
			Bounds bounds = spline.GetBounds(false);
			Vector3 vector = spline.Transform.position - bounds.center;
			foreach (CurvySplineSegment controlPoint in spline.ControlPoints)
			{
				controlPoint.Transform.position += vector;
			}
			spline.transform.position -= vector;
			spline.Refresh();
		}

		public static void SetFirstCP(CurvySplineSegment newStartCP)
		{
			CurvySpline spline = newStartCP.Spline;
			if (newStartCP.ControlPointIndex > 0)
			{
				CurvySplineSegment[] array = new CurvySplineSegment[newStartCP.ControlPointIndex];
				for (int i = 0; i < newStartCP.ControlPointIndex; i++)
				{
					array[i] = spline.ControlPoints[i];
				}
				CurvySplineSegment[] array2 = array;
				foreach (CurvySplineSegment item in array2)
				{
					spline.ControlPoints.Remove(item);
					spline.ControlPoints.Add(item);
				}
				spline._RenameControlPointsByIndex();
				spline.RefreshImmediately(true, true, false);
			}
		}

		public static void FlipSpline(CurvySpline spline)
		{
			spline.ControlPoints.Reverse();
			spline._RenameControlPointsByIndex();
			spline.RefreshImmediately(true, true, false);
		}

		public static CurvySpline SplitSpline(CurvySplineSegment firstCP)
		{
			CurvySpline spline = firstCP.Spline;
			CurvySpline curvySpline = CurvySpline.Create(spline);
			curvySpline.name = spline.name + "_parted";
			List<CurvySplineSegment> range = spline.ControlPoints.GetRange(firstCP.ControlPointIndex, spline.ControlPointCount - firstCP.ControlPointIndex);
			for (int i = 0; i < range.Count; i++)
			{
				range[i].Transform.parent = curvySpline.Transform;
				range[i]._ReSettle();
			}
			spline.ControlPoints.Clear();
			spline.RefreshImmediately(true, true, false);
			curvySpline._RenameControlPointsByIndex();
			curvySpline.RefreshImmediately(true, true, false);
			return curvySpline;
		}

		public static void JoinSpline(CurvySplineSegment sourceCP, CurvySplineSegment destCP)
		{
			if (!sourceCP || !destCP)
			{
				return;
			}
			CurvySpline spline = sourceCP.Spline;
			CurvySpline spline2 = destCP.Spline;
			if (!(spline == spline2))
			{
				for (int i = 0; i < spline.ControlPointCount; i++)
				{
					spline.ControlPoints[i].Transform.parent = spline2.Transform;
					spline.ControlPoints[i]._ReSettle();
				}
				spline2.ControlPoints.InsertRange(destCP.ControlPointIndex + 1, spline.ControlPoints);
				spline2._RenameControlPointsByIndex();
				spline2.RefreshImmediately(true, true, false);
				spline.Destroy();
			}
		}

		public static void InterpolateBezierHandles(CurvyInterpolation interpolation, float offset, bool freeMoveHandles, params CurvySplineSegment[] controlPoints)
		{
			if (controlPoints.Length == 0)
			{
				return;
			}
			offset = Mathf.Clamp01(offset);
			foreach (CurvySplineSegment curvySplineSegment in controlPoints)
			{
				curvySplineSegment.FreeHandles = freeMoveHandles;
				CurvySplineSegment previousSegment = curvySplineSegment.PreviousSegment;
				if ((bool)previousSegment)
				{
					curvySplineSegment.HandleInPosition = previousSegment.Interpolate(1f - offset, interpolation);
				}
				else
				{
					curvySplineSegment.HandleInPosition = curvySplineSegment.Interpolate(0f, interpolation);
				}
				if (freeMoveHandles)
				{
					if (curvySplineSegment.IsValidSegment)
					{
						curvySplineSegment.HandleOutPosition = curvySplineSegment.Interpolate(offset, interpolation);
					}
					else
					{
						curvySplineSegment.HandleInPosition = Vector3.zero;
					}
				}
			}
			controlPoints[0].Spline.Refresh();
		}

		public static Vector3 FixNaN(Vector3 v)
		{
			if (float.IsNaN(v.x))
			{
				v.x = 0f;
			}
			if (float.IsNaN(v.y))
			{
				v.y = 0f;
			}
			if (float.IsNaN(v.z))
			{
				v.z = 0f;
			}
			return v;
		}

		public static float GetHandleSize(Vector3 position)
		{
			Camera current = Camera.current;
			position = Gizmos.matrix.MultiplyPoint(position);
			if ((bool)current)
			{
				Transform transform = current.transform;
				Vector3 position2 = transform.position;
				float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
				Vector3 vector = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
				Vector3 vector2 = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
				float magnitude = (vector - vector2).magnitude;
				return 80f / Mathf.Max(magnitude, 0.0001f);
			}
			return 20f;
		}

		internal static void Grow(CurvySplineBase spline, Vector3 center, float growByPercent)
		{
			if (spline is CurvySplineGroup)
			{
				CurvySplineGroup curvySplineGroup = (CurvySplineGroup)spline;
				{
					foreach (CurvySpline spline2 in curvySplineGroup.Splines)
					{
						Grow(spline2, center, growByPercent);
					}
					return;
				}
			}
			CurvySpline curvySpline = (CurvySpline)spline;
			foreach (CurvySplineSegment controlPoint in curvySpline.ControlPoints)
			{
				Vector3 vector = center - controlPoint.Position;
				float magnitude = vector.magnitude;
				controlPoint.Position = center + vector.normalized * magnitude * growByPercent;
			}
		}
	}
}
