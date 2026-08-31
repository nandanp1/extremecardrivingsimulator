using System.Collections;
using FluffyUnderware.Curvy;
using UnityEngine;

[ExecuteInEditMode]
public class SplineWalker : MonoBehaviour
{
	public CurvySplineBase Spline;

	public CurvyClamping Clamping;

	public bool SetOrientation = true;

	public bool FastInterpolation;

	public bool MoveByWorldUnits;

	public float InitialF;

	public float Speed;

	public bool Forward = true;

	public CurvyUpdateMethod UpdateIn;

	private float mTF;

	private Transform mTransform;

	public float TF
	{
		get
		{
			return mTF;
		}
		set
		{
			mTF = value;
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
		mTF = InitialF;
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
			if (MoveByWorldUnits)
			{
				mTransform.position = ((!FastInterpolation) ? Spline.MoveBy(ref mTF, ref direction, Speed * Time.deltaTime, Clamping) : Spline.MoveByFast(ref mTF, ref direction, Speed * Time.deltaTime, Clamping));
			}
			else
			{
				mTransform.position = ((!FastInterpolation) ? Spline.Move(ref mTF, ref direction, Speed * Time.deltaTime, Clamping) : Spline.MoveFast(ref mTF, ref direction, Speed * Time.deltaTime, Clamping));
			}
			if (SetOrientation)
			{
				base.transform.rotation = Spline.GetOrientationFast(mTF);
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
			if (Spline.Interpolate(InitialF) != mTransform.position)
			{
				mTransform.position = Spline.Interpolate(InitialF);
			}
			if (SetOrientation && mTransform.rotation != Spline.GetOrientationFast(InitialF))
			{
				mTransform.rotation = Spline.GetOrientationFast(InitialF);
			}
		}
	}
}
