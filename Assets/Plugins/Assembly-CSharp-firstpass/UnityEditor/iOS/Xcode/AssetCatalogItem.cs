using System;
using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode
{
	internal abstract class AssetCatalogItem
	{
		public readonly string name;

		public readonly string authorId;

		protected Dictionary<string, string> m_Properties = new Dictionary<string, string>();

		protected string m_Path;

		public string path
		{
			get
			{
				return m_Path;
			}
		}

		public AssetCatalogItem(string name, string authorId)
		{
			if (name != null && name.Contains("/"))
			{
				throw new Exception("Asset catalog item must not have slashes in name");
			}
			this.name = name;
			this.authorId = authorId;
		}

		protected JsonElementDict WriteInfoToJson(JsonDocument doc)
		{
			JsonElementDict jsonElementDict = doc.root.CreateDict("info");
			jsonElementDict.SetInteger("version", 1);
			jsonElementDict.SetString("author", authorId);
			return jsonElementDict;
		}

		public abstract void Write();
	}
}
