using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXCopyFilesBuildPhaseData : FileGUIDListBase
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "files/*" });

		public string name;

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXCopyFilesBuildPhaseData Create(string name, string subfolderSpec)
		{
			PBXCopyFilesBuildPhaseData pBXCopyFilesBuildPhaseData = new PBXCopyFilesBuildPhaseData();
			pBXCopyFilesBuildPhaseData.guid = PBXGUID.Generate();
			pBXCopyFilesBuildPhaseData.SetPropertyString("isa", "PBXCopyFilesBuildPhase");
			pBXCopyFilesBuildPhaseData.SetPropertyString("buildActionMask", "2147483647");
			pBXCopyFilesBuildPhaseData.SetPropertyString("dstPath", string.Empty);
			pBXCopyFilesBuildPhaseData.SetPropertyString("dstSubfolderSpec", subfolderSpec);
			pBXCopyFilesBuildPhaseData.files = new List<string>();
			pBXCopyFilesBuildPhaseData.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
			pBXCopyFilesBuildPhaseData.name = name;
			return pBXCopyFilesBuildPhaseData;
		}

		public override void UpdateProps()
		{
			SetPropertyList("files", files);
			SetPropertyString("name", name);
		}

		public override void UpdateVars()
		{
			files = GetPropertyList("files");
			name = GetPropertyString("name");
		}
	}
}
