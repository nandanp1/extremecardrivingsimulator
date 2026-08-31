using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEditor.iOS.Xcode.PBX
{
	internal class KnownSectionBase<T> : SectionBase where T : PBXObjectData, new()
	{
		private Dictionary<string, T> m_Entries = new Dictionary<string, T>();

		private string m_Name;

		public T this[string guid]
		{
			get
			{
				if (m_Entries.ContainsKey(guid))
				{
					return m_Entries[guid];
				}
				return (T)null;
			}
		}

		public KnownSectionBase(string sectionName)
		{
			m_Name = sectionName;
		}

		public IEnumerable<KeyValuePair<string, T>> GetEntries()
		{
			return m_Entries;
		}

		public IEnumerable<string> GetGuids()
		{
			return m_Entries.Keys;
		}

		public IEnumerable<T> GetObjects()
		{
			return m_Entries.Values;
		}

		public override void AddObject(string key, PBXElementDict value)
		{
			T val = new T();
			val.guid = key;
			val.SetPropertiesWhenSerializing(value);
			val.UpdateVars();
			m_Entries[val.guid] = val;
		}

		public override void WriteSection(StringBuilder sb, GUIDToCommentMap comments)
		{
			if (m_Entries.Count == 0)
			{
				return;
			}
			sb.AppendFormat("\n\n/* Begin {0} section */", m_Name);
			List<string> list = new List<string>(m_Entries.Keys);
			list.Sort(StringComparer.Ordinal);
			foreach (string item in list)
			{
				T val = m_Entries[item];
				val.UpdateProps();
				sb.Append("\n\t\t");
				comments.WriteStringBuilder(sb, val.guid);
				sb.Append(" = ");
				Serializer.WriteDict(sb, val.GetPropertiesWhenSerializing(), 2, val.shouldCompact, val.checker, comments);
				sb.Append(";");
			}
			sb.AppendFormat("\n/* End {0} section */", m_Name);
		}

		public bool HasEntry(string guid)
		{
			return m_Entries.ContainsKey(guid);
		}

		public void AddEntry(T obj)
		{
			m_Entries[obj.guid] = obj;
		}

		public void RemoveEntry(string guid)
		{
			if (m_Entries.ContainsKey(guid))
			{
				m_Entries.Remove(guid);
			}
		}
	}
}
