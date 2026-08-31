using System.Collections;
using FluffyUnderware.Curvy;
using UnityEngine;

[ExecuteInEditMode]
public class SplineWalkerDistance : MonoBehaviour
{
	public CurvySplineBase Spline;

	public CurvyClamping Clamping;

	public bool SetOrientation = true;

	public bool FastInterpolation;

	public float InitialDistance;

	public float Speed;

	public bool Forward = true;

	public CurvyUpdateMethod UpdateIn;

	private float mDistance;

	private Transform mTransform;

	public float Distance
	{
		get
		{
			return mDistance;
		}
		set
		{
			mDistance = value;
		}
	}

	public int Dir
	{
		get
		{
			return Forward ? 1 : (-1);
		}
		set
		{
			bool flag = value >= 0;
			if (flag != Forward)
			{
				Forward = flag;
			}
		}
	}

	private IEnumerator Start()
	{
		mDistance = InitialDistance;
		Speed = Mathf.Abs(Speed);
		mTransform = base.transform;
		if ((bool)Spline)
		{
			while (!Spline.IsInitialized)
			{
				yield return null;
			}
			InitPosAndRot();
		}
	}

	private void Update()
	{
		if (UpdateIn == CurvyUpdateMethod.Update)
		{
			doUpdate();
		}
	}

	private void LateUpdate()
	{
		if (UpdateIn == CurvyUpdateMethod.LateUpdate)
		{
			doUpdate();
		}
	}

	private void FixedUpdate()
	{
		if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
		{
			doUpdate();
		}
	}

	private void doUpdate()
	{
		if (!Spline || !Spline.IsInitialized)
		{
			return;
		}
		if (Application.isPlaying)
		{
			int direction = Dir;
			float tf = Spline.DistanceToTF(mDistance);
			mTransform.position = ((!FastInterpolation) ? Spline.MoveBy(ref tf, ref direction, Speed * Time.deltaTime, Clamping) : Spline.MoveByFast(ref tf, ref direction, Speed * Time.deltaTime, Clamping));
			mDistance = Spline.TFToDistance(tf);
			if (SetOrientation)
			{
				base.transform.rotation = Spline.GetOrientationFast(tf);
			}
			Dir = direction;
		}
		else
		{
			InitPosAndRot();
		}
	}

	private void InitPosAndRot()
	{
		if ((bool)Spline)
		{
			float tf = Spline.DistanceToTF(InitialDistance);
			if (mTransform.position != Spline.Interpolate(tf))
			{
				mTransform.position = Spline.Interpolate(tf);
			}
			if (SetOrientation && mTransform.rotation != Spline.GetOrientationFast(tf))
			{
				mTransform.rotation = Spline.GetOrientationFast(tf);
			}
		}
	}
}
