using System;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class Utils
	{
		public static string FixSlashesInPath(string path)
		{
			if (path == null)
			{
				return null;
			}
			return path.Replace('\\', '/');
		}

		public static void CombinePaths(string path1, PBXSourceTree tree1, string path2, PBXSourceTree tree2, out string resPath, out PBXSourceTree resTree)
		{
			if (tree2 == PBXSourceTree.Group)
			{
				resPath = CombinePaths(path1, path2);
				resTree = tree1;
			}
			else
			{
				resPath = path2;
				resTree = tree2;
			}
		}

		public static string CombinePaths(string path1, string path2)
		{
			if (path2.StartsWith("/"))
			{
				return path2;
			}
			if (path1.EndsWith("/"))
			{
				return path1 + path2;
			}
			if (path1 == string.Empty)
			{
				return path2;
			}
			if (path2 == string.Empty)
			{
				return path1;
			}
			return path1 + "/" + path2;
		}

		public static string GetDirectoryFromPath(string path)
		{
			int num = path.LastIndexOf('/');
			if (num == -1)
			{
				return string.Empty;
			}
			return path.Substring(0, num);
		}

		public static string GetFilenameFromPath(string path)
		{
			int num = path.LastIndexOf('/');
			if (num == -1)
			{
				return path;
			}
			return path.Substring(num + 1);
		}

		public static string[] SplitPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new string[0];
			}
			return path.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
