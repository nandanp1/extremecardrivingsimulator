using FluffyUnderware.Curvy;
using UnityEngine;

[RequireComponent(typeof(FollowSpline))]
public class CurvyFollowSplineController : MonoBehaviour
{
	private float _interpolationStartSpeed;

	private float _interpolationStartTime;

	private float _interpolationEndTime;

	[HideInInspector]
	public bool isPlaying;

	private float _desireSpeed;

	private float _nextDesireSpeed;

	private float _currentControlPointSpeed;

	private CurvySplineSegment _currentDestinyControlPoint;

	private FollowSpline _followSpline;

	private CurvySplineSegment _currentSegment;

	private CurvySplineSegment _nextControlPoint;

	private CurvySplineSegmentExtension _nextControlPointExtension;

	private CurvySplineSegment _interpolatedControlPoint;

	private bool _interpolate;

	private bool _first = true;

	private float _elapsed;

	private float _timer;

	private float _interpolationTime;

	private void Awake()
	{
		_followSpline = GetComponent<FollowSpline>();
	}

	private void Start()
	{
		_followSpline.Mode = FollowSpline.FollowMode.AbsolutePrecise;
		_followSpline.Speed = 0f;
		_interpolationTime = _followSpline.Spline.GetComponent<CurvySplineExtension>().interpolationTime;
	}

	private void Update()
	{
		if (IsFinalized())
		{
			isPlaying = false;
			_first = true;
		}
		if (!isPlaying || !_followSpline.Spline.IsInitialized || _followSpline.CurrentSegment == null || _followSpline.CurrentSegment.IsLastControlPoint)
		{
			return;
		}
		if (_first)
		{
			_first = false;
			_timer = Time.time;
			_elapsed = 0f;
		}
		if (_timer > 0f)
		{
			_elapsed = Time.time - _timer;
		}
		else
		{
			_elapsed = 0f;
		}
		if (_currentSegment != _followSpline.CurrentSegment)
		{
			_currentSegment = _followSpline.CurrentSegment;
			_nextControlPoint = GetNextNotIngoredControlPoint(_currentSegment);
			_nextControlPointExtension = _nextControlPoint.GetComponent<CurvySplineSegmentExtension>();
			_desireSpeed = _nextControlPointExtension.desireSpeed;
			_interpolatedControlPoint = GetNextNotIngoredControlPoint(_nextControlPoint);
			if (_nextControlPoint.IsLastControlPoint)
			{
				_nextDesireSpeed = 0f;
			}
			else
			{
				_nextDesireSpeed = _interpolatedControlPoint.GetComponent<CurvySplineSegmentExtension>().desireSpeed;
			}
			_interpolationStartTime = 0f;
		}
		if (_interpolationTime > 0f)
		{
			if (!_nextControlPoint.GetComponent<CurvySplineSegmentExtension>().ignoreTimeToCome)
			{
				float num = _followSpline.Speed * _interpolationTime / _currentSegment.Length;
				if (1f - _followSpline.CurrentSegmentF < 0.1f)
				{
					if (_interpolationStartTime == 0f)
					{
						_interpolationStartSpeed = _followSpline.Speed;
						_interpolationStartTime = Time.time;
						_interpolationEndTime = _interpolationStartTime + _interpolationTime;
					}
					if (Time.time < _interpolationEndTime)
					{
						float t = (Time.time - _interpolationStartTime) / _interpolationTime;
						_followSpline.Speed = Mathf.Lerp(_interpolationStartSpeed, _nextDesireSpeed, t);
					}
					else
					{
						_followSpline.Speed = _desireSpeed;
					}
				}
				else
				{
					_followSpline.Speed = _desireSpeed;
				}
			}
			else
			{
				_followSpline.Speed = _desireSpeed;
			}
		}
		else
		{
			_followSpline.Speed = _desireSpeed;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(_followSpline.Spline.gameObject);
	}

	public void Play()
	{
		isPlaying = true;
	}

	public void Stop()
	{
		isPlaying = false;
	}

	public bool IsFinalized()
	{
		return _followSpline.CurrentTF == 1f;
	}

	private CurvySplineSegment GetNextNotIngoredControlPoint(CurvySplineSegment from)
	{
		if (from.IsLastControlPoint)
		{
			return from;
		}
		CurvySplineSegment nextControlPoint = from.NextControlPoint;
		if (nextControlPoint.IsLastControlPoint)
		{
			return nextControlPoint;
		}
		CurvySplineSegmentExtension component = nextControlPoint.GetComponent<CurvySplineSegmentExtension>();
		while (component.ignoreTimeToCome)
		{
			nextControlPoint = nextControlPoint.NextControlPoint;
			component = nextControlPoint.GetComponent<CurvySplineSegmentExtension>();
			if (nextControlPoint.IsLastControlPoint)
			{
				break;
			}
		}
		return nextControlPoint;
	}

	public void Restart()
	{
		_first = true;
		_elapsed = 0f;
		_timer = 0f;
		isPlaying = false;
		_followSpline.Initialize();
	}
}
