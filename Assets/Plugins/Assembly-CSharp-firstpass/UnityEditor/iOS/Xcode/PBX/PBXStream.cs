namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXStream
	{
		private static bool DontNeedQuotes(string src)
		{
			if (src.Length == 0)
			{
				return false;
			}
			bool flag = false;
			foreach (char c in src)
			{
				if (!char.IsLetterOrDigit(c))
				{
					switch (c)
					{
					case '/':
						flag = true;
						break;
					default:
						return false;
					case '*':
					case '.':
					case '_':
						break;
					}
				}
			}
			if (flag && (src.Contains("//") || src.Contains("/*") || src.Contains("*/")))
			{
				return false;
			}
			return true;
		}

		public static string QuoteStringIfNeeded(string src)
		{
			if (DontNeedQuotes(src))
			{
				return src;
			}
			return "\"" + src.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
		}

		public static string UnquoteString(string src)
		{
			if (!src.StartsWith("\"") || !src.EndsWith("\""))
			{
				return src;
			}
			return src.Substring(1, src.Length - 2).Replace("\\\\", "嚟").Replace("\\\"", "\"")
				.Replace("\\n", "\n")
				.Replace("嚟", "\\");
		}
	}
}
