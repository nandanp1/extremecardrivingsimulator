using System;
using UnityEngine;

namespace PrefabEvolution
{
	[SelectionBase]
	[AddComponentMenu("")]
	public class PEPrefabScript : MonoBehaviour, ISerializationCallbackReceiver
	{
		public static class EditorBridge
		{
			public static Action<PEPrefabScript> OnValidate;

			public static Func<GameObject, string> GetAssetGuid;

			public static Func<string, GameObject> GetAssetByGuid;
		}

		[HideInInspector]
		public PEExposedProperties Properties = new PEExposedProperties();

		[HideInInspector]
		public PELinkage Links = new PELinkage();

		[HideInInspector]
		public PEModifications Modifications;

		public string ParentPrefabGUID;

		public string PrefabGUID;

		public GameObject ParentPrefab
		{
			get
			{
				return EditorBridge.GetAssetByGuid(ParentPrefabGUID);
			}
			set
			{
				string text = EditorBridge.GetAssetGuid(value);
				if (!string.IsNullOrEmpty(text))
				{
					ParentPrefabGUID = text;
				}
			}
		}

		public GameObject Prefab
		{
			get
			{
				return EditorBridge.GetAssetByGuid(PrefabGUID);
			}
			set
			{
				string text = EditorBridge.GetAssetGuid(value);
				if (!string.IsNullOrEmpty(text))
				{
					PrefabGUID = text;
				}
			}
		}

		public event Action OnBuildModifications;

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Properties.PrefabScript = this;
			Properties.InheritedProperties = null;
		}

		private void OnValidate()
		{
			if (EditorBridge.OnValidate != null)
			{
				EditorBridge.OnValidate(this);
			}
		}

		public void SetHideInternalObjects(bool value)
		{
			Component[] componentsInChildren = GetComponentsInChildren<Component>(true);
			foreach (Component component in componentsInChildren)
			{
				if (!(component is PEPrefabScript) && !(component == base.transform))
				{
					if (component is Transform)
					{
						SetObjectHide(component.gameObject, value);
					}
					else
					{
						SetObjectHide(component, value);
					}
				}
			}
		}

		public static void SetObjectHide(UnityEngine.Object obj, bool value)
		{
			obj.HideFlagsSet(HideFlags.HideInHierarchy | HideFlags.HideInInspector, value);
		}

		public void InvokeOnBuildModifications()
		{
			if (OnBuildModifications != null)
			{
				OnBuildModifications();
			}
		}
	}
}
