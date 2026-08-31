using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class BuildConfigEntryData
	{
		public string name;

		public List<string> val = new List<string>();

		public static string ExtractValue(string src)
		{
			return PBXStream.UnquoteString(src.Trim().TrimEnd(','));
		}

		public void AddValue(string value)
		{
			if (!val.Contains(value))
			{
				val.Add(value);
			}
		}

		public void RemoveValue(string value)
		{
			val.RemoveAll((string v) => v == value);
		}

		public static BuildConfigEntryData FromNameValue(string name, string value)
		{
			BuildConfigEntryData buildConfigEntryData = new BuildConfigEntryData();
			buildConfigEntryData.name = name;
			buildConfigEntryData.AddValue(value);
			return buildConfigEntryData;
		}
	}
}
