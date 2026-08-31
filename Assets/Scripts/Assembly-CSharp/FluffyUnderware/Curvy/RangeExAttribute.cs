using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class RangeExAttribute : PropertyAttribute
	{
		public string Tooltip;

		public float Min;

		public float Max;

		public RangeExAttribute(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public RangeExAttribute(float min, float max, string tooltip)
		{
			Min = min;
			Max = max;
			Tooltip = tooltip;
		}
	}
}
