using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[Serializable]
	public class CurvyRange
	{
		public enum RangeMode
		{
			Relative = 0,
			Absolute = 1,
			NonLinear = 2
		}

		public RangeMode Mode;

		public float From;

		public float To;

		public float MaxDistance;

		public float Length
		{
			get
			{
				return To - From;
			}
		}

		public CurvyRange()
		{
			Mode = RangeMode.Relative;
			From = 0f;
			To = 1f;
		}

		public CurvyRange(RangeMode mode, float from, float to)
		{
			Mode = mode;
			From = from;
			To = to;
		}

		public static CurvyRange Relative(float from, float to)
		{
			CurvyRange curvyRange = new CurvyRange(RangeMode.Relative, from, to);
			curvyRange.Validate();
			return curvyRange;
		}

		public static CurvyRange Absolute(float from, float to)
		{
			CurvyRange curvyRange = new CurvyRange(RangeMode.Absolute, from, to);
			curvyRange.Validate();
			return curvyRange;
		}

		public static CurvyRange NonLinear(float from, float to)
		{
			CurvyRange curvyRange = new CurvyRange(RangeMode.NonLinear, from, to);
			curvyRange.Validate();
			return curvyRange;
		}

		public void Validate(float maxDistance)
		{
			MaxDistance = maxDistance;
			Validate();
		}

		public void Validate()
		{
			if (From < 0f)
			{
				From = 0f;
			}
			if (Mode == RangeMode.Absolute)
			{
				if (To > MaxDistance)
				{
					To = MaxDistance;
				}
			}
			else
			{
				if (From > 1f)
				{
					From = 1f;
				}
				if (To > 1f)
				{
					To = 1f;
				}
			}
			if (To < From)
			{
				To = From;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			CurvyRange curvyRange = obj as CurvyRange;
			if (curvyRange == null)
			{
				return false;
			}
			return Equals(curvyRange);
		}

		public bool Equals(CurvyRange with)
		{
			return Mode == with.Mode && Mathf.Approximately(From, with.From) && Mathf.Approximately(To, with.To);
		}

		public override int GetHashCode()
		{
			return new
			{
				A = Mode,
				B = From,
				C = To
			}.GetHashCode();
		}
	}
}
