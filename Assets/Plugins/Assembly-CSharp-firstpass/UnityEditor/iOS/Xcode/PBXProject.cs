using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.iOS.Xcode.PBX;

namespace UnityEditor.iOS.Xcode
{
	public class PBXProject
	{
		private PBXProjectData m_Data = new PBXProjectData();

		private KnownSectionBase<PBXContainerItemProxyData> containerItems
		{
			get
			{
				return m_Data.containerItems;
			}
		}

		private KnownSectionBase<PBXReferenceProxyData> references
		{
			get
			{
				return m_Data.references;
			}
		}

		private KnownSectionBase<PBXSourcesBuildPhaseData> sources
		{
			get
			{
				return m_Data.sources;
			}
		}

		private KnownSectionBase<PBXFrameworksBuildPhaseData> frameworks
		{
			get
			{
				return m_Data.frameworks;
			}
		}

		private KnownSectionBase<PBXResourcesBuildPhaseData> resources
		{
			get
			{
				return m_Data.resources;
			}
		}

		private KnownSectionBase<PBXCopyFilesBuildPhaseData> copyFiles
		{
			get
			{
				return m_Data.copyFiles;
			}
		}

		private KnownSectionBase<PBXShellScriptBuildPhaseData> shellScripts
		{
			get
			{
				return m_Data.shellScripts;
			}
		}

		private KnownSectionBase<PBXNativeTargetData> nativeTargets
		{
			get
			{
				return m_Data.nativeTargets;
			}
		}

		private KnownSectionBase<PBXTargetDependencyData> targetDependencies
		{
			get
			{
				return m_Data.targetDependencies;
			}
		}

		private KnownSectionBase<PBXVariantGroupData> variantGroups
		{
			get
			{
				return m_Data.variantGroups;
			}
		}

		private KnownSectionBase<XCBuildConfigurationData> buildConfigs
		{
			get
			{
				return m_Data.buildConfigs;
			}
		}

		private KnownSectionBase<XCConfigurationListData> configs
		{
			get
			{
				return m_Data.configs;
			}
		}

		private PBXProjectSection project
		{
			get
			{
				return m_Data.project;
			}
		}

		private PBXBuildFileData BuildFilesGet(string guid)
		{
			return m_Data.BuildFilesGet(guid);
		}

		private void BuildFilesAdd(string targetGuid, PBXBuildFileData buildFile)
		{
			m_Data.BuildFilesAdd(targetGuid, buildFile);
		}

		private void BuildFilesRemove(string targetGuid, string fileGuid)
		{
			m_Data.BuildFilesRemove(targetGuid, fileGuid);
		}

		private PBXBuildFileData BuildFilesGetForSourceFile(string targetGuid, string fileGuid)
		{
			return m_Data.BuildFilesGetForSourceFile(targetGuid, fileGuid);
		}

		private IEnumerable<PBXBuildFileData> BuildFilesGetAll()
		{
			return m_Data.BuildFilesGetAll();
		}

		private void FileRefsAdd(string realPath, string projectPath, PBXGroupData parent, PBXFileReferenceData fileRef)
		{
			m_Data.FileRefsAdd(realPath, projectPath, parent, fileRef);
		}

		private PBXFileReferenceData FileRefsGet(string guid)
		{
			return m_Data.FileRefsGet(guid);
		}

		private PBXFileReferenceData FileRefsGetByRealPath(string path, PBXSourceTree sourceTree)
		{
			return m_Data.FileRefsGetByRealPath(path, sourceTree);
		}

		private PBXFileReferenceData FileRefsGetByProjectPath(string path)
		{
			return m_Data.FileRefsGetByProjectPath(path);
		}

		private void FileRefsRemove(string guid)
		{
			m_Data.FileRefsRemove(guid);
		}

		private PBXGroupData GroupsGet(string guid)
		{
			return m_Data.GroupsGet(guid);
		}

		private PBXGroupData GroupsGetByChild(string childGuid)
		{
			return m_Data.GroupsGetByChild(childGuid);
		}

		private PBXGroupData GroupsGetMainGroup()
		{
			return m_Data.GroupsGetMainGroup();
		}

		private PBXGroupData GroupsGetByProjectPath(string sourceGroup)
		{
			return m_Data.GroupsGetByProjectPath(sourceGroup);
		}

		private void GroupsAdd(string projectPath, PBXGroupData parent, PBXGroupData gr)
		{
			m_Data.GroupsAdd(projectPath, parent, gr);
		}

