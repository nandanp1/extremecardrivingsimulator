namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXContainerItemProxyData : PBXObjectData
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "containerPortal/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXContainerItemProxyData Create(string containerRef, string proxyType, string remoteGlobalGUID, string remoteInfo)
		{
			PBXContainerItemProxyData pBXContainerItemProxyData = new PBXContainerItemProxyData();
			pBXContainerItemProxyData.guid = PBXGUID.Generate();
			pBXContainerItemProxyData.SetPropertyString("isa", "PBXContainerItemProxy");
			pBXContainerItemProxyData.SetPropertyString("containerPortal", containerRef);
			pBXContainerItemProxyData.SetPropertyString("proxyType", proxyType);
			pBXContainerItemProxyData.SetPropertyString("remoteGlobalIDString", remoteGlobalGUID);
			pBXContainerItemProxyData.SetPropertyString("remoteInfo", remoteInfo);
			return pBXContainerItemProxyData;
		}
	}
}
