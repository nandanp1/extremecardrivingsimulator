using System;
using UnityEngine;

namespace AxesInMotion.UnityHelpers
{
	public class ImageHelper
	{
		public static byte[] GetBytesImage(Texture2D texture)
		{
			return GetBytesTexture2DImage(texture);
		}

		public static byte[] GetBytesScreenshotImage()
		{
			return GetBytesTexture2DImage(GetScreenshotAsTexture());
		}

		public static byte[] GetBytesBase64StringImage(string base64StringImage)
		{
			return Convert.FromBase64String(base64StringImage);
		}

		public static byte[] GetBytesImageResources(string path)
		{
			Texture2D texture = (Texture2D)Resources.Load(path, typeof(Texture2D));
			return GetBytesTexture2DImage(texture);
		}

		public static byte[] GetBytesImageURL(string url, float secondsTimeout = 0f)
		{
			float time = Time.time;
			WWW wWW = new WWW(url);
			while (!wWW.isDone)
			{
				if (secondsTimeout > 0f && Time.time - time < secondsTimeout)
				{
					return null;
				}
			}
			return GetBytesTexture2DImage(wWW.texture);
		}

		public static byte[] GetBytesTexture2DImage(Texture2D texture)
		{
			return texture.EncodeToPNG();
		}

		public static Texture2D GetScreenshotAsTexture()
		{
			int width = Screen.width;
			int height = Screen.height;
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
			texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			texture2D.anisoLevel = 9;
			texture2D.filterMode = FilterMode.Point;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.Apply();
			return texture2D;
		}
	}
}