		private void GroupsAddDuplicate(PBXGroupData gr)
		{
			m_Data.GroupsAddDuplicate(gr);
		}

		private void GroupsRemove(string guid)
		{
			m_Data.GroupsRemove(guid);
		}

		private FileGUIDListBase BuildSectionAny(PBXNativeTargetData target, string path, bool isFolderRef)
		{
			return m_Data.BuildSectionAny(target, path, isFolderRef);
		}

		public static string GetPBXProjectPath(string buildPath)
		{
			return Utils.CombinePaths(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
		}

		public static string GetUnityTargetName()
		{
			return "Unity-iPhone";
		}

		public static string GetUnityTestTargetName()
		{
			return "Unity-iPhone Tests";
		}

		internal string ProjectGuid()
		{
			return project.project.guid;
		}

		public string TargetGuidByName(string name)
		{
			foreach (KeyValuePair<string, PBXNativeTargetData> entry in nativeTargets.GetEntries())
			{
				if (entry.Value.name == name)
				{
					return entry.Key;
				}
			}
			return null;
		}

		public static bool IsKnownExtension(string ext)
		{
			return FileTypeUtils.IsKnownExtension(ext);
		}

		public static bool IsBuildable(string ext)
		{
			return FileTypeUtils.IsBuildableFile(ext);
		}

		private string AddFileImpl(string path, string projectPath, PBXSourceTree tree, bool isFolderReference)
		{
			path = Utils.FixSlashesInPath(path);
			projectPath = Utils.FixSlashesInPath(projectPath);
			if (!isFolderReference && Path.GetExtension(path) != Path.GetExtension(projectPath))
			{
				throw new Exception("Project and real path extensions do not match");
			}
			string text = FindFileGuidByProjectPath(projectPath);
			if (text == null)
			{
				text = FindFileGuidByRealPath(path);
			}
			if (text == null)
			{
				PBXFileReferenceData pBXFileReferenceData = ((!isFolderReference) ? PBXFileReferenceData.CreateFromFile(path, Utils.GetFilenameFromPath(projectPath), tree) : PBXFileReferenceData.CreateFromFolderReference(path, Utils.GetFilenameFromPath(projectPath), tree));
				PBXGroupData pBXGroupData = CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath));
				pBXGroupData.children.AddGUID(pBXFileReferenceData.guid);
				FileRefsAdd(path, projectPath, pBXGroupData, pBXFileReferenceData);
				text = pBXFileReferenceData.guid;
			}
			return text;
		}

		public string AddFile(string path, string projectPath)
		{
			return AddFileImpl(path, projectPath, PBXSourceTree.Source, false);
		}

		public string AddFile(string path, string projectPath, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			return AddFileImpl(path, projectPath, sourceTree, false);
		}

		public string AddFolderReference(string path, string projectPath)
		{
			return AddFileImpl(path, projectPath, PBXSourceTree.Source, true);
		}

		public string AddFolderReference(string path, string projectPath, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			return AddFileImpl(path, projectPath, sourceTree, true);
		}

		private void AddBuildFileImpl(string targetGuid, string fileGuid, bool weak, string compileFlags)
		{
			PBXNativeTargetData target = nativeTargets[targetGuid];
			PBXFileReferenceData pBXFileReferenceData = FileRefsGet(fileGuid);
			string extension = Path.GetExtension(pBXFileReferenceData.path);
			if (FileTypeUtils.IsBuildable(extension, pBXFileReferenceData.isFolderReference) && BuildFilesGetForSourceFile(targetGuid, fileGuid) == null)
			{
				PBXBuildFileData pBXBuildFileData = PBXBuildFileData.CreateFromFile(fileGuid, weak, compileFlags);
				BuildFilesAdd(targetGuid, pBXBuildFileData);
				BuildSectionAny(target, extension, pBXFileReferenceData.isFolderReference).files.AddGUID(pBXBuildFileData.guid);
			}
		}

		public void AddFileToBuild(string targetGuid, string fileGuid)
		{
			AddBuildFileImpl(targetGuid, fileGuid, false, null);
		}

		public void AddFileToBuildWithFlags(string targetGuid, string fileGuid, string compileFlags)
		{
			AddBuildFileImpl(targetGuid, fileGuid, false, compileFlags);
		}

