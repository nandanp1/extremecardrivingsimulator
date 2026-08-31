using UnityEngine;

namespace Crosstales.BadWord
{
	public class DomainProviderFile : DomainProvider
	{
		public string FilePathWindows = string.Empty;

		public string FilePathOSX = string.Empty;

		public string FilePathLinux = string.Empty;

		public DomainFile[] Resources;

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
