using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXNativeTargetData : PBXObjectData
	{
		public GUIDList phases;

		public string buildConfigList;

		public string name;

		public GUIDList dependencies;

		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[5] { "buildPhases/*", "buildRules/*", "dependencies/*", "productReference/*", "buildConfigurationList/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXNativeTargetData Create(string name, string productRef, string productType, string buildConfigList)
		{
			PBXNativeTargetData pBXNativeTargetData = new PBXNativeTargetData();
			pBXNativeTargetData.guid = PBXGUID.Generate();
			pBXNativeTargetData.SetPropertyString("isa", "PBXNativeTarget");
			pBXNativeTargetData.buildConfigList = buildConfigList;
			pBXNativeTargetData.phases = new GUIDList();
			pBXNativeTargetData.SetPropertyList("buildRules", new List<string>());
			pBXNativeTargetData.dependencies = new GUIDList();
			pBXNativeTargetData.name = name;
			pBXNativeTargetData.SetPropertyString("productName", name);
			pBXNativeTargetData.SetPropertyString("productReference", productRef);
			pBXNativeTargetData.SetPropertyString("productType", productType);
			return pBXNativeTargetData;
		}

		public override void UpdateProps()
		{
			SetPropertyString("buildConfigurationList", buildConfigList);
			SetPropertyString("name", name);
			SetPropertyList("buildPhases", phases);
			SetPropertyList("dependencies", dependencies);
		}

		public override void UpdateVars()
		{
			buildConfigList = GetPropertyString("buildConfigurationList");
			name = GetPropertyString("name");
			phases = GetPropertyList("buildPhases");
			dependencies = GetPropertyList("dependencies");
		}
	}
}
