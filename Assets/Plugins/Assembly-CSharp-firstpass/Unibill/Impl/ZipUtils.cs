using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Unibill.Impl
{
	public class ZipUtils
	{
		public static void decompress(Stream stream, string outputPath)
		{
			ZipInputStream zipInputStream = new ZipInputStream(stream);
			ZipEntry nextEntry = zipInputStream.GetNextEntry();
			byte[] buffer = new byte[4096];
			while (nextEntry != null)
			{
				string name = nextEntry.Name;
				string path = Path.Combine(outputPath, name);
				string directoryName = Path.GetDirectoryName(path);
				if (directoryName.Length > 0)
				{
					Directory.CreateDirectory(directoryName);
				}
				if (!nextEntry.IsDirectory)
				{
					using (FileStream destination = File.Create(path))
					{
						Copy(zipInputStream, destination, buffer);
					}
				}
				nextEntry = zipInputStream.GetNextEntry();
			}
		}

		private static void Copy(Stream source, Stream destination, byte[] buffer)
		{
			bool flag = true;
			while (flag)
			{
				int num = source.Read(buffer, 0, buffer.Length);
				if (num > 0)
				{
					destination.Write(buffer, 0, num);
					continue;
				}
				destination.Flush();
				flag = false;
			}
		}
	}
}
