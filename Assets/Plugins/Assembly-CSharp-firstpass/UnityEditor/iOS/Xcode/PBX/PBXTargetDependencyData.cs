namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXTargetDependencyData : PBXObjectData
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[2] { "target/*", "targetProxy/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXTargetDependencyData Create(string target, string targetProxy)
		{
			PBXTargetDependencyData pBXTargetDependencyData = new PBXTargetDependencyData();
			pBXTargetDependencyData.guid = PBXGUID.Generate();
			pBXTargetDependencyData.SetPropertyString("isa", "PBXTargetDependency");
			pBXTargetDependencyData.SetPropertyString("target", target);
			pBXTargetDependencyData.SetPropertyString("targetProxy", targetProxy);
			return pBXTargetDependencyData;
		}
	}
}
