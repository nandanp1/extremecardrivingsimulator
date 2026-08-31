using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal abstract class AssetCatalogItemWithVariants : AssetCatalogItem
	{
		protected class VariantData
		{
			public DeviceRequirement requirement;

			public string path;

			public VariantData(DeviceRequirement requirement, string path)
			{
				this.requirement = requirement;
				this.path = path;
			}
		}

		protected List<VariantData> m_Variants = new List<VariantData>();

		protected List<string> m_ODRTags = new List<string>();

		protected AssetCatalogItemWithVariants(string name, string authorId)
			: base(name, authorId)
		{
		}

		public bool HasVariant(DeviceRequirement requirement)
		{
			foreach (VariantData variant in m_Variants)
			{
				if (variant.requirement.values == requirement.values)
				{
					return true;
				}
			}
			return false;
		}

		public void AddOnDemandResourceTag(string tag)
		{
			if (!m_ODRTags.Contains(tag))
			{
				m_ODRTags.Add(tag);
			}
		}

		protected void AddVariant(VariantData newItem)
		{
			foreach (VariantData variant in m_Variants)
			{
				if (variant.requirement.values == newItem.requirement.values)
				{
					throw new Exception("The given requirement has been already added");
				}
				if (Path.GetFileName(variant.path) == Path.GetFileName(base.path))
				{
					throw new Exception("Two items within the same set must not have the same file name");
				}
			}
			if (Path.GetFileName(newItem.path) == "Contents.json")
			{
				throw new Exception("The file name must not be equal to Contents.json");
			}
			m_Variants.Add(newItem);
		}

		protected void WriteODRTagsToJson(JsonElementDict info)
		{
			if (m_ODRTags.Count <= 0)
			{
				return;
			}
			JsonElementArray jsonElementArray = info.CreateArray("on-demand-resource-tags");
			foreach (string oDRTag in m_ODRTags)
			{
				jsonElementArray.AddString(oDRTag);
			}
		}

		protected void WriteRequirementsToJson(JsonElementDict item, DeviceRequirement req)
		{
			foreach (KeyValuePair<string, string> value in req.values)
			{
				if (value.Value != null && value.Value != string.Empty)
				{
					item.SetString(value.Key, value.Value);
				}
			}
		}
	}
}
