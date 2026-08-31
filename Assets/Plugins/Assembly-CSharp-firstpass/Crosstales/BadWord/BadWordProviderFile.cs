using UnityEngine;

namespace Crosstales.BadWord
{
	public class BadWordProviderFile : BadWordProvider
	{
		public string FilePathWindows = string.Empty;

		public string FilePathOSX = string.Empty;

		public string FilePathLinux = string.Empty;

		public BadWordFile[] Resources;

		private string filePath = string.Empty;

		public override void Load()
		{
			Debug.LogWarning("The current platform is not supported!");
		}

		public override void Save()
		{
			Debug.LogWarning("Save not implemented!");
		}
	}
}
