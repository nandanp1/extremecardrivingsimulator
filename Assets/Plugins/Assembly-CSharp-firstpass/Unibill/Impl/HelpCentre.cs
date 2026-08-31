using System.Collections.Generic;

namespace Unibill.Impl
{
	public class HelpCentre
	{
		private Dictionary<string, object> helpMap;

		public HelpCentre(string json)
		{
			helpMap = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
		}

		public string getMessage(UnibillError error)
		{
			string arg = string.Format("http://www.outlinegames.com/unibillerrors#{0}", error);
			return string.Format("{0}.\nSee {1}", helpMap[error.ToString()], arg);
		}
	}
}
