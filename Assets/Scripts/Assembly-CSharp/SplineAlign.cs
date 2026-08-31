using System.Collections;
using FluffyUnderware.Curvy;
using UnityEngine;

[ExecuteInEditMode]
public class SplineAlign : MonoBehaviour
{
	public CurvySplineBase Spline;

	public float Distance;

	public bool UseWorldUnits;

	public bool SetOrientation = true;

	public CurvyUpdateMethod UpdateIn;

	private IEnumerator Start()
	{
		if ((bool)Spline)
		{
			while (!Spline.IsInitialized)
			{
				yield return null;
			}
			Set();
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
		if ((bool)Spline && !Application.isPlaying)
		{
			Set();
		}
	}

	private void Set()
	{
		float tf;
		if (UseWorldUnits)
		{
			if (Distance >= Spline.Length)
			{
				Distance -= Spline.Length;
			}
			else if (Distance < 0f)
			{
				Distance += Spline.Length;
			}
			tf = Spline.DistanceToTF(Distance);
		}
		else
		{
			if (Distance >= 1f)
			{
				Distance--;
			}
			else if (Distance < 0f)
			{
				Distance++;
			}
			tf = Distance;
		}
		if (base.transform.position != Spline.Interpolate(tf))
		{
			base.transform.position = Spline.Interpolate(tf);
		}
		if (SetOrientation && base.transform.rotation != Spline.GetOrientationFast(tf))
		{
			base.transform.rotation = Spline.GetOrientationFast(tf);
		}
	}
}
