using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetImageStack : AssetCatalogItem
	{
		private List<AssetImageStackLayer> m_Layers = new List<AssetImageStackLayer>();

		internal AssetImageStack(string assetCatalogPath, string name, string authorId)
			: base(name, authorId)
		{
			m_Path = Path.Combine(assetCatalogPath, name + ".imagestack");
		}

		public AssetImageStackLayer AddLayer(string name)
		{
			foreach (AssetImageStackLayer layer in m_Layers)
			{
				if (layer.name == name)
				{
					throw new Exception("A layer with given name already exists");
				}
			}
			AssetImageStackLayer assetImageStackLayer = new AssetImageStackLayer(m_Path, name, authorId);
			m_Layers.Add(assetImageStackLayer);
			return assetImageStackLayer;
		}

		public override void Write()
		{
			Directory.CreateDirectory(m_Path);
			JsonDocument jsonDocument = new JsonDocument();
			WriteInfoToJson(jsonDocument);
			JsonElementArray jsonElementArray = jsonDocument.root.CreateArray("layers");
			foreach (AssetImageStackLayer layer in m_Layers)
			{
				layer.Write();
				JsonElementDict jsonElementDict = jsonElementArray.AddDict();
				jsonElementDict.SetString("filename", Path.GetFileName(layer.path));
			}
			jsonDocument.WriteToFile(Path.Combine(m_Path, "Contents.json"));
		}
	}
}
