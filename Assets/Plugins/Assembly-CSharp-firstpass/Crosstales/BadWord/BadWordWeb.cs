using System;

namespace Crosstales.BadWord
{
	[Serializable]
	public class BadWordWeb : Source
	{
		public string URL = string.Empty;

		public int SkipHeaderLines;

		public int SkipFooterLines;

		public char SplitChar = '#';
	}
}
