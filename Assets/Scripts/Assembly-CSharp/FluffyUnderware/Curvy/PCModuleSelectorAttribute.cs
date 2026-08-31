using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class PCModuleSelectorAttribute : PropertyAttribute
	{
		public string MissingMessage = "Source missing";
	}
}
