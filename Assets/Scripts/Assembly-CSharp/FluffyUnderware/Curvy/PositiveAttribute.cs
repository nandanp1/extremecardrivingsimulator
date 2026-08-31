using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class PositiveAttribute : PropertyAttribute
	{
		public string Tooltip;

		public PositiveAttribute()
		{
		}

		public PositiveAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}
}
