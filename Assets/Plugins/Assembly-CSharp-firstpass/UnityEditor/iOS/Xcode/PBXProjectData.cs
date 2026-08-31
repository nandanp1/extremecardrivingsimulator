using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.iOS.Xcode.PBX;

namespace UnityEditor.iOS.Xcode
{
	internal class PBXProjectData
	{
		private Dictionary<string, SectionBase> m_Section;

		private PBXElementDict m_RootElements;

		private PBXElementDict m_UnknownObjects;

		private string m_ObjectVersion;

		private List<string> m_SectionOrder;

		private Dictionary<string, KnownSectionBase<PBXObjectData>> m_UnknownSections;

		private KnownSectionBase<PBXBuildFileData> buildFiles;

		private KnownSectionBase<PBXFileReferenceData> fileRefs;

		private KnownSectionBase<PBXGroupData> groups;

		public KnownSectionBase<PBXContainerItemProxyData> containerItems;

		public KnownSectionBase<PBXReferenceProxyData> references;

		public KnownSectionBase<PBXSourcesBuildPhaseData> sources;

		public KnownSectionBase<PBXFrameworksBuildPhaseData> frameworks;

		public KnownSectionBase<PBXResourcesBuildPhaseData> resources;

		public KnownSectionBase<PBXCopyFilesBuildPhaseData> copyFiles;

		public KnownSectionBase<PBXShellScriptBuildPhaseData> shellScripts;

		public KnownSectionBase<PBXNativeTargetData> nativeTargets;

		public KnownSectionBase<PBXTargetDependencyData> targetDependencies;

		public KnownSectionBase<PBXVariantGroupData> variantGroups;

		public KnownSectionBase<XCBuildConfigurationData> buildConfigs;

		public KnownSectionBase<XCConfigurationListData> configs;

		public PBXProjectSection project;

		private Dictionary<string, Dictionary<string, PBXBuildFileData>> m_FileGuidToBuildFileMap;

		private Dictionary<string, PBXFileReferenceData> m_ProjectPathToFileRefMap;

		private Dictionary<string, string> m_FileRefGuidToProjectPathMap;

		private Dictionary<PBXSourceTree, Dictionary<string, PBXFileReferenceData>> m_RealPathToFileRefMap;

		private Dictionary<string, PBXGroupData> m_ProjectPathToGroupMap;

		private Dictionary<string, string> m_GroupGuidToProjectPathMap;

		private Dictionary<string, PBXGroupData> m_GuidToParentGroupMap;

		public PBXBuildFileData BuildFilesGet(string guid)
		{
			return buildFiles[guid];
		}

		public void BuildFilesAdd(string targetGuid, PBXBuildFileData buildFile)
		{
			if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
			{
				m_FileGuidToBuildFileMap[targetGuid] = new Dictionary<string, PBXBuildFileData>();
			}
			m_FileGuidToBuildFileMap[targetGuid][buildFile.fileRef] = buildFile;
			buildFiles.AddEntry(buildFile);
		}

		public void BuildFilesRemove(string targetGuid, string fileGuid)
		{
			PBXBuildFileData pBXBuildFileData = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFileData != null)
			{
				m_FileGuidToBuildFileMap[targetGuid].Remove(pBXBuildFileData.fileRef);
				buildFiles.RemoveEntry(pBXBuildFileData.guid);
			}
		}

		public PBXBuildFileData BuildFilesGetForSourceFile(string targetGuid, string fileGuid)
		{
			if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
			{
				return null;
			}
			if (!m_FileGuidToBuildFileMap[targetGuid].ContainsKey(fileGuid))
			{
				return null;
			}
			return m_FileGuidToBuildFileMap[targetGuid][fileGuid];
		}

		public IEnumerable<PBXBuildFileData> BuildFilesGetAll()
		{
			return buildFiles.GetObjects();
		}

		public void FileRefsAdd(string realPath, string projectPath, PBXGroupData parent, PBXFileReferenceData fileRef)
		{
			fileRefs.AddEntry(fileRef);
			m_ProjectPathToFileRefMap.Add(projectPath, fileRef);
			m_FileRefGuidToProjectPathMap.Add(fileRef.guid, projectPath);
			m_RealPathToFileRefMap[fileRef.tree].Add(realPath, fileRef);
			m_GuidToParentGroupMap.Add(fileRef.guid, parent);
		}

		public PBXFileReferenceData FileRefsGet(string guid)
		{
			return fileRefs[guid];
		}

