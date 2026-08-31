using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class NetworkNodeSelectorAttribute : PropertyAttribute
	{
		public string Tooltip;

		public NetworkNodeSelectorAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}
}
