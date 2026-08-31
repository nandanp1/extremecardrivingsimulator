using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode
{
	public class JsonElementArray : JsonElement
	{
		public List<JsonElement> values = new List<JsonElement>();

		public void AddString(string val)
		{
			values.Add(new JsonElementString(val));
		}

		public void AddInteger(int val)
		{
			values.Add(new JsonElementInteger(val));
		}

		public void AddBoolean(bool val)
		{
			values.Add(new JsonElementBoolean(val));
		}

		public JsonElementArray AddArray()
		{
			JsonElementArray jsonElementArray = new JsonElementArray();
			values.Add(jsonElementArray);
			return jsonElementArray;
		}

		public JsonElementDict AddDict()
		{
			JsonElementDict jsonElementDict = new JsonElementDict();
			values.Add(jsonElementDict);
			return jsonElementDict;
		}
	}
}
