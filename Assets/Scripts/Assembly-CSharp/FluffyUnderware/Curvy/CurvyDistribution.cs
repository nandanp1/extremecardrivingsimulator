using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[Serializable]
	public class CurvyDistribution
	{
		public enum DistributionMode
		{
			NonLinear = 0,
			Distance = 1,
			Adaptive = 2
		}

		public DistributionMode Mode;

		public float Step;

		public float MinDistance;

		public float Angle;

		public CurvyDistribution()
		{
			Mode = DistributionMode.Distance;
			Step = 1f;
			MinDistance = 0.2f;
			Angle = 5f;
		}

		public CurvyDistribution(DistributionMode mode, float step, float angle, float minDistance)
		{
			Mode = mode;
			Step = step;
			MinDistance = minDistance;
			Angle = angle;
		}

		public static CurvyDistribution NonLinear(float step)
		{
			return new CurvyDistribution(DistributionMode.NonLinear, step, 0f, 5f);
		}

		public static CurvyDistribution Distance(float step)
		{
			return new CurvyDistribution(DistributionMode.Distance, step, 0f, 5f);
		}

		public static CurvyDistribution Adaptive(float angle, float minDistance)
		{
			return new CurvyDistribution(DistributionMode.Adaptive, -1f, angle, minDistance);
		}

		public void Validate()
		{
			if (Mathf.Approximately(0f, Angle))
			{
				Angle = 0.001f;
			}
			if (MinDistance < 0.001f)
			{
				MinDistance = 0.001f;
			}
			if (Step < 0.0001f)
			{
				Step = 0.0001f;
			}
		}

		public bool Equals(CurvyDistribution with)
		{
			return Mode == with.Mode && Mathf.Approximately(Step, with.Step) && Mathf.Approximately(MinDistance, with.MinDistance) && Mathf.Approximately(Angle, with.Angle);
		}

		public override int GetHashCode()
		{
			return new
			{
				A = Mode,
				B = Step,
				C = MinDistance,
				D = Angle
			}.GetHashCode();
		}
	}
}
