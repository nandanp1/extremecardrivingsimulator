using System;
using System.Collections.Generic;
using UnityEngine;

public static class Localization
{
	public delegate byte[] LoadFunction(string path);

	public static LoadFunction loadFunction;

	public static bool localizationHasBeenSet = false;

	private static string[] mLanguages = null;

	private static Dictionary<string, string> mOldDictionary = new Dictionary<string, string>();

	private static Dictionary<string, string[]> mDictionary = new Dictionary<string, string[]>();

	private static int mLanguageIndex = -1;

	private static string mLanguage;

	public static Dictionary<string, string[]> dictionary
	{
		get
		{
			if (!localizationHasBeenSet)
			{
				language = PlayerPrefs.GetString("Language", "English");
			}
			return mDictionary;
		}
		set
		{
			localizationHasBeenSet = value != null;
			mDictionary = value;
		}
	}

	public static string[] knownLanguages
	{
		get
		{
			if (!localizationHasBeenSet)
			{
				LoadDictionary(PlayerPrefs.GetString("Language", "English"));
			}
			return mLanguages;
		}
	}

	public static string language
	{
		get
		{
			if (string.IsNullOrEmpty(mLanguage))
			{
				string[] array = knownLanguages;
				mLanguage = PlayerPrefs.GetString("Language", (array == null) ? "English" : array[0]);
				LoadAndSelect(mLanguage);
			}
			return mLanguage;
		}
		set
		{
			if (mLanguage != value)
			{
				mLanguage = value;
				LoadAndSelect(value);
			}
		}
	}

	[Obsolete("Localization is now always active. You no longer need to check this property.")]
	public static bool isActive
	{
		get
		{
			return true;
		}
	}

	private static bool LoadDictionary(string value)
	{
		byte[] array = null;
		if (!localizationHasBeenSet)
		{
			if (loadFunction == null)
			{
				TextAsset textAsset = Resources.Load<TextAsset>("Localization");
				if (textAsset != null)
				{
					array = textAsset.bytes;
				}
			}
			else
			{
				array = loadFunction("Localization");
			}
			localizationHasBeenSet = true;
		}
		if (LoadCSV(array))
		{
			return true;
		}
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (loadFunction == null)
		{
			TextAsset textAsset2 = Resources.Load<TextAsset>(value);
			if (textAsset2 != null)
			{
				array = textAsset2.bytes;
			}
		}
		else
		{
			array = loadFunction(value);
		}
		if (array != null)
		{
			Set(value, array);
			return true;
		}
		return false;
	}

	private static bool LoadAndSelect(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			if (mDictionary.Count == 0 && !LoadDictionary(value))
			{
				return false;
			}
			if (SelectLanguage(value))
			{
				return true;
			}
		}
		if (mOldDictionary.Count > 0)
		{
			return true;
		}
		mOldDictionary.Clear();
		mDictionary.Clear();
		if (string.IsNullOrEmpty(value))
		{
			PlayerPrefs.DeleteKey("Language");
		}
		return false;
	}

	public static void Load(TextAsset asset)
	{
		ByteReader byteReader = new ByteReader(asset);
		Set(asset.name, byteReader.ReadDictionary());
	}

	public static void Set(string languageName, byte[] bytes)
	{
		ByteReader byteReader = new ByteReader(bytes);
		Set(languageName, byteReader.ReadDictionary());
	}

	public static bool LoadCSV(TextAsset asset, bool merge = false)
	{
		return LoadCSV(asset.bytes, asset, merge);
	}

	public static bool LoadCSV(byte[] bytes, bool merge = false)
	{
		return LoadCSV(bytes, null, merge);
	}

	private static bool LoadCSV(byte[] bytes, TextAsset asset, bool merge = false)
	{
		if (bytes == null)
		{
			return false;
		}
		ByteReader byteReader = new ByteReader(bytes);
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 2)
		{
			return false;
		}
		if (!merge || betterList.size - 1 != mLanguage.Length)
		{
			merge = false;
			mDictionary.Clear();
		}
		betterList[0] = "KEY";
		mLanguages = new string[betterList.size - 1];
		for (int i = 0; i < mLanguages.Length; i++)
		{
			mLanguages[i] = betterList[i + 1];
		}
		while (betterList != null)
		{
			AddCSV(betterList, !merge);
			betterList = byteReader.ReadCSV();
		}
		return true;
	}

	private static bool SelectLanguage(string language)
	{
		mLanguageIndex = -1;
		if (mDictionary.Count == 0)
		{
			return false;
		}
		string[] value;
		if (mDictionary.TryGetValue("KEY", out value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == language)
				{
					mOldDictionary.Clear();
					mLanguageIndex = i;
					mLanguage = language;
					PlayerPrefs.SetString("Language", mLanguage);
					UIRoot.Broadcast("OnLocalize");
					return true;
				}
			}
		}
		return false;
	}

	private static void AddCSV(BetterList<string> values, bool warnOnDuplicate = true)
	{
		if (values.size < 2)
		{
			return;
		}
		string text = values[0];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string[] array = new string[values.size - 1];
		for (int i = 1; i < values.size; i++)
		{
			array[i - 1] = values[i];
		}
		if (mDictionary.ContainsKey(text))
		{
			mDictionary[text] = array;
			if (warnOnDuplicate)
			{
				Debug.LogWarning("Localization key '" + text + "' is already present");
			}
			return;
		}
		try
		{
			mDictionary.Add(text, array);
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to add '" + text + "' to the Localization dictionary.\n" + ex.Message);
		}
	}

	public static void Set(string languageName, Dictionary<string, string> dictionary)
	{
		mLanguage = languageName;
		PlayerPrefs.SetString("Language", mLanguage);
		mOldDictionary = dictionary;
		localizationHasBeenSet = false;
		mLanguageIndex = -1;
		mLanguages = new string[1] { languageName };
		UIRoot.Broadcast("OnLocalize");
	}

	public static string Get(string key)
	{
		if (!localizationHasBeenSet)
		{
			language = PlayerPrefs.GetString("Language", "English");
		}
		string key2 = key + " Mobile";
		string[] value;
		string value2;
		if (mLanguageIndex != -1 && mDictionary.TryGetValue(key2, out value))
		{
			if (mLanguageIndex < value.Length)
			{
				return value[mLanguageIndex];
			}
		}
		else if (mOldDictionary.TryGetValue(key2, out value2))
		{
			return value2;
		}
		if (mLanguageIndex != -1 && mDictionary.TryGetValue(key, out value))
		{
			if (mLanguageIndex < value.Length)
			{
				return value[mLanguageIndex];
			}
		}
		else if (mOldDictionary.TryGetValue(key, out value2))
		{
			return value2;
		}
		return key;
	}

	public static string Format(string key, params object[] parameters)
	{
		return string.Format(Get(key), parameters);
	}

	[Obsolete("Use Localization.Get instead")]
	public static string Localize(string key)
	{
		return Get(key);
	}

	public static bool Exists(string key)
	{
		if (!localizationHasBeenSet)
		{
			language = PlayerPrefs.GetString("Language", "English");
		}
		string key2 = key + " Mobile";
		if (mDictionary.ContainsKey(key2))
		{
			return true;
		}
		if (mOldDictionary.ContainsKey(key2))
		{
			return true;
		}
		return mDictionary.ContainsKey(key) || mOldDictionary.ContainsKey(key);
	}
}
