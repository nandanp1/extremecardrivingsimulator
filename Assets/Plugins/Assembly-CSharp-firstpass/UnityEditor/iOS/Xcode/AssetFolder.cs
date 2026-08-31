using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetFolder : AssetCatalogItem
	{
		private List<AssetCatalogItem> m_Items = new List<AssetCatalogItem>();

		private bool m_ProvidesNamespace;

		public bool providesNamespace
		{
			get
			{
				return m_ProvidesNamespace;
			}
			set
			{
				if (m_Items.Count > 0 && value != m_ProvidesNamespace)
				{
					throw new Exception("Asset folder namespace providing status can't be changed after items have been added");
				}
				m_ProvidesNamespace = value;
			}
		}

		internal AssetFolder(string parentPath, string name, string authorId)
			: base(name, authorId)
		{
			if (name != null)
			{
				m_Path = Path.Combine(parentPath, name);
			}
			else
			{
				m_Path = parentPath;
			}
		}

		public AssetFolder OpenFolder(string name)
		{
			AssetCatalogItem child = GetChild(name);
			if (child != null)
			{
				if (child is AssetFolder)
				{
					return child as AssetFolder;
				}
				throw new Exception("The given path is already occupied with an asset");
			}
			AssetFolder assetFolder = new AssetFolder(m_Path, name, authorId);
			m_Items.Add(assetFolder);
			return assetFolder;
		}

		private T GetExistingItemWithType<T>(string name) where T : class
		{
			AssetCatalogItem child = GetChild(name);
			if (child != null)
			{
				if (child is T)
				{
					return child as T;
				}
				throw new Exception("The given path is already occupied with an asset");
			}
			return (T)null;
		}

		public AssetDataSet OpenDataSet(string name)
		{
			AssetDataSet existingItemWithType = GetExistingItemWithType<AssetDataSet>(name);
			if (existingItemWithType != null)
			{
				return existingItemWithType;
			}
			AssetDataSet assetDataSet = new AssetDataSet(m_Path, name, authorId);
			m_Items.Add(assetDataSet);
			return assetDataSet;
		}

		public AssetImageSet OpenImageSet(string name)
		{
			AssetImageSet existingItemWithType = GetExistingItemWithType<AssetImageSet>(name);
			if (existingItemWithType != null)
			{
				return existingItemWithType;
			}
			AssetImageSet assetImageSet = new AssetImageSet(m_Path, name, authorId);
			m_Items.Add(assetImageSet);
			return assetImageSet;
		}

		public AssetImageStack OpenImageStack(string name)
		{
			AssetImageStack existingItemWithType = GetExistingItemWithType<AssetImageStack>(name);
			if (existingItemWithType != null)
			{
				return existingItemWithType;
			}
			AssetImageStack assetImageStack = new AssetImageStack(m_Path, name, authorId);
			m_Items.Add(assetImageStack);
			return assetImageStack;
		}

		public AssetCatalogItem GetChild(string name)
		{
			foreach (AssetCatalogItem item in m_Items)
			{
				if (item.name == name)
				{
					return item;
				}
			}
			return null;
		}

		private void WriteJson()
		{
			if (providesNamespace)
			{
				JsonDocument jsonDocument = new JsonDocument();
				WriteInfoToJson(jsonDocument);
				JsonElementDict jsonElementDict = jsonDocument.root.CreateDict("properties");
				jsonElementDict.SetBoolean("provides-namespace", providesNamespace);
				jsonDocument.WriteToFile(Path.Combine(m_Path, "Contents.json"));
			}
		}

		public override void Write()
		{
			if (Directory.Exists(m_Path))
			{
				Directory.Delete(m_Path, true);
			}
			Directory.CreateDirectory(m_Path);
			WriteJson();
			foreach (AssetCatalogItem item in m_Items)
			{
				item.Write();
			}
		}
	}
}
