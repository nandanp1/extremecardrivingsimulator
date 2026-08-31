using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXProjectObjectData : PBXObjectData
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[5] { "buildConfigurationList/*", "mainGroup/*", "projectReferences/*/ProductGroup/*", "projectReferences/*/ProjectRef/*", "targets/*" });

		public List<ProjectReference> projectReferences = new List<ProjectReference>();

		public List<string> targets = new List<string>();

		public List<string> knownAssetTags = new List<string>();

		public string buildConfigList;

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public string mainGroup
		{
			get
			{
				return GetPropertyString("mainGroup");
			}
		}

		public void AddReference(string productGroup, string projectRef)
		{
			projectReferences.Add(ProjectReference.Create(productGroup, projectRef));
		}

		public override void UpdateProps()
		{
			m_Properties.values.Remove("projectReferences");
			if (projectReferences.Count > 0)
			{
				PBXElementArray pBXElementArray = m_Properties.CreateArray("projectReferences");
				foreach (ProjectReference projectReference in projectReferences)
				{
					PBXElementDict pBXElementDict = pBXElementArray.AddDict();
					pBXElementDict.SetString("ProductGroup", projectReference.group);
					pBXElementDict.SetString("ProjectRef", projectReference.projectRef);
				}
			}
			SetPropertyList("targets", targets);
			SetPropertyString("buildConfigurationList", buildConfigList);
			if (knownAssetTags.Count <= 0)
			{
				return;
			}
			PBXElementDict pBXElementDict2 = ((!m_Properties.Contains("attributes")) ? m_Properties.CreateDict("attributes") : m_Properties["attributes"].AsDict());
			PBXElementArray pBXElementArray2 = pBXElementDict2.CreateArray("knownAssetTags");
			foreach (string knownAssetTag in knownAssetTags)
			{
				pBXElementArray2.AddString(knownAssetTag);
			}
		}

		public override void UpdateVars()
		{
			projectReferences = new List<ProjectReference>();
			if (m_Properties.Contains("projectReferences"))
			{
				PBXElementArray pBXElementArray = m_Properties["projectReferences"].AsArray();
				foreach (PBXElement value in pBXElementArray.values)
				{
					PBXElementDict pBXElementDict = value.AsDict();
					if (pBXElementDict.Contains("ProductGroup") && pBXElementDict.Contains("ProjectRef"))
					{
						string text = pBXElementDict["ProductGroup"].AsString();
						string projectRef = pBXElementDict["ProjectRef"].AsString();
						projectReferences.Add(ProjectReference.Create(text, projectRef));
					}
				}
			}
			targets = GetPropertyList("targets");
			buildConfigList = GetPropertyString("buildConfigurationList");
			knownAssetTags = new List<string>();
			if (!m_Properties.Contains("attributes"))
			{
				return;
			}
			PBXElementDict pBXElementDict2 = m_Properties["attributes"].AsDict();
			if (!pBXElementDict2.Contains("knownAssetTags"))
			{
				return;
			}
			PBXElementArray pBXElementArray2 = pBXElementDict2["knownAssetTags"].AsArray();
			foreach (PBXElement value2 in pBXElementArray2.values)
			{
				knownAssetTags.Add(value2.AsString());
			}
		}
	}
}
