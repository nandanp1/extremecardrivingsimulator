using UnityEngine;

public class LanguageToggleButton : MonoBehaviour
{
	public SettingsManager.Language language;

	private UIButton button;

	private void OnEnable()
	{
		button = GetComponent<UIButton>();
		MySingleton<Manager>.Instance.allLanguages.Add(this);
		UpdateSprite();
	}

	private void OnDisable()
	{
		if ((bool)MySingleton<Manager>.Instance)
		{
			MySingleton<Manager>.Instance.allLanguages.Remove(this);
		}
	}

	private void OnClick()
	{
		SettingsManager.SetLanguage(language);
		Localization.language = language.ToString();
		UpdateLanguagesSprites();
	}

	private void UpdateSprite()
	{
		bool flag = MySingleton<Manager>.Instance.language == language;
		Color defaultColor = button.defaultColor;
		defaultColor.a = ((!flag) ? 0.3f : 1f);
		button.defaultColor = defaultColor;
	}

	private void UpdateLanguagesSprites()
	{
		MySingleton<Manager>.Instance.allLanguages.ForEach((LanguageToggleButton language) =>
		{
			language.UpdateSprite();
		});
	}
}
