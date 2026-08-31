using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXFrameworksBuildPhaseData : FileGUIDListBase
	{
		public static PBXFrameworksBuildPhaseData Create()
		{
			PBXFrameworksBuildPhaseData pBXFrameworksBuildPhaseData = new PBXFrameworksBuildPhaseData();
			pBXFrameworksBuildPhaseData.guid = PBXGUID.Generate();
			pBXFrameworksBuildPhaseData.SetPropertyString("isa", "PBXFrameworksBuildPhase");
			pBXFrameworksBuildPhaseData.SetPropertyString("buildActionMask", "2147483647");
			pBXFrameworksBuildPhaseData.files = new List<string>();
			pBXFrameworksBuildPhaseData.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
			return pBXFrameworksBuildPhaseData;
		}
	}
}
