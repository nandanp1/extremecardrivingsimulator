using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class LabelAttribute : PropertyAttribute
	{
		public string Text = string.Empty;

		public string Tooltip;

		public LabelAttribute()
		{
		}

		public LabelAttribute(string text)
			: this(text, null)
		{
		}

		public LabelAttribute(string text, string tooltip)
		{
			Text = text;
			Tooltip = tooltip;
		}
	}
}
