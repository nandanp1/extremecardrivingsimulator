using System.Collections;
using FluffyUnderware.Curvy;
using UnityEngine;

[ExecuteInEditMode]
public class SplineWalkerCon : MonoBehaviour
{
	public CurvySpline Spline;

	public CurvyClamping Clamping;

	public bool SetOrientation = true;

	public bool FastInterpolation;

	public bool MoveByWorldUnits;

	public float InitialF;

	public float Speed;

	public string AdditionalTags = string.Empty;

	public int MinTagMatches = 1;

	public string ReverseSuffix = "Rev";

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

	public string[] ResultingTags
	{
		get
		{
			return buildTags();
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
				mTransform.position = ((!FastInterpolation) ? Spline.MoveByConnection(ref Spline, ref mTF, ref direction, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()) : Spline.MoveByConnectionFast(ref Spline, ref mTF, ref direction, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()));
			}
			else
			{
				mTransform.position = ((!FastInterpolation) ? Spline.MoveConnection(ref Spline, ref mTF, ref direction, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()) : Spline.MoveConnectionFast(ref Spline, ref mTF, ref direction, Speed * Time.deltaTime, Clamping, MinTagMatches, buildTags()));
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

	private string[] buildTags()
	{
		if (Forward)
		{
			return (Spline.name + " " + AdditionalTags).Split(' ');
		}
		return (Spline.name + ReverseSuffix + " " + AdditionalTags).Split(' ');
	}
}
