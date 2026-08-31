using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabEvolution
{
	[Serializable]
	public class PEModifications
	{
		[Serializable]
		public class PropertyData
		{
			public enum PropertyMode
			{
				Default = 0,
				Keep = 1,
				Ignore = 2
			}

			public UnityEngine.Object Object;

			public int ObjeckLink;

			public string PropertyPath;

			public PropertyMode Mode;

			public object UserData;
		}

		[Serializable]
		public class HierarchyData
		{
			public Transform child;

			public Transform parent;
		}

		[Serializable]
		public class ComponentsData
		{
			public Component child;

			public GameObject parent;
		}

		public List<PropertyData> Modificated = new List<PropertyData>();

		public List<HierarchyData> NonPrefabObjects = new List<HierarchyData>();

		public List<ComponentsData> NonPrefabComponents = new List<ComponentsData>();

		public List<int> RemovedObjects = new List<int>();

		public List<HierarchyData> TransformParentChanges = new List<HierarchyData>();
	}
}
