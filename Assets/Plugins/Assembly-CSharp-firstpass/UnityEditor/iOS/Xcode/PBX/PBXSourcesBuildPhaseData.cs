using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXSourcesBuildPhaseData : FileGUIDListBase
	{
		public static PBXSourcesBuildPhaseData Create()
		{
			PBXSourcesBuildPhaseData pBXSourcesBuildPhaseData = new PBXSourcesBuildPhaseData();
			pBXSourcesBuildPhaseData.guid = PBXGUID.Generate();
			pBXSourcesBuildPhaseData.SetPropertyString("isa", "PBXSourcesBuildPhase");
			pBXSourcesBuildPhaseData.SetPropertyString("buildActionMask", "2147483647");
			pBXSourcesBuildPhaseData.files = new List<string>();
			pBXSourcesBuildPhaseData.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
			return pBXSourcesBuildPhaseData;
		}
	}
}