		public PBXFileReferenceData FileRefsGetByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (m_RealPathToFileRefMap[sourceTree].ContainsKey(path))
			{
				return m_RealPathToFileRefMap[sourceTree][path];
			}
			return null;
		}

		public PBXFileReferenceData FileRefsGetByProjectPath(string path)
		{
			if (m_ProjectPathToFileRefMap.ContainsKey(path))
			{
				return m_ProjectPathToFileRefMap[path];
			}
			return null;
		}

		public void FileRefsRemove(string guid)
		{
			PBXFileReferenceData pBXFileReferenceData = fileRefs[guid];
			fileRefs.RemoveEntry(guid);
			m_ProjectPathToFileRefMap.Remove(m_FileRefGuidToProjectPathMap[guid]);
			m_FileRefGuidToProjectPathMap.Remove(guid);
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				m_RealPathToFileRefMap[item].Remove(pBXFileReferenceData.path);
			}
			m_GuidToParentGroupMap.Remove(guid);
		}

		public PBXGroupData GroupsGet(string guid)
		{
			return groups[guid];
		}

		public PBXGroupData GroupsGetByChild(string childGuid)
		{
			return m_GuidToParentGroupMap[childGuid];
		}

		public PBXGroupData GroupsGetMainGroup()
		{
			return groups[project.project.mainGroup];
		}

		public PBXGroupData GroupsGetByProjectPath(string sourceGroup)
		{
			if (m_ProjectPathToGroupMap.ContainsKey(sourceGroup))
			{
				return m_ProjectPathToGroupMap[sourceGroup];
			}
			return null;
		}

		public void GroupsAdd(string projectPath, PBXGroupData parent, PBXGroupData gr)
		{
			m_ProjectPathToGroupMap.Add(projectPath, gr);
			m_GroupGuidToProjectPathMap.Add(gr.guid, projectPath);
			m_GuidToParentGroupMap.Add(gr.guid, parent);
			groups.AddEntry(gr);
		}

		public void GroupsAddDuplicate(PBXGroupData gr)
		{
			groups.AddEntry(gr);
		}

		public void GroupsRemove(string guid)
		{
			m_ProjectPathToGroupMap.Remove(m_GroupGuidToProjectPathMap[guid]);
			m_GroupGuidToProjectPathMap.Remove(guid);
			m_GuidToParentGroupMap.Remove(guid);
			groups.RemoveEntry(guid);
		}

		public FileGUIDListBase BuildSectionAny(PBXNativeTargetData target, string path, bool isFolderRef)
		{
			string extension = Path.GetExtension(path);
			switch (FileTypeUtils.GetFileType(extension, isFolderRef))
			{
			case PBXFileType.Framework:
				foreach (string item in (IEnumerable<string>)target.phases)
				{
					if (frameworks.HasEntry(item))
					{
						return frameworks[item];
					}
				}
				break;
			case PBXFileType.Resource:
				foreach (string item2 in (IEnumerable<string>)target.phases)
				{
					if (resources.HasEntry(item2))
					{
						return resources[item2];
					}
				}
				break;
			case PBXFileType.Source:
				foreach (string item3 in (IEnumerable<string>)target.phases)
				{
					if (sources.HasEntry(item3))
					{
						return sources[item3];
					}
				}
				break;
			case PBXFileType.CopyFile:
				foreach (string item4 in (IEnumerable<string>)target.phases)
				{
					if (copyFiles.HasEntry(item4))
					{
						return copyFiles[item4];
					}
				}
				break;
			}
			return null;
		}

		private void RefreshBuildFilesMapForBuildFileGuidList(Dictionary<string, PBXBuildFileData> mapForTarget, FileGUIDListBase list)
		{
			foreach (string item in (IEnumerable<string>)list.files)
			{
				PBXBuildFileData pBXBuildFileData = buildFiles[item];
				mapForTarget[pBXBuildFileData.fileRef] = pBXBuildFileData;
			}
		}

		private void RefreshMapsForGroupChildren(string projectPath, string realPath, PBXSourceTree realPathTree, PBXGroupData parent)
		{
			List<string> list = new List<string>(parent.children);
			foreach (string item in list)
			{
				PBXFileReferenceData pBXFileReferenceData = fileRefs[item];
				string resPath;
				PBXSourceTree resTree;
				if (pBXFileReferenceData != null)
				{
					string text = Utils.CombinePaths(projectPath, pBXFileReferenceData.name);
					Utils.CombinePaths(realPath, realPathTree, pBXFileReferenceData.path, pBXFileReferenceData.tree, out resPath, out resTree);
					if (!m_ProjectPathToFileRefMap.ContainsKey(text))
					{
						m_ProjectPathToFileRefMap.Add(text, pBXFileReferenceData);
					}
					if (!m_FileRefGuidToProjectPathMap.ContainsKey(pBXFileReferenceData.guid))
					{
						m_FileRefGuidToProjectPathMap.Add(pBXFileReferenceData.guid, text);
					}
					if (!m_RealPathToFileRefMap[resTree].ContainsKey(resPath))
					{
						m_RealPathToFileRefMap[resTree].Add(resPath, pBXFileReferenceData);
					}
					if (!m_GuidToParentGroupMap.ContainsKey(item))
					{
						m_GuidToParentGroupMap.Add(item, parent);
					}
					continue;
				}
				PBXGroupData pBXGroupData = groups[item];
				if (pBXGroupData != null)
				{
					string text = Utils.CombinePaths(projectPath, pBXGroupData.name);
					Utils.CombinePaths(realPath, realPathTree, pBXGroupData.path, pBXGroupData.tree, out resPath, out resTree);
					if (!m_ProjectPathToGroupMap.ContainsKey(text))
					{
						m_ProjectPathToGroupMap.Add(text, pBXGroupData);
					}
					if (!m_GroupGuidToProjectPathMap.ContainsKey(pBXGroupData.guid))
					{
						m_GroupGuidToProjectPathMap.Add(pBXGroupData.guid, text);
					}
					if (!m_GuidToParentGroupMap.ContainsKey(item))
					{
						m_GuidToParentGroupMap.Add(item, parent);
					}
					RefreshMapsForGroupChildren(text, resPath, resTree, pBXGroupData);
				}
			}
		}

		private void RefreshAuxMaps()
		{
			foreach (KeyValuePair<string, PBXNativeTargetData> entry in nativeTargets.GetEntries())
			{
				Dictionary<string, PBXBuildFileData> dictionary = new Dictionary<string, PBXBuildFileData>();
				foreach (string item in (IEnumerable<string>)entry.Value.phases)
				{
					if (frameworks.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, frameworks[item]);
					}
					if (resources.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, resources[item]);
					}
					if (sources.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, sources[item]);
					}
					if (copyFiles.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, copyFiles[item]);
					}
				}
				m_FileGuidToBuildFileMap[entry.Key] = dictionary;
			}
			RefreshMapsForGroupChildren(string.Empty, string.Empty, PBXSourceTree.Source, GroupsGetMainGroup());
		}

		public void Clear()
		{
			buildFiles = new KnownSectionBase<PBXBuildFileData>("PBXBuildFile");
			fileRefs = new KnownSectionBase<PBXFileReferenceData>("PBXFileReference");
			groups = new KnownSectionBase<PBXGroupData>("PBXGroup");
			containerItems = new KnownSectionBase<PBXContainerItemProxyData>("PBXContainerItemProxy");
			references = new KnownSectionBase<PBXReferenceProxyData>("PBXReferenceProxy");
			sources = new KnownSectionBase<PBXSourcesBuildPhaseData>("PBXSourcesBuildPhase");
			frameworks = new KnownSectionBase<PBXFrameworksBuildPhaseData>("PBXFrameworksBuildPhase");
			resources = new KnownSectionBase<PBXResourcesBuildPhaseData>("PBXResourcesBuildPhase");
			copyFiles = new KnownSectionBase<PBXCopyFilesBuildPhaseData>("PBXCopyFilesBuildPhase");
			shellScripts = new KnownSectionBase<PBXShellScriptBuildPhaseData>("PBXShellScriptBuildPhase");
			nativeTargets = new KnownSectionBase<PBXNativeTargetData>("PBXNativeTarget");
			targetDependencies = new KnownSectionBase<PBXTargetDependencyData>("PBXTargetDependency");
			variantGroups = new KnownSectionBase<PBXVariantGroupData>("PBXVariantGroup");
			buildConfigs = new KnownSectionBase<XCBuildConfigurationData>("XCBuildConfiguration");
			configs = new KnownSectionBase<XCConfigurationListData>("XCConfigurationList");
			project = new PBXProjectSection();
			m_UnknownSections = new Dictionary<string, KnownSectionBase<PBXObjectData>>();
			m_Section = new Dictionary<string, SectionBase>
			{
				{ "PBXBuildFile", buildFiles },
				{ "PBXFileReference", fileRefs },
				{ "PBXGroup", groups },
				{ "PBXContainerItemProxy", containerItems },
				{ "PBXReferenceProxy", references },
				{ "PBXSourcesBuildPhase", sources },
				{ "PBXFrameworksBuildPhase", frameworks },
				{ "PBXResourcesBuildPhase", resources },
				{ "PBXCopyFilesBuildPhase", copyFiles },
				{ "PBXShellScriptBuildPhase", shellScripts },
				{ "PBXNativeTarget", nativeTargets },
				{ "PBXTargetDependency", targetDependencies },
				{ "PBXVariantGroup", variantGroups },
				{ "XCBuildConfiguration", buildConfigs },
				{ "XCConfigurationList", configs },
				{ "PBXProject", project }
			};
			m_RootElements = new PBXElementDict();
			m_UnknownObjects = new PBXElementDict();
			m_ObjectVersion = null;
			m_SectionOrder = new List<string>
			{
				"PBXBuildFile", "PBXContainerItemProxy", "PBXCopyFilesBuildPhase", "PBXFileReference", "PBXFrameworksBuildPhase", "PBXGroup", "PBXNativeTarget", "PBXProject", "PBXReferenceProxy", "PBXResourcesBuildPhase",
				"PBXShellScriptBuildPhase", "PBXSourcesBuildPhase", "PBXTargetDependency", "PBXVariantGroup", "XCBuildConfiguration", "XCConfigurationList"
			};
			m_FileGuidToBuildFileMap = new Dictionary<string, Dictionary<string, PBXBuildFileData>>();
			m_ProjectPathToFileRefMap = new Dictionary<string, PBXFileReferenceData>();
			m_FileRefGuidToProjectPathMap = new Dictionary<string, string>();
			m_RealPathToFileRefMap = new Dictionary<PBXSourceTree, Dictionary<string, PBXFileReferenceData>>();
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				m_RealPathToFileRefMap.Add(item, new Dictionary<string, PBXFileReferenceData>());
			}
			m_ProjectPathToGroupMap = new Dictionary<string, PBXGroupData>();
			m_GroupGuidToProjectPathMap = new Dictionary<string, string>();
			m_GuidToParentGroupMap = new Dictionary<string, PBXGroupData>();
		}

		private void BuildCommentMapForBuildFiles(GUIDToCommentMap comments, List<string> guids, string sectName)
		{
			foreach (string guid in guids)
			{
				PBXBuildFileData pBXBuildFileData = BuildFilesGet(guid);
				if (pBXBuildFileData == null)
				{
					continue;
				}
				PBXFileReferenceData pBXFileReferenceData = FileRefsGet(pBXBuildFileData.fileRef);
				if (pBXFileReferenceData != null)
				{
					comments.Add(guid, string.Format("{0} in {1}", pBXFileReferenceData.name, sectName));
					continue;
				}
				PBXReferenceProxyData pBXReferenceProxyData = references[pBXBuildFileData.fileRef];
				if (pBXReferenceProxyData != null)
				{
					comments.Add(guid, string.Format("{0} in {1}", pBXReferenceProxyData.path, sectName));
				}
			}
		}

		private GUIDToCommentMap BuildCommentMap()
		{
			GUIDToCommentMap gUIDToCommentMap = new GUIDToCommentMap();
			foreach (PBXGroupData @object in groups.GetObjects())
			{
				gUIDToCommentMap.Add(@object.guid, @object.name);
			}
			foreach (PBXContainerItemProxyData object2 in containerItems.GetObjects())
			{
				gUIDToCommentMap.Add(object2.guid, "PBXContainerItemProxy");
			}
			foreach (PBXReferenceProxyData object3 in references.GetObjects())
			{
				gUIDToCommentMap.Add(object3.guid, object3.path);
			}
			foreach (PBXSourcesBuildPhaseData object4 in sources.GetObjects())
			{
				gUIDToCommentMap.Add(object4.guid, "Sources");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object4.files, "Sources");
			}
			foreach (PBXResourcesBuildPhaseData object5 in resources.GetObjects())
			{
				gUIDToCommentMap.Add(object5.guid, "Resources");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object5.files, "Resources");
			}
			foreach (PBXFrameworksBuildPhaseData object6 in frameworks.GetObjects())
			{
				gUIDToCommentMap.Add(object6.guid, "Frameworks");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object6.files, "Frameworks");
			}
			foreach (PBXCopyFilesBuildPhaseData object7 in copyFiles.GetObjects())
			{
				string text = object7.name;
				if (text == null)
				{
					text = "CopyFiles";
				}
				gUIDToCommentMap.Add(object7.guid, text);
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object7.files, text);
			}
			foreach (PBXShellScriptBuildPhaseData object8 in shellScripts.GetObjects())
			{
				gUIDToCommentMap.Add(object8.guid, "ShellScript");
			}
			foreach (PBXTargetDependencyData object9 in targetDependencies.GetObjects())
			{
				gUIDToCommentMap.Add(object9.guid, "PBXTargetDependency");
			}
			foreach (PBXNativeTargetData object10 in nativeTargets.GetObjects())
			{
				gUIDToCommentMap.Add(object10.guid, object10.name);
				gUIDToCommentMap.Add(object10.buildConfigList, string.Format("Build configuration list for PBXNativeTarget \"{0}\"", object10.name));
			}
			foreach (PBXVariantGroupData object11 in variantGroups.GetObjects())
			{
				gUIDToCommentMap.Add(object11.guid, object11.name);
			}
			foreach (XCBuildConfigurationData object12 in buildConfigs.GetObjects())
			{
				gUIDToCommentMap.Add(object12.guid, object12.name);
			}
			foreach (PBXProjectObjectData object13 in project.GetObjects())
			{
				gUIDToCommentMap.Add(object13.guid, "Project object");
				gUIDToCommentMap.Add(object13.buildConfigList, "Build configuration list for PBXProject \"Unity-iPhone\"");
			}
			foreach (PBXFileReferenceData object14 in fileRefs.GetObjects())
			{
				gUIDToCommentMap.Add(object14.guid, object14.name);
			}
			if (m_RootElements.Contains("rootObject") && m_RootElements["rootObject"] is PBXElementString)
			{
				gUIDToCommentMap.Add(m_RootElements["rootObject"].AsString(), "Project object");
			}
			return gUIDToCommentMap;
		}

		private static PBXElementDict ParseContent(string content)
		{
			TokenList tokens = Lexer.Tokenize(content);
			Parser parser = new Parser(tokens);
			TreeAST ast = parser.ParseTree();
			return Serializer.ParseTreeAST(ast, tokens, content);
		}

		public void ReadFromStream(TextReader sr)
		{
			Clear();
			m_RootElements = ParseContent(sr.ReadToEnd());
			if (!m_RootElements.Contains("objects"))
			{
				throw new Exception("Invalid PBX project file: no objects element");
			}
			PBXElementDict pBXElementDict = m_RootElements["objects"].AsDict();
			m_RootElements.Remove("objects");
			m_RootElements.SetString("objects", "OBJMARKER");
			if (m_RootElements.Contains("objectVersion"))
			{
				m_ObjectVersion = m_RootElements["objectVersion"].AsString();
				m_RootElements.Remove("objectVersion");
			}
			List<string> list = new List<string>();
			string prevSectionName = null;
			foreach (KeyValuePair<string, PBXElement> value2 in pBXElementDict.values)
			{
				list.Add(value2.Key);
				PBXElement value = value2.Value;
				if (!(value is PBXElementDict) || !value.AsDict().Contains("isa"))
				{
					m_UnknownObjects.values.Add(value2.Key, value);
					continue;
				}
				PBXElementDict pBXElementDict2 = value.AsDict();
				string text = pBXElementDict2["isa"].AsString();
				if (m_Section.ContainsKey(text))
				{
					SectionBase sectionBase = m_Section[text];
					sectionBase.AddObject(value2.Key, pBXElementDict2);
				}
				else
				{
					KnownSectionBase<PBXObjectData> knownSectionBase;
					if (m_UnknownSections.ContainsKey(text))
					{
						knownSectionBase = m_UnknownSections[text];
					}
					else
					{
						knownSectionBase = new KnownSectionBase<PBXObjectData>(text);
						m_UnknownSections.Add(text, knownSectionBase);
					}
					knownSectionBase.AddObject(value2.Key, pBXElementDict2);
					if (!m_SectionOrder.Contains(text))
					{
						int index = 0;
						if (prevSectionName != null)
						{
							index = m_SectionOrder.FindIndex((string x) => x == prevSectionName);
							index++;
						}
						m_SectionOrder.Insert(index, text);
					}
				}
				prevSectionName = text;
			}
			RepairStructure(list);
			RefreshAuxMaps();
		}

		public string WriteToString()
		{
			GUIDToCommentMap comments = BuildCommentMap();
			PropertyCommentChecker checker = new PropertyCommentChecker();
			GUIDToCommentMap comments2 = new GUIDToCommentMap();
			StringBuilder stringBuilder = new StringBuilder();
			if (m_ObjectVersion != null)
			{
				stringBuilder.AppendFormat("objectVersion = {0};\n\t", m_ObjectVersion);
			}
			stringBuilder.Append("objects = {");
			foreach (string item in m_SectionOrder)
			{
				if (m_Section.ContainsKey(item))
				{
					m_Section[item].WriteSection(stringBuilder, comments);
				}
				else if (m_UnknownSections.ContainsKey(item))
				{
					m_UnknownSections[item].WriteSection(stringBuilder, comments);
				}
			}
			foreach (KeyValuePair<string, PBXElement> value in m_UnknownObjects.values)
			{
				Serializer.WriteDictKeyValue(stringBuilder, value.Key, value.Value, 2, false, checker, comments2);
			}
			stringBuilder.Append("\n\t};");
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("// !$*UTF8*$!\n");
			Serializer.WriteDict(stringBuilder2, m_RootElements, 0, false, new PropertyCommentChecker(new string[1] { "rootObject/*" }), comments);
			stringBuilder2.Append("\n");
			string text = stringBuilder2.ToString();
			return text.Replace("objects = OBJMARKER;", stringBuilder.ToString());
		}

		private void RepairStructure(List<string> allGuids)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (string allGuid in allGuids)
			{
				dictionary.Add(allGuid, false);
			}
			while (RepairStructureImpl(dictionary))
			{
			}
		}

		private static void RemoveMissingGuidsFromGuidList(GUIDList guidList, Dictionary<string, bool> allGuids)
		{
			List<string> list = null;
			foreach (string item in (IEnumerable<string>)guidList)
			{
				if (!allGuids.ContainsKey(item))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(item);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (string item2 in list)
			{
				guidList.RemoveGUID(item2);
			}
		}

		private static bool RemoveObjectsFromSection<T>(KnownSectionBase<T> section, Dictionary<string, bool> allGuids, Func<T, bool> checker) where T : PBXObjectData, new()
		{
			List<string> list = null;
			foreach (KeyValuePair<string, T> entry in section.GetEntries())
			{
				if (checker(entry.Value))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(entry.Key);
				}
			}
			if (list != null)
			{
				foreach (string item in list)
				{
					section.RemoveEntry(item);
					allGuids.Remove(item);
				}
				return true;
			}
			return false;
		}

		private bool RepairStructureImpl(Dictionary<string, bool> allGuids)
		{
			bool flag = false;
			flag |= RemoveObjectsFromSection(buildFiles, allGuids, (PBXBuildFileData o) => o.fileRef == null || !allGuids.ContainsKey(o.fileRef));
			flag |= RemoveObjectsFromSection(groups, allGuids, (PBXGroupData o) => o.children == null);
			foreach (PBXGroupData @object in groups.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(@object.children, allGuids);
			}
			flag |= RemoveObjectsFromSection(sources, allGuids, (PBXSourcesBuildPhaseData o) => o.files == null);
			foreach (PBXSourcesBuildPhaseData object2 in sources.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object2.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(frameworks, allGuids, (PBXFrameworksBuildPhaseData o) => o.files == null);
			foreach (PBXFrameworksBuildPhaseData object3 in frameworks.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object3.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(resources, allGuids, (PBXResourcesBuildPhaseData o) => o.files == null);
			foreach (PBXResourcesBuildPhaseData object4 in resources.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object4.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(copyFiles, allGuids, (PBXCopyFilesBuildPhaseData o) => o.files == null);
			foreach (PBXCopyFilesBuildPhaseData object5 in copyFiles.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object5.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(shellScripts, allGuids, (PBXShellScriptBuildPhaseData o) => o.files == null);
			foreach (PBXShellScriptBuildPhaseData object6 in shellScripts.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object6.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(nativeTargets, allGuids, (PBXNativeTargetData o) => o.phases == null);
			foreach (PBXNativeTargetData object7 in nativeTargets.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object7.phases, allGuids);
			}
			flag |= RemoveObjectsFromSection(variantGroups, allGuids, (PBXVariantGroupData o) => o.children == null);
			foreach (PBXVariantGroupData object8 in variantGroups.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object8.children, allGuids);
			}
			flag |= RemoveObjectsFromSection(configs, allGuids, (XCConfigurationListData o) => o.buildConfigs == null);
			foreach (XCConfigurationListData object9 in configs.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object9.buildConfigs, allGuids);
			}
			return flag;
		}
	}
}
