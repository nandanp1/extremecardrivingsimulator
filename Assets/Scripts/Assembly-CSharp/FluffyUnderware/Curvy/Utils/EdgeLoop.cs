using System;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Utils
{
	[Serializable]
	internal class EdgeLoop
	{
		public int[] vertexIndex;

		public int vertexCount
		{
			get
			{
				return vertexIndex.Length - 1;
			}
		}

		public int TriIndexCount
		{
			get
			{
				return vertexCount * 6;
			}
		}

		public EdgeLoop(List<int> verts)
		{
			vertexIndex = verts.ToArray();
		}

		public void ShiftIndices(int by)
		{
			for (int i = 0; i < vertexIndex.Length; i++)
			{
				vertexIndex[i] += by;
			}
		}

		public override string ToString()
		{
			string text = string.Empty;
			int[] array = vertexIndex;
			foreach (int num in array)
			{
				text = text + num + ", ";
			}
			return text;
		}
	}
}
