namespace UnityEditor.iOS.Xcode
{
	public class JsonElementString : JsonElement
	{
		public string value;

		public JsonElementString(string v)
		{
			value = v;
		}
	}
}
