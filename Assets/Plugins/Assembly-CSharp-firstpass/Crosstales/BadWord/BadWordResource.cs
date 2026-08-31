using System;
using UnityEngine;

namespace Crosstales.BadWord
{
	[Serializable]
	public class BadWordResource : Source
	{
		public TextAsset Resource;

		public int SkipHeaderLines;

		public int SkipFooterLines;

		public char SplitChar = '#';
	}
}
