using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXBuildFileData : PBXObjectData
	{
		public string fileRef;

		public string compileFlags;

		public bool weak;

		public List<string> assetTags;

		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "fileRef/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		internal override bool shouldCompact
		{
			get
			{
				return true;
			}
		}

		public static PBXBuildFileData CreateFromFile(string fileRefGUID, bool weak, string compileFlags)
		{
			PBXBuildFileData pBXBuildFileData = new PBXBuildFileData();
			pBXBuildFileData.guid = PBXGUID.Generate();
			pBXBuildFileData.SetPropertyString("isa", "PBXBuildFile");
			pBXBuildFileData.fileRef = fileRefGUID;
			pBXBuildFileData.compileFlags = compileFlags;
			pBXBuildFileData.weak = weak;
			pBXBuildFileData.assetTags = new List<string>();
			return pBXBuildFileData;
		}

		public override void UpdateProps()
		{
			SetPropertyString("fileRef", fileRef);
			PBXElementDict pBXElementDict = null;
			if (m_Properties.Contains("settings"))
			{
				pBXElementDict = m_Properties["settings"].AsDict();
			}
			if (compileFlags != null && compileFlags != string.Empty)
			{
				if (pBXElementDict == null)
				{
					pBXElementDict = m_Properties.CreateDict("settings");
				}
				pBXElementDict.SetString("COMPILER_FLAGS", compileFlags);
			}
			else if (pBXElementDict != null)
			{
				pBXElementDict.Remove("COMPILER_FLAGS");
			}
			if (weak)
			{
				if (pBXElementDict == null)
				{
					pBXElementDict = m_Properties.CreateDict("settings");
				}
				PBXElementArray pBXElementArray = null;
				pBXElementArray = ((!pBXElementDict.Contains("ATTRIBUTES")) ? pBXElementDict.CreateArray("ATTRIBUTES") : pBXElementDict["ATTRIBUTES"].AsArray());
				bool flag = false;
				foreach (PBXElement value in pBXElementArray.values)
				{
					if (value is PBXElementString && value.AsString() == "Weak")
					{
						flag = true;
					}
				}
				if (!flag)
				{
					pBXElementArray.AddString("Weak");
				}
			}
			else if (pBXElementDict != null && pBXElementDict.Contains("ATTRIBUTES"))
			{
				PBXElementArray pBXElementArray2 = pBXElementDict["ATTRIBUTES"].AsArray();
				pBXElementArray2.values.RemoveAll((PBXElement el) => el is PBXElementString && el.AsString() == "Weak");
				if (pBXElementArray2.values.Count == 0)
				{
					pBXElementDict.Remove("ATTRIBUTES");
				}
			}
			if (assetTags.Count > 0)
			{
				if (pBXElementDict == null)
				{
					pBXElementDict = m_Properties.CreateDict("settings");
				}
				PBXElementArray pBXElementArray3 = pBXElementDict.CreateArray("ASSET_TAGS");
				foreach (string assetTag in assetTags)
				{
					pBXElementArray3.AddString(assetTag);
				}
			}
			else if (pBXElementDict != null)
			{
				pBXElementDict.Remove("ASSET_TAGS");
			}
			if (pBXElementDict != null && pBXElementDict.values.Count == 0)
			{
				m_Properties.Remove("settings");
			}
		}

		public override void UpdateVars()
		{
			fileRef = GetPropertyString("fileRef");
			compileFlags = null;
			weak = false;
			assetTags = new List<string>();
			if (!m_Properties.Contains("settings"))
			{
				return;
			}
			PBXElementDict pBXElementDict = m_Properties["settings"].AsDict();
			if (pBXElementDict.Contains("COMPILER_FLAGS"))
			{
				compileFlags = pBXElementDict["COMPILER_FLAGS"].AsString();
			}
			if (pBXElementDict.Contains("ATTRIBUTES"))
			{
				PBXElementArray pBXElementArray = pBXElementDict["ATTRIBUTES"].AsArray();
				foreach (PBXElement value in pBXElementArray.values)
				{
					if (value is PBXElementString && value.AsString() == "Weak")
					{
						weak = true;
					}
				}
			}
			if (!pBXElementDict.Contains("ASSET_TAGS"))
			{
				return;
			}
			PBXElementArray pBXElementArray2 = pBXElementDict["ASSET_TAGS"].AsArray();
			foreach (PBXElement value2 in pBXElementArray2.values)
			{
				assetTags.Add(value2.AsString());
			}
		}
	}
}
