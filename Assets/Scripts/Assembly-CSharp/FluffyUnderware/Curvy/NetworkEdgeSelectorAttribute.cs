using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class NetworkEdgeSelectorAttribute : PropertyAttribute
	{
		public string Tooltip;

		public NetworkEdgeSelectorAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}
}
