using System;

namespace Crosstales.BadWord
{
	[Serializable]
	public class BadWordFile : Source
	{
		public string File = string.Empty;

		public int SkipHeaderLines;

		public int SkipFooterLines;

		public char SplitChar = '#';
	}
}