		public List<string> GetCompileFlagsForFile(string targetGuid, string fileGuid)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData == null)
			{
				return null;
			}
			if (pBXBuildFileData.compileFlags == null)
			{
				return new List<string>();
			}
			List<string> list = new List<string>();
			list.Add(pBXBuildFileData.compileFlags);
			return list;
		}

		public void SetCompileFlagsForFile(string targetGuid, string fileGuid, List<string> compileFlags)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData != null)
			{
				if (compileFlags == null)
				{
					pBXBuildFileData.compileFlags = null;
				}
				else
				{
					pBXBuildFileData.compileFlags = string.Join(" ", compileFlags.ToArray());
				}
			}
		}

		public void AddAssetTagForFile(string targetGuid, string fileGuid, string tag)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData != null)
			{
				if (!pBXBuildFileData.assetTags.Contains(tag))
				{
					pBXBuildFileData.assetTags.Add(tag);
				}
				if (!project.project.knownAssetTags.Contains(tag))
				{
					project.project.knownAssetTags.Add(tag);
				}
			}
		}

		public void RemoveAssetTagForFile(string targetGuid, string fileGuid, string tag)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData == null)
			{
				return;
			}
			pBXBuildFileData.assetTags.Remove(tag);
			foreach (PBXBuildFileData item in BuildFilesGetAll())
			{
				if (item.assetTags.Contains(tag))
				{
					return;
				}
			}
			project.project.knownAssetTags.Remove(tag);
		}

		public void AddAssetTagToDefaultInstall(string targetGuid, string tag)
		{
			if (project.project.knownAssetTags.Contains(tag))
			{
				AddBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", tag);
			}
		}

		public void RemoveAssetTagFromDefaultInstall(string targetGuid, string tag)
		{
			UpdateBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", null, new string[1] { tag });
		}

		public void RemoveAssetTag(string tag)
		{
			foreach (PBXBuildFileData item in BuildFilesGetAll())
			{
				item.assetTags.Remove(tag);
			}
			foreach (string guid in nativeTargets.GetGuids())
			{
				RemoveAssetTagFromDefaultInstall(guid, tag);
			}
			project.project.knownAssetTags.Remove(tag);
		}

		public bool ContainsFileByRealPath(string path)
		{
			return FindFileGuidByRealPath(path) != null;
		}

		public bool ContainsFileByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			return FindFileGuidByRealPath(path, sourceTree) != null;
		}

		public bool ContainsFileByProjectPath(string path)
		{
			return FindFileGuidByProjectPath(path) != null;
		}

		public bool HasFramework(string framework)
		{
			return ContainsFileByRealPath("System/Library/Frameworks/" + framework);
		}

		public void AddFrameworkToProject(string targetGuid, string framework, bool weak)
		{
			string fileGuid = AddFile("System/Library/Frameworks/" + framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
			AddBuildFileImpl(targetGuid, fileGuid, weak, null);
		}

		public void RemoveFrameworkFromProject(string targetGuid, string framework)
		{
			string text = FindFileGuidByRealPath("System/Library/Frameworks/" + framework);
			if (text != null)
			{
				RemoveFile(text);
			}
		}

		public string FindFileGuidByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			path = Utils.FixSlashesInPath(path);
			PBXFileReferenceData pBXFileReferenceData = FileRefsGetByRealPath(path, sourceTree);
			if (pBXFileReferenceData != null)
			{
				return pBXFileReferenceData.guid;
			}
			return null;
		}

		public string FindFileGuidByRealPath(string path)
		{
			path = Utils.FixSlashesInPath(path);
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				string text = FindFileGuidByRealPath(path, item);
				if (text != null)
				{
					return text;
				}
			}
			return null;
		}

		public string FindFileGuidByProjectPath(string path)
		{
			path = Utils.FixSlashesInPath(path);
			PBXFileReferenceData pBXFileReferenceData = FileRefsGetByProjectPath(path);
			if (pBXFileReferenceData != null)
			{
				return pBXFileReferenceData.guid;
			}
			return null;
		}

		public void RemoveFileFromBuild(string targetGuid, string fileGuid)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData == null)
			{
				return;
			}
			BuildFilesRemove(targetGuid, fileGuid);
			string guid = pBXBuildFileData.guid;
			if (guid == null)
			{
				return;
			}
			foreach (KeyValuePair<string, PBXSourcesBuildPhaseData> entry in sources.GetEntries())
			{
				entry.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXResourcesBuildPhaseData> entry2 in resources.GetEntries())
			{
				entry2.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXCopyFilesBuildPhaseData> entry3 in copyFiles.GetEntries())
			{
				entry3.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXFrameworksBuildPhaseData> entry4 in frameworks.GetEntries())
			{
				entry4.Value.files.RemoveGUID(guid);
			}
		}

		public void RemoveFile(string fileGuid)
		{
			if (fileGuid == null)
			{
				return;
			}
			PBXGroupData pBXGroupData = GroupsGetByChild(fileGuid);
			if (pBXGroupData != null)
			{
				pBXGroupData.children.RemoveGUID(fileGuid);
			}
			RemoveGroupIfEmpty(pBXGroupData);
			foreach (KeyValuePair<string, PBXNativeTargetData> entry in nativeTargets.GetEntries())
			{
				RemoveFileFromBuild(entry.Value.guid, fileGuid);
			}
			FileRefsRemove(fileGuid);
		}

		private void RemoveGroupIfEmpty(PBXGroupData gr)
		{
			if (gr.children.Count == 0 && gr != GroupsGetMainGroup())
			{
				PBXGroupData pBXGroupData = GroupsGetByChild(gr.guid);
				pBXGroupData.children.RemoveGUID(gr.guid);
				RemoveGroupIfEmpty(pBXGroupData);
				GroupsRemove(gr.guid);
			}
		}

		private void RemoveGroupChildrenRecursive(PBXGroupData parent)
		{
			List<string> list = new List<string>(parent.children);
			parent.children.Clear();
			foreach (string item in list)
			{
				PBXFileReferenceData pBXFileReferenceData = FileRefsGet(item);
				if (pBXFileReferenceData != null)
				{
					foreach (KeyValuePair<string, PBXNativeTargetData> entry in nativeTargets.GetEntries())
					{
						RemoveFileFromBuild(entry.Value.guid, item);
					}
					FileRefsRemove(item);
					continue;
				}
				PBXGroupData pBXGroupData = GroupsGet(item);
				if (pBXGroupData != null)
				{
					RemoveGroupChildrenRecursive(pBXGroupData);
					GroupsRemove(pBXGroupData.guid);
				}
			}
		}

		internal void RemoveFilesByProjectPathRecursive(string projectPath)
		{
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroupData pBXGroupData = GroupsGetByProjectPath(projectPath);
			if (pBXGroupData != null)
			{
				RemoveGroupChildrenRecursive(pBXGroupData);
				RemoveGroupIfEmpty(pBXGroupData);
			}
		}

		internal List<string> GetGroupChildrenFiles(string projectPath)
		{
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroupData pBXGroupData = GroupsGetByProjectPath(projectPath);
			if (pBXGroupData == null)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (string item in (IEnumerable<string>)pBXGroupData.children)
			{
				PBXFileReferenceData pBXFileReferenceData = FileRefsGet(item);
				if (pBXFileReferenceData != null)
				{
					list.Add(pBXFileReferenceData.name);
				}
			}
			return list;
		}

		private PBXGroupData GetPBXGroupChildByName(PBXGroupData group, string name)
		{
			foreach (string item in (IEnumerable<string>)group.children)
			{
				PBXGroupData pBXGroupData = GroupsGet(item);
				if (pBXGroupData != null && pBXGroupData.name == name)
				{
					return pBXGroupData;
				}
			}
			return null;
		}

		private PBXGroupData CreateSourceGroup(string sourceGroup)
		{
			sourceGroup = Utils.FixSlashesInPath(sourceGroup);
			if (sourceGroup == null || sourceGroup == string.Empty)
			{
				return GroupsGetMainGroup();
			}
			PBXGroupData pBXGroupData = GroupsGetByProjectPath(sourceGroup);
			if (pBXGroupData != null)
			{
				return pBXGroupData;
			}
			pBXGroupData = GroupsGetMainGroup();
			string[] array = Utils.SplitPath(sourceGroup);
			string text = null;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				text = ((text != null) ? (text + "/" + text2) : text2);
				PBXGroupData pBXGroupChildByName = GetPBXGroupChildByName(pBXGroupData, text2);
				if (pBXGroupChildByName != null)
				{
					pBXGroupData = pBXGroupChildByName;
					continue;
				}
				PBXGroupData pBXGroupData2 = PBXGroupData.Create(text2, text2, PBXSourceTree.Group);
				pBXGroupData.children.AddGUID(pBXGroupData2.guid);
				GroupsAdd(text, pBXGroupData, pBXGroupData2);
				pBXGroupData = pBXGroupData2;
			}
			return pBXGroupData;
		}

		public void AddExternalProjectDependency(string path, string projectPath, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			path = Utils.FixSlashesInPath(path);
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroupData pBXGroupData = PBXGroupData.CreateRelative("Products");
			GroupsAddDuplicate(pBXGroupData);
			PBXFileReferenceData pBXFileReferenceData = PBXFileReferenceData.CreateFromFile(path, Path.GetFileName(projectPath), sourceTree);
			FileRefsAdd(path, projectPath, null, pBXFileReferenceData);
			CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath)).children.AddGUID(pBXFileReferenceData.guid);
			project.project.AddReference(pBXGroupData.guid, pBXFileReferenceData.guid);
		}

		public void AddExternalLibraryDependency(string targetGuid, string filename, string remoteFileGuid, string projectPath, string remoteInfo)
		{
			PBXNativeTargetData target = nativeTargets[targetGuid];
			filename = Utils.FixSlashesInPath(filename);
			projectPath = Utils.FixSlashesInPath(projectPath);
			string text = FindFileGuidByRealPath(projectPath);
			if (text == null)
			{
				throw new Exception("No such project");
			}
			string text2 = null;
			foreach (ProjectReference projectReference in project.project.projectReferences)
			{
				if (projectReference.projectRef == text)
				{
					text2 = projectReference.group;
					break;
				}
			}
			if (text2 == null)
			{
				throw new Exception("Malformed project: no project in project references");
			}
			PBXGroupData pBXGroupData = GroupsGet(text2);
			string extension = Path.GetExtension(filename);
			if (!FileTypeUtils.IsBuildableFile(extension))
			{
				throw new Exception("Wrong file extension");
			}
			PBXContainerItemProxyData pBXContainerItemProxyData = PBXContainerItemProxyData.Create(text, "2", remoteFileGuid, remoteInfo);
			containerItems.AddEntry(pBXContainerItemProxyData);
			string typeName = FileTypeUtils.GetTypeName(extension);
			PBXReferenceProxyData pBXReferenceProxyData = PBXReferenceProxyData.Create(filename, typeName, pBXContainerItemProxyData.guid, "BUILT_PRODUCTS_DIR");
			references.AddEntry(pBXReferenceProxyData);
			PBXBuildFileData pBXBuildFileData = PBXBuildFileData.CreateFromFile(pBXReferenceProxyData.guid, false, null);
			BuildFilesAdd(targetGuid, pBXBuildFileData);
			BuildSectionAny(target, extension, false).files.AddGUID(pBXBuildFileData.guid);
			pBXGroupData.children.AddGUID(pBXReferenceProxyData.guid);
		}

		private void SetDefaultAppExtensionReleaseBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
		{
			config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
			config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
			config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
			config.AddProperty("CLANG_ENABLE_MODULES", "YES");
			config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
			config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
			config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
			config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
			config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
			config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
			config.AddProperty("COPY_PHASE_STRIP", "YES");
			config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
			config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
			config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
			config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
			config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
			config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
			config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
			config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
			config.AddProperty("INFOPLIST_FILE", infoPlistPath);
			config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
			config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
			config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
			config.AddProperty("SKIP_INSTALL", "YES");
			config.AddProperty("VALIDATE_PRODUCT", "YES");
		}

		private void SetDefaultAppExtensionDebugBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
		{
			config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
			config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
			config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
			config.AddProperty("CLANG_ENABLE_MODULES", "YES");
			config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
			config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
			config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
			config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
			config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
			config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
			config.AddProperty("COPY_PHASE_STRIP", "NO");
			config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
			config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
			config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
			config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
			config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
			config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
			config.AddProperty("GCC_SYMBOLS_PRIVATE_EXTERN", "NO");
			config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
			config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
			config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
			config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
			config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
			config.AddProperty("INFOPLIST_FILE", infoPlistPath);
			config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
			config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
			config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
			config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
			config.AddProperty("SKIP_INSTALL", "YES");
		}

		internal PBXNativeTargetData CreateNewTarget(string name, string ext, string type)
		{
			XCBuildConfigurationData xCBuildConfigurationData = XCBuildConfigurationData.Create("Release");
			buildConfigs.AddEntry(xCBuildConfigurationData);
			XCBuildConfigurationData xCBuildConfigurationData2 = XCBuildConfigurationData.Create("Debug");
			buildConfigs.AddEntry(xCBuildConfigurationData2);
			XCConfigurationListData xCConfigurationListData = XCConfigurationListData.Create();
			configs.AddEntry(xCConfigurationListData);
			xCConfigurationListData.buildConfigs.AddGUID(xCBuildConfigurationData.guid);
			xCConfigurationListData.buildConfigs.AddGUID(xCBuildConfigurationData2.guid);
			string text = name + ext;
			string productRef = AddFile(text, "Products/" + text, PBXSourceTree.Build);
			PBXNativeTargetData pBXNativeTargetData = PBXNativeTargetData.Create(name, productRef, type, xCConfigurationListData.guid);
			nativeTargets.AddEntry(pBXNativeTargetData);
			project.project.targets.Add(pBXNativeTargetData.guid);
			return pBXNativeTargetData;
		}

		internal string AddAppExtension(string mainTarget, string name, string infoPlistPath)
		{
			string text = ".appex";
			PBXNativeTargetData pBXNativeTargetData = CreateNewTarget(name, text, "com.apple.product-type.app-extension");
			SetDefaultAppExtensionReleaseBuildFlags(buildConfigs[BuildConfigByName(pBXNativeTargetData.guid, "Release")], infoPlistPath);
			SetDefaultAppExtensionDebugBuildFlags(buildConfigs[BuildConfigByName(pBXNativeTargetData.guid, "Debug")], infoPlistPath);
			PBXSourcesBuildPhaseData pBXSourcesBuildPhaseData = PBXSourcesBuildPhaseData.Create();
			sources.AddEntry(pBXSourcesBuildPhaseData);
			pBXNativeTargetData.phases.AddGUID(pBXSourcesBuildPhaseData.guid);
			PBXResourcesBuildPhaseData pBXResourcesBuildPhaseData = PBXResourcesBuildPhaseData.Create();
			resources.AddEntry(pBXResourcesBuildPhaseData);
			pBXNativeTargetData.phases.AddGUID(pBXResourcesBuildPhaseData.guid);
			PBXFrameworksBuildPhaseData pBXFrameworksBuildPhaseData = PBXFrameworksBuildPhaseData.Create();
			frameworks.AddEntry(pBXFrameworksBuildPhaseData);
			pBXNativeTargetData.phases.AddGUID(pBXFrameworksBuildPhaseData.guid);
			PBXCopyFilesBuildPhaseData pBXCopyFilesBuildPhaseData = PBXCopyFilesBuildPhaseData.Create("Embed App Extensions", "13");
			copyFiles.AddEntry(pBXCopyFilesBuildPhaseData);
			nativeTargets[mainTarget].phases.AddGUID(pBXCopyFilesBuildPhaseData.guid);
			PBXContainerItemProxyData pBXContainerItemProxyData = PBXContainerItemProxyData.Create(project.project.guid, "1", pBXNativeTargetData.guid, name);
			containerItems.AddEntry(pBXContainerItemProxyData);
			PBXTargetDependencyData pBXTargetDependencyData = PBXTargetDependencyData.Create(pBXNativeTargetData.guid, pBXContainerItemProxyData.guid);
			targetDependencies.AddEntry(pBXTargetDependencyData);
			nativeTargets[mainTarget].dependencies.AddGUID(pBXTargetDependencyData.guid);
			PBXBuildFileData pBXBuildFileData = PBXBuildFileData.CreateFromFile(FindFileGuidByProjectPath("Products/" + name + text), false, string.Empty);
			BuildFilesAdd(mainTarget, pBXBuildFileData);
			pBXCopyFilesBuildPhaseData.files.AddGUID(pBXBuildFileData.guid);
			AddFile(infoPlistPath, name + "/Supporting Files/Info.plist", PBXSourceTree.Source);
			return pBXNativeTargetData.guid;
		}

		public string BuildConfigByName(string targetGuid, string name)
		{
			PBXNativeTargetData pBXNativeTargetData = nativeTargets[targetGuid];
			foreach (string item in (IEnumerable<string>)configs[pBXNativeTargetData.buildConfigList].buildConfigs)
			{
				XCBuildConfigurationData xCBuildConfigurationData = buildConfigs[item];
				if (xCBuildConfigurationData != null && xCBuildConfigurationData.name == name)
				{
					return xCBuildConfigurationData.guid;
				}
			}
			return null;
		}

		private string GetConfigListForTarget(string targetGuid)
		{
			if (targetGuid == project.project.guid)
			{
				return project.project.buildConfigList;
			}
			return nativeTargets[targetGuid].buildConfigList;
		}

		public void AddBuildProperty(string targetGuid, string name, string value)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				AddBuildPropertyForConfig(item, name, value);
			}
		}

		public void AddBuildProperty(IEnumerable<string> targetGuids, string name, string value)
		{
			foreach (string targetGuid in targetGuids)
			{
				AddBuildProperty(targetGuid, name, value);
			}
		}

		public void AddBuildPropertyForConfig(string configGuid, string name, string value)
		{
			buildConfigs[configGuid].AddProperty(name, value);
		}

		public void AddBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
		{
			foreach (string configGuid in configGuids)
			{
				AddBuildPropertyForConfig(configGuid, name, value);
			}
		}

		public void SetBuildProperty(string targetGuid, string name, string value)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				SetBuildPropertyForConfig(item, name, value);
			}
		}

		public void SetBuildProperty(IEnumerable<string> targetGuids, string name, string value)
		{
			foreach (string targetGuid in targetGuids)
			{
				SetBuildProperty(targetGuid, name, value);
			}
		}

		public void SetBuildPropertyForConfig(string configGuid, string name, string value)
		{
			buildConfigs[configGuid].SetProperty(name, value);
		}

		public void SetBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
		{
			foreach (string configGuid in configGuids)
			{
				SetBuildPropertyForConfig(configGuid, name, value);
			}
		}

		internal void RemoveBuildProperty(string targetGuid, string name)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				RemoveBuildPropertyForConfig(item, name);
			}
		}

		internal void RemoveBuildProperty(IEnumerable<string> targetGuids, string name)
		{
			foreach (string targetGuid in targetGuids)
			{
				RemoveBuildProperty(targetGuid, name);
			}
		}

		internal void RemoveBuildPropertyForConfig(string configGuid, string name)
		{
			buildConfigs[configGuid].RemoveProperty(name);
		}

		internal void RemoveBuildPropertyForConfig(IEnumerable<string> configGuids, string name)
		{
			foreach (string configGuid in configGuids)
			{
				RemoveBuildPropertyForConfig(configGuid, name);
			}
		}

		public void UpdateBuildProperty(string targetGuid, string name, IEnumerable<string> addValues, IEnumerable<string> removeValues)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				UpdateBuildPropertyForConfig(item, name, addValues, removeValues);
			}
		}

		public void UpdateBuildProperty(IEnumerable<string> targetGuids, string name, IEnumerable<string> addValues, IEnumerable<string> removeValues)
		{
			foreach (string targetGuid in targetGuids)
			{
				UpdateBuildProperty(targetGuid, name, addValues, removeValues);
			}
		}

		public void UpdateBuildPropertyForConfig(string configGuid, string name, IEnumerable<string> addValues, IEnumerable<string> removeValues)
		{
			XCBuildConfigurationData xCBuildConfigurationData = buildConfigs[configGuid];
			if (xCBuildConfigurationData == null)
			{
				return;
			}
			if (removeValues != null)
			{
				foreach (string removeValue in removeValues)
				{
					xCBuildConfigurationData.RemovePropertyValue(name, removeValue);
				}
			}
			if (addValues == null)
			{
				return;
			}
			foreach (string addValue in addValues)
			{
				xCBuildConfigurationData.AddProperty(name, addValue);
			}
		}

		public void UpdateBuildPropertyForConfig(IEnumerable<string> configGuids, string name, IEnumerable<string> addValues, IEnumerable<string> removeValues)
		{
			foreach (string configGuid in configGuids)
			{
				UpdateBuildProperty(configGuid, name, addValues, removeValues);
			}
		}

		public void ReadFromFile(string path)
		{
			ReadFromString(File.ReadAllText(path));
		}

		public void ReadFromString(string src)
		{
			TextReader sr = new StringReader(src);
			ReadFromStream(sr);
		}

		public void ReadFromStream(TextReader sr)
		{
			m_Data.ReadFromStream(sr);
		}

		public void WriteToFile(string path)
		{
			File.WriteAllText(path, WriteToString());
		}

		public void WriteToStream(TextWriter sw)
		{
			sw.Write(WriteToString());
		}

		public string WriteToString()
		{
			return m_Data.WriteToString();
		}
	}
}
