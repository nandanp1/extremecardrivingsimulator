using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXProjectSection : KnownSectionBase<PBXProjectObjectData>
	{
		public PBXProjectObjectData project
		{
			get
			{
				using (IEnumerator<KeyValuePair<string, PBXProjectObjectData>> enumerator = GetEntries().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current.Value;
					}
				}
				return null;
			}
		}

		public PBXProjectSection()
			: base("PBXProject")
		{
		}
	}
}
