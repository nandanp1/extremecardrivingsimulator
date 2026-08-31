using System;
using System.Text;
using UnityEngine;

namespace Crosstales.BadWord
{
	[Serializable]
	public abstract class Source
	{
		public string Name = string.Empty;

		public string Description = string.Empty;

		public Sprite Icon;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(GetType().Name + " {");
			stringBuilder.Append("Name='" + Name + "',");
			stringBuilder.Append("Description='" + Description + "',");
			stringBuilder.Append(string.Concat("Icon='", Icon, "'"));
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
	}
}
