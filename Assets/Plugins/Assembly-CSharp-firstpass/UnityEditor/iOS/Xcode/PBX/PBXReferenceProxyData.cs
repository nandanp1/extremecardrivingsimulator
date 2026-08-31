namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXReferenceProxyData : PBXObjectData
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "remoteRef/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public string path
		{
			get
			{
				return GetPropertyString("path");
			}
		}

		public static PBXReferenceProxyData Create(string path, string fileType, string remoteRef, string sourceTree)
		{
			PBXReferenceProxyData pBXReferenceProxyData = new PBXReferenceProxyData();
			pBXReferenceProxyData.guid = PBXGUID.Generate();
			pBXReferenceProxyData.SetPropertyString("isa", "PBXReferenceProxy");
			pBXReferenceProxyData.SetPropertyString("path", path);
			pBXReferenceProxyData.SetPropertyString("fileType", fileType);
			pBXReferenceProxyData.SetPropertyString("remoteRef", remoteRef);
			pBXReferenceProxyData.SetPropertyString("sourceTree", sourceTree);
			return pBXReferenceProxyData;
		}
	}
}
