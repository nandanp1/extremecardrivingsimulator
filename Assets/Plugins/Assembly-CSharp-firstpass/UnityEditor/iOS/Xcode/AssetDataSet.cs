using System;
using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetDataSet : AssetCatalogItemWithVariants
	{
		private class DataSetVariant : VariantData
		{
			public string id;

			public DataSetVariant(DeviceRequirement requirement, string path, string id)
				: base(requirement, path)
			{
				this.id = id;
			}
		}

		internal AssetDataSet(string parentPath, string name, string authorId)
			: base(name, authorId)
		{
			m_Path = Path.Combine(parentPath, name + ".dataset");
		}

		public void AddVariant(DeviceRequirement requirement, string path, string typeIdentifier)
		{
			foreach (DataSetVariant variant in m_Variants)
			{
				if (variant.id != null && typeIdentifier != null && variant.id == typeIdentifier)
				{
					throw new Exception("Two items within the same dataset must not have the same id");
				}
			}
			AddVariant(new DataSetVariant(requirement, path, typeIdentifier));
		}

		public override void Write()
		{
			Directory.CreateDirectory(m_Path);
			JsonDocument jsonDocument = new JsonDocument();
			JsonElementDict info = WriteInfoToJson(jsonDocument);
			WriteODRTagsToJson(info);
			JsonElementArray jsonElementArray = jsonDocument.root.CreateArray("data");
			foreach (DataSetVariant variant in m_Variants)
			{
				string fileName = Path.GetFileName(variant.path);
				File.Copy(variant.path, Path.Combine(m_Path, fileName));
				JsonElementDict jsonElementDict = jsonElementArray.AddDict();
				jsonElementDict.SetString("filename", fileName);
				WriteRequirementsToJson(jsonElementDict, variant.requirement);
				if (variant.id != null)
				{
					jsonElementDict.SetString("universal-type-identifier", variant.id);
				}
			}
			jsonDocument.WriteToFile(Path.Combine(m_Path, "Contents.json"));
		}
	}
}
