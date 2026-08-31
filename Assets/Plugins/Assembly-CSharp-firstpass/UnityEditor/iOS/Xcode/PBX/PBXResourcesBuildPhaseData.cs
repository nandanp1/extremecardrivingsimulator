using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXResourcesBuildPhaseData : FileGUIDListBase
	{
		public static PBXResourcesBuildPhaseData Create()
		{
			PBXResourcesBuildPhaseData pBXResourcesBuildPhaseData = new PBXResourcesBuildPhaseData();
			pBXResourcesBuildPhaseData.guid = PBXGUID.Generate();
			pBXResourcesBuildPhaseData.SetPropertyString("isa", "PBXResourcesBuildPhase");
			pBXResourcesBuildPhaseData.SetPropertyString("buildActionMask", "2147483647");
			pBXResourcesBuildPhaseData.files = new List<string>();
			pBXResourcesBuildPhaseData.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
			return pBXResourcesBuildPhaseData;
		}
	}
}
