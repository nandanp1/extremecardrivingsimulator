using System;
using UnityEngine;

public class AndroidSocialGate
{
	public static void StartShareIntent(string caption, string message, string packageNamePattern = "")
	{
		StartShareIntentWithSubject(caption, message, string.Empty, packageNamePattern);
	}

	public static void StartShareIntentWithSubject(string caption, string message, string subject, string packageNamePattern = "")
	{
		AndroidNative.StartShareIntent(caption, message, subject, packageNamePattern);
	}

	public static void StartShareIntent(string caption, string message, Texture2D texture, string packageNamePattern = "")
	{
		StartShareIntentWithSubject(caption, message, string.Empty, texture, packageNamePattern);
	}

	public static void StartShareIntentWithSubject(string caption, string message, string subject, Texture2D texture, string packageNamePattern = "")
	{
		byte[] inArray = texture.EncodeToPNG();
		string media = Convert.ToBase64String(inArray);
		AndroidNative.StartShareIntent(caption, message, subject, media, packageNamePattern);
	}
}
