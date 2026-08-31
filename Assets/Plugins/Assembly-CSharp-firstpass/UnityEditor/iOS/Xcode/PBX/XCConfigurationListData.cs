namespace UnityEditor.iOS.Xcode.PBX
{
	internal class XCConfigurationListData : PBXObjectData
	{
		public GUIDList buildConfigs;

		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "buildConfigurations/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static XCConfigurationListData Create()
		{
			XCConfigurationListData xCConfigurationListData = new XCConfigurationListData();
			xCConfigurationListData.guid = PBXGUID.Generate();
			xCConfigurationListData.SetPropertyString("isa", "XCConfigurationList");
			xCConfigurationListData.buildConfigs = new GUIDList();
			xCConfigurationListData.SetPropertyString("defaultConfigurationIsVisible", "0");
			return xCConfigurationListData;
		}

		public override void UpdateProps()
		{
			SetPropertyList("buildConfigurations", buildConfigs);
		}

		public override void UpdateVars()
		{
			buildConfigs = GetPropertyList("buildConfigurations");
		}
	}
}
