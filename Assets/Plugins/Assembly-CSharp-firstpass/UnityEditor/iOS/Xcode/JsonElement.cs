namespace UnityEditor.iOS.Xcode
{
	public class JsonElement
	{
		public JsonElement this[string key]
		{
			get
			{
				return AsDict()[key];
			}
			set
			{
				AsDict()[key] = value;
			}
		}

		protected JsonElement()
		{
		}

		public string AsString()
		{
			return ((JsonElementString)this).value;
		}

		public int AsInteger()
		{
			return ((JsonElementInteger)this).value;
		}

		public bool AsBoolean()
		{
			return ((JsonElementBoolean)this).value;
		}

		public JsonElementArray AsArray()
		{
			return (JsonElementArray)this;
		}

		public JsonElementDict AsDict()
		{
			return (JsonElementDict)this;
		}
	}
}
