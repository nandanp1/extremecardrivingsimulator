using System.Collections.Generic;
using UnityEngine;

public class AndroidNativeSettings : ScriptableObject
{
	public const string VERSION_NUMBER = "3.8";

	private const string ISNSettingsAssetName = "AndroidNativeSettings";

	private const string ISNSettingsPath = "Extensions/AndroidNative/Resources";

	private const string ISNSettingsAssetExtension = ".asset";

	public bool EnableGamesAPI = true;

	public bool EnableAppStateAPI = true;

	public bool LoadProfileIcons = true;

	public bool LoadProfileImages = true;

	public string base64EncodedPublicKey = "REPLACE_WITH_YOUR_PUBLIC_KEY";

	public List<string> InAppProducts = new List<string>();

	private static AndroidNativeSettings instance;

	public static AndroidNativeSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load("AndroidNativeSettings") as AndroidNativeSettings;
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<AndroidNativeSettings>();
				}
			}
			return instance;
		}
	}

	public bool IsBase64KeyWasReplaced
	{
		get
		{
			if (base64EncodedPublicKey.Equals("REPLACE_WITH_YOUR_PUBLIC_KEY"))
			{
				return false;
			}
			return true;
		}
	}
}
