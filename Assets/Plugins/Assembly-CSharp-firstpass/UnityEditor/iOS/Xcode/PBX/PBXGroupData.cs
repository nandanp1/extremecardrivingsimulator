using System;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class PBXGroupData : PBXObjectData
	{
		public GUIDList children;

		public PBXSourceTree tree;

		public string name;

		public string path;

		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "children/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXGroupData Create(string name, string path, PBXSourceTree tree)
		{
			if (name.Contains("/"))
			{
				throw new Exception("Group name must not contain '/'");
			}
			PBXGroupData pBXGroupData = new PBXGroupData();
			pBXGroupData.guid = PBXGUID.Generate();
			pBXGroupData.SetPropertyString("isa", "PBXGroup");
			pBXGroupData.name = name;
			pBXGroupData.path = path;
			pBXGroupData.tree = PBXSourceTree.Group;
			pBXGroupData.children = new GUIDList();
			return pBXGroupData;
		}

		public static PBXGroupData CreateRelative(string name)
		{
			return Create(name, name, PBXSourceTree.Group);
		}

		public override void UpdateProps()
		{
			SetPropertyList("children", children);
			if (name == path)
			{
				SetPropertyString("name", null);
			}
			else
			{
				SetPropertyString("name", name);
			}
			if (path == string.Empty)
			{
				SetPropertyString("path", null);
			}
			else
			{
				SetPropertyString("path", path);
			}
			SetPropertyString("sourceTree", FileTypeUtils.SourceTreeDesc(tree));
		}

		public override void UpdateVars()
		{
			children = GetPropertyList("children");
			path = GetPropertyString("path");
			name = GetPropertyString("name");
			if (name == null)
			{
				name = path;
			}
			if (path == null)
			{
				path = string.Empty;
			}
			tree = FileTypeUtils.ParseSourceTree(GetPropertyString("sourceTree"));
		}
	}
}
