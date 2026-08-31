using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetImageStackLayer : AssetCatalogItem
	{
		private AssetImageSet m_Imageset;

		private string m_ReferencedName;

		internal AssetImageStackLayer(string assetCatalogPath, string name, string authorId)
			: base(name, authorId)
		{
			m_Path = Path.Combine(assetCatalogPath, name + ".imagestacklayer");
			m_Imageset = new AssetImageSet(m_Path, "Content", authorId);
		}

		public void SetReference(string name)
		{
			m_Imageset = null;
			m_ReferencedName = name;
		}

		public string ReferencedName()
		{
			return m_ReferencedName;
		}

		public AssetImageSet GetImageSet()
		{
			return m_Imageset;
		}

		public override void Write()
		{
			Directory.CreateDirectory(m_Path);
			JsonDocument jsonDocument = new JsonDocument();
			WriteInfoToJson(jsonDocument);
			if (m_ReferencedName != null)
			{
				JsonElementDict jsonElementDict = jsonDocument.root.CreateDict("properties");
				JsonElementDict jsonElementDict2 = jsonElementDict.CreateDict("content-reference");
				jsonElementDict2.SetString("type", "image-set");
				jsonElementDict2.SetString("name", m_ReferencedName);
				jsonElementDict2.SetString("matching-style", "fully-qualified-name");
			}
			if (m_Imageset != null)
			{
				m_Imageset.Write();
			}
			jsonDocument.WriteToFile(Path.Combine(m_Path, "Contents.json"));
		}
	}
}
