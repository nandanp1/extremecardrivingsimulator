using FluffyUnderware.Curvy;
using UnityEngine;

public class CurvyFollowSplineAndLookAtTarget : FollowSpline
{
	[Label(Tooltip = "The Transform to look at")]
	public Transform LookAtTarget;

	public override void Refresh()
	{
		base.Refresh();
		if ((bool)LookAtTarget)
		{
			base.Transform.LookAt(LookAtTarget);
		}
	}
}
