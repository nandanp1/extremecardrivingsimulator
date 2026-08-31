using System;
using UnityEngine;

namespace Crosstales.BadWord
{
	public abstract class Manager : MonoBehaviour
	{
		public string MarkPrefix = "<b><color=red>";

		public string MarkPostfix = "</color></b>";

		protected static void logFilterIsNull(string clazz)
		{
			Debug.LogWarning("Filter is null!" + Environment.NewLine + "Did you add the '" + clazz + "' to the current scene?");
		}
	}
}
