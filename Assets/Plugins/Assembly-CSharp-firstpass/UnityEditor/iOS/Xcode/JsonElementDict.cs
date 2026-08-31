using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode
{
	public class JsonElementDict : JsonElement
	{
		private SortedDictionary<string, JsonElement> m_PrivateValue = new SortedDictionary<string, JsonElement>();

		public IDictionary<string, JsonElement> values
		{
			get
			{
				return m_PrivateValue;
			}
		}

		public new JsonElement this[string key]
		{
			get
			{
				if (values.ContainsKey(key))
				{
					return values[key];
				}
				return null;
			}
			set
			{
				values[key] = value;
			}
		}

		public bool Contains(string key)
		{
			return values.ContainsKey(key);
		}

		public void Remove(string key)
		{
			values.Remove(key);
		}

		public void SetInteger(string key, int val)
		{
			values[key] = new JsonElementInteger(val);
		}

		public void SetString(string key, string val)
		{
			values[key] = new JsonElementString(val);
		}

		public void SetBoolean(string key, bool val)
		{
			values[key] = new JsonElementBoolean(val);
		}

		public JsonElementArray CreateArray(string key)
		{
			JsonElementArray jsonElementArray = new JsonElementArray();
			values[key] = jsonElementArray;
			return jsonElementArray;
		}

		public JsonElementDict CreateDict(string key)
		{
			JsonElementDict jsonElementDict = new JsonElementDict();
			values[key] = jsonElementDict;
			return jsonElementDict;
		}
	}
}
