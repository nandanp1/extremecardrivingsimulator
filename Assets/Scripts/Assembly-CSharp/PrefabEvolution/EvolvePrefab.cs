using UnityEngine;

namespace PrefabEvolution
{
	[AddComponentMenu("")]
	public class EvolvePrefab : PEPrefabScript
	{
		private void Awake()
		{
			if (!Application.isEditor)
			{
				Links = null;
				Modifications = null;
			}
		}

		[ContextMenu("HideInternals")]
		private void HideInternals()
		{
			SetHideInternalObjects(true);
		}

		[ContextMenu("ShowInternals")]
		private void ShowInternals()
		{
			SetHideInternalObjects(false);
		}
	}
}
