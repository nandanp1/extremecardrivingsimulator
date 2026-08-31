using System;

namespace Crosstales.BadWord
{
	[Serializable]
	public class DomainWeb : Source
	{
		public string URL = string.Empty;

		public int SkipHeaderLines;

		public int SkipFooterLines;

		public char SplitChar = '#';
	}
}
