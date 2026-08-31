using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class XCBuildConfigurationData : PBXObjectData
	{
		protected SortedDictionary<string, BuildConfigEntryData> entries = new SortedDictionary<string, BuildConfigEntryData>();

		public string name
		{
			get
			{
				return GetPropertyString("name");
			}
		}

		private static string EscapeWithQuotesIfNeeded(string name, string value)
		{
			if (name != "LIBRARY_SEARCH_PATHS")
			{
				return value;
			}
			if (!value.Contains(" "))
			{
				return value;
			}
			if (value.First() == '"' && value.Last() == '"')
			{
				return value;
			}
			return "\"" + value + "\"";
		}

		public void SetProperty(string name, string value)
		{
			entries[name] = BuildConfigEntryData.FromNameValue(name, EscapeWithQuotesIfNeeded(name, value));
		}

		public void AddProperty(string name, string value)
		{
			if (entries.ContainsKey(name))
			{
				entries[name].AddValue(EscapeWithQuotesIfNeeded(name, value));
			}
			else
			{
				SetProperty(name, value);
			}
		}

		public void RemoveProperty(string name)
		{
			if (entries.ContainsKey(name))
			{
				entries.Remove(name);
			}
		}

		public void RemovePropertyValue(string name, string value)
		{
			if (entries.ContainsKey(name))
			{
				entries[name].RemoveValue(EscapeWithQuotesIfNeeded(name, value));
			}
		}

		public static XCBuildConfigurationData Create(string name)
		{
			XCBuildConfigurationData xCBuildConfigurationData = new XCBuildConfigurationData();
			xCBuildConfigurationData.guid = PBXGUID.Generate();
			xCBuildConfigurationData.SetPropertyString("isa", "XCBuildConfiguration");
			xCBuildConfigurationData.SetPropertyString("name", name);
			return xCBuildConfigurationData;
		}

		public override void UpdateProps()
		{
			PBXElementDict pBXElementDict = m_Properties.CreateDict("buildSettings");
			foreach (KeyValuePair<string, BuildConfigEntryData> entry in entries)
			{
				if (entry.Value.val.Count == 0)
				{
					continue;
				}
				if (entry.Value.val.Count == 1)
				{
					pBXElementDict.SetString(entry.Key, entry.Value.val[0]);
					continue;
				}
				PBXElementArray pBXElementArray = pBXElementDict.CreateArray(entry.Key);
				foreach (string item in entry.Value.val)
				{
					pBXElementArray.AddString(item);
				}
			}
		}

		public override void UpdateVars()
		{
			entries = new SortedDictionary<string, BuildConfigEntryData>();
			if (!m_Properties.Contains("buildSettings"))
			{
				return;
			}
			PBXElementDict pBXElementDict = m_Properties["buildSettings"].AsDict();
			foreach (string key in pBXElementDict.values.Keys)
			{
				PBXElement pBXElement = pBXElementDict[key];
				if (pBXElement is PBXElementString)
				{
					if (entries.ContainsKey(key))
					{
						entries[key].val.Add(pBXElement.AsString());
					}
					else
					{
						entries.Add(key, BuildConfigEntryData.FromNameValue(key, pBXElement.AsString()));
					}
				}
				else
				{
					if (!(pBXElement is PBXElementArray))
					{
						continue;
					}
					foreach (PBXElement value in pBXElement.AsArray().values)
					{
						if (value is PBXElementString)
						{
							if (entries.ContainsKey(key))
							{
								entries[key].val.Add(value.AsString());
							}
							else
							{
								entries.Add(key, BuildConfigEntryData.FromNameValue(key, value.AsString()));
							}
						}
					}
				}
			}
		}
	}
}
