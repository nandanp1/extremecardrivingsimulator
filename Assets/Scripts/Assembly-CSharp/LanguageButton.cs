using System;
using UnityEngine;

public class LanguageButton : MonoBehaviour
{
	public enum Type
	{
		Flag = 0,
		Title = 1,
		Store = 2
	}

	private const string PREFIX_SPRITE = "flag_";

	private const string PREFIX_TITLE = "Sp_LabelTitle_";

	private const string PREFIX_TITLE_STORE = "Sp_LabelTitleStore_";

	public UISprite spriteAux;

	public Type type;

	public UISprite spLanguage;

	private string[] languagesSupported;

	private int currentIndex;

	private void OnEnable()
	{
		languagesSupported = Enum.GetNames(typeof(SettingsManager.Language));
		string text = MySingleton<Manager>.Instance.language.ToString();
		int num = languagesSupported.Length;
		for (int i = 0; i != num; i++)
		{
			if (text == languagesSupported[i])
			{
				currentIndex = i;
				break;
			}
		}
		UpdateGUI();
	}

	private void OnClick()
	{
		UpdateLogic();
		UpdateGUI();
		MySingleton<Manager>.Instance.guiCurrentLogic.UpdateLocalizeLabels();
		if ((bool)spriteAux)
		{
			spriteAux.spriteName = "Sp_LabelTitle_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower();
		}
	}

	private void UpdateLogic()
	{
		currentIndex++;
		string text = languagesSupported[currentIndex % languagesSupported.Length];
		SettingsManager.Language language = (SettingsManager.Language)(int)Enum.Parse(typeof(SettingsManager.Language), text, true);
		SettingsManager.SetLanguage(language);
		Localization.language = text;
	}

	private void UpdateGUI()
	{
		string empty = string.Empty;
		switch (type)
		{
		case Type.Flag:
			empty = "flag_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower();
			break;
		case Type.Title:
			empty = "Sp_LabelTitle_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower();
			break;
		case Type.Store:
		{
			string text = languagesSupported[currentIndex % languagesSupported.Length].ToLower();
			empty = ((!(text == "english")) ? ("Sp_LabelTitle_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower()) : ("Sp_LabelTitleStore_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower()));
			break;
		}
		default:
			empty = "flag_" + languagesSupported[currentIndex % languagesSupported.Length].ToLower();
			break;
		}
		spLanguage.spriteName = empty;
	}
}
