using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.iOS.Xcode.PBX;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetCatalog
	{
		private AssetFolder m_Root;

		public string path
		{
			get
			{
				return m_Root.path;
			}
		}

		public AssetFolder root
		{
			get
			{
				return m_Root;
			}
		}

		public AssetCatalog(string path, string authorId)
		{
			if (Path.GetExtension(path) != ".xcassets")
			{
				throw new Exception("Asset catalogs must have xcassets extension");
			}
			m_Root = new AssetFolder(path, null, authorId);
		}

		private AssetFolder OpenFolderForResource(string relativePath)
		{
			List<string> list = Utils.SplitPath(relativePath).ToList();
			list.RemoveAt(list.Count - 1);
			AssetFolder assetFolder = root;
			foreach (string item in list)
			{
				assetFolder = assetFolder.OpenFolder(item);
			}
			return assetFolder;
		}

		public AssetDataSet OpenDataSet(string relativePath)
		{
			AssetFolder assetFolder = OpenFolderForResource(relativePath);
			return assetFolder.OpenDataSet(Path.GetFileName(relativePath));
		}

		public AssetImageSet OpenImageSet(string relativePath)
		{
			AssetFolder assetFolder = OpenFolderForResource(relativePath);
			return assetFolder.OpenImageSet(Path.GetFileName(relativePath));
		}

		public AssetImageStack OpenImageStack(string relativePath)
		{
			AssetFolder assetFolder = OpenFolderForResource(relativePath);
			return assetFolder.OpenImageStack(Path.GetFileName(relativePath));
		}

		public AssetFolder OpenFolder(string relativePath)
		{
			if (relativePath == null)
			{
				return root;
			}
			string[] array = Utils.SplitPath(relativePath);
			if (array.Length == 0)
			{
				return root;
			}
			AssetFolder assetFolder = root;
			string[] array2 = array;
			foreach (string name in array2)
			{
				assetFolder = assetFolder.OpenFolder(name);
			}
			return assetFolder;
		}

		public AssetFolder OpenNamespacedFolder(string relativeBasePath, string namespacePath)
		{
			AssetFolder assetFolder = OpenFolder(relativeBasePath);
			string[] array = Utils.SplitPath(namespacePath);
			string[] array2 = array;
			foreach (string name in array2)
			{
				assetFolder = assetFolder.OpenFolder(name);
				assetFolder.providesNamespace = true;
			}
			return assetFolder;
		}

		public void Write()
		{
			m_Root.Write();
		}
	}
}
