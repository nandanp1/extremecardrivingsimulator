using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(CurvySpline))]
public class CurvySplineExtension : MonoBehaviour
{
	private CurvySpline _curvySpline;

	public float interpolationTime;

	private void Awake()
	{
		_curvySpline = GetComponent<CurvySpline>();
	}

	public void InitializeForEditor()
	{
		Awake();
	}

	public CurvySplineSegment[] GetCurvyControlPoints()
	{
		if (_curvySpline.IsInitialized)
		{
			return _curvySpline.ControlPoints.ToArray();
		}
		return null;
	}

	public CurvySplineSegment[] GetCurvySegments()
	{
		if (_curvySpline.IsInitialized)
		{
			return _curvySpline.Segments.ToArray();
		}
		return null;
	}

	public CurvySplineSegmentExtension[] GetCurvySegmentsExtension()
	{
		if (_curvySpline.IsInitialized)
		{
			return _curvySpline.GetComponentsInChildren<CurvySplineSegmentExtension>();
		}
		return null;
	}
}
