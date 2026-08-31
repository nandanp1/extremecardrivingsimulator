using System.IO;
using System.Text;

namespace UnityEditor.iOS.Xcode
{
	public class JsonDocument
	{
		public JsonElementDict root;

		public string indentString = "  ";

		public JsonDocument()
		{
			root = new JsonElementDict();
		}

		private void AppendIndent(StringBuilder sb, int indent)
		{
			for (int i = 0; i < indent; i++)
			{
				sb.Append(indentString);
			}
		}

		private void WriteString(StringBuilder sb, string str)
		{
			sb.Append('"');
			sb.Append(str);
			sb.Append('"');
		}

		private void WriteBoolean(StringBuilder sb, bool value)
		{
			sb.Append((!value) ? "false" : "true");
		}

		private void WriteInteger(StringBuilder sb, int value)
		{
			sb.Append(value.ToString());
		}

		private void WriteDictKeyValue(StringBuilder sb, string key, JsonElement value, int indent)
		{
			sb.Append("\n");
			AppendIndent(sb, indent);
			WriteString(sb, key);
			sb.Append(" : ");
			if (value is JsonElementString)
			{
				WriteString(sb, value.AsString());
			}
			else if (value is JsonElementInteger)
			{
				WriteInteger(sb, value.AsInteger());
			}
			else if (value is JsonElementBoolean)
			{
				WriteBoolean(sb, value.AsBoolean());
			}
			else if (value is JsonElementDict)
			{
				WriteDict(sb, value.AsDict(), indent);
			}
			else if (value is JsonElementArray)
			{
				WriteArray(sb, value.AsArray(), indent);
			}
		}

		private void WriteDict(StringBuilder sb, JsonElementDict el, int indent)
		{
			sb.Append("{");
			bool flag = false;
			foreach (string key in el.values.Keys)
			{
				if (flag)
				{
					sb.Append(",");
				}
				WriteDictKeyValue(sb, key, el[key], indent + 1);
				flag = true;
			}
			sb.Append("\n");
			AppendIndent(sb, indent);
			sb.Append("}");
		}

		private void WriteArray(StringBuilder sb, JsonElementArray el, int indent)
		{
			sb.Append("[");
			bool flag = false;
			foreach (JsonElement value in el.values)
			{
				if (flag)
				{
					sb.Append(",");
				}
				sb.Append("\n");
				AppendIndent(sb, indent + 1);
				if (value is JsonElementString)
				{
					WriteString(sb, value.AsString());
				}
				else if (value is JsonElementInteger)
				{
					WriteInteger(sb, value.AsInteger());
				}
				else if (value is JsonElementBoolean)
				{
					WriteBoolean(sb, value.AsBoolean());
				}
				else if (value is JsonElementDict)
				{
					WriteDict(sb, value.AsDict(), indent + 1);
				}
				else if (value is JsonElementArray)
				{
					WriteArray(sb, value.AsArray(), indent + 1);
				}
				flag = true;
			}
			sb.Append("\n");
			AppendIndent(sb, indent);
			sb.Append("]");
		}

		public void WriteToFile(string path)
		{
			File.WriteAllText(path, WriteToString());
		}

		public void WriteToStream(TextWriter tw)
		{
			tw.Write(WriteToString());
		}

		public string WriteToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			WriteDict(stringBuilder, root, 0);
			return stringBuilder.ToString();
		}
	}
}
