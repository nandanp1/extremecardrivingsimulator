using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MySingleton<AchievementManager>
{
	public enum AchievementCategory
	{
		MULTIPLAYER = 0,
		GENERAL = 1
	}

	public enum Achievement
	{
		MULTIPLAYER_PLAY_FACEBOOK_FRIEND = 0,
		MULTIPLAYER_PLAY_FACEBOOK_FRIEND_FULL_ROOM = 1,
		GENERAL_SHARE_SCREENSHOT_CAR_ON_FACEBOOK = 2
	}

	public delegate void AchievementCategoryCompletedHandler(AchievementCategory achievementCategory);

	public GameObject prefabPanelAchievement;

	private GameObject instantiatedPanelAchievement;

	public Dictionary<Achievement, string> internDescriptionDictionary = new Dictionary<Achievement, string>();

	public Dictionary<Achievement, int> internMoneyDictionary = new Dictionary<Achievement, int>();

	public Dictionary<AchievementCategory, bool> categoriesCompleted = new Dictionary<AchievementCategory, bool>();

	public Dictionary<AchievementCategory, List<Achievement>> achievementsByCategory = new Dictionary<AchievementCategory, List<Achievement>>();

	public Dictionary<AchievementCategory, string> categoryNameDictionary { get; set; }

	public Dictionary<Achievement, int> multiplayerAchievementDictionaryMoney { get; set; }

	public Dictionary<Achievement, string> multiplayerAchievementDictionaryDescription { get; set; }

	public Dictionary<Achievement, int> generalAchievementDictionaryMoney { get; set; }

	public Dictionary<Achievement, string> generalAchievementDictionaryDescription { get; set; }

	public event AchievementCategoryCompletedHandler AchievementCategoryCompleted;

	public void Start()
	{
		categoryNameDictionary = new Dictionary<AchievementCategory, string>();
		multiplayerAchievementDictionaryMoney = new Dictionary<Achievement, int>();
		multiplayerAchievementDictionaryDescription = new Dictionary<Achievement, string>();
		generalAchievementDictionaryMoney = new Dictionary<Achievement, int>();
		generalAchievementDictionaryDescription = new Dictionary<Achievement, string>();
		InitAchievementsCategoryNames();
		InitMultiplayerAchievementDictionaryMoney();
		InitMultiplayerAchievementDictionaryDescription();
		InitGeneralAchievementDictionaryMoney();
		InitGeneralAchievementDictionaryDescription();
		InitInternMoneyDictionary();
		InitNumAchievementsCompleted();
		InitAchievementsByCategory();
	}

	private void InitNumAchievementsCompleted()
	{
		foreach (int value in Enum.GetValues(typeof(AchievementCategory)))
		{
			categoriesCompleted.Add((AchievementCategory)value, false);
		}
	}

	private void InitAchievementsByCategory()
	{
		List<Achievement> list = new List<Achievement>();
		List<Achievement> list2 = new List<Achievement>();
		foreach (int value in Enum.GetValues(typeof(Achievement)))
		{
			switch (GetCategory((Achievement)value))
			{
			case AchievementCategory.GENERAL:
				list.Add((Achievement)value);
				break;
			case AchievementCategory.MULTIPLAYER:
				list2.Add((Achievement)value);
				break;
			}
		}
		achievementsByCategory[AchievementCategory.GENERAL] = list;
		achievementsByCategory[AchievementCategory.MULTIPLAYER] = list2;
	}

	private void InitInternMoneyDictionary()
	{
		foreach (KeyValuePair<Achievement, int> item in multiplayerAchievementDictionaryMoney)
		{
			internMoneyDictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<Achievement, string> item2 in multiplayerAchievementDictionaryDescription)
		{
			internDescriptionDictionary.Add(item2.Key, item2.Value);
		}
		foreach (KeyValuePair<Achievement, int> item3 in generalAchievementDictionaryMoney)
		{
			internMoneyDictionary.Add(item3.Key, item3.Value);
		}
		foreach (KeyValuePair<Achievement, string> item4 in generalAchievementDictionaryDescription)
		{
			internDescriptionDictionary.Add(item4.Key, item4.Value);
		}
	}

	private void InitAchievementsCategoryNames()
	{
		categoryNameDictionary[AchievementCategory.GENERAL] = "general_achievements_category";
		categoryNameDictionary[AchievementCategory.MULTIPLAYER] = "multiplayer_achievements_category";
	}

	private void InitMultiplayerAchievementDictionaryMoney()
	{
		multiplayerAchievementDictionaryMoney[Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND] = 500;
		multiplayerAchievementDictionaryMoney[Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND_FULL_ROOM] = 2000;
	}

	private void InitMultiplayerAchievementDictionaryDescription()
	{
		multiplayerAchievementDictionaryDescription[Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND] = "multiplayer_facebook_friend";
		multiplayerAchievementDictionaryDescription[Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND_FULL_ROOM] = "multiplayer_facebook_friend_full_room";
	}

	private void InitGeneralAchievementDictionaryMoney()
	{
		generalAchievementDictionaryMoney[Achievement.GENERAL_SHARE_SCREENSHOT_CAR_ON_FACEBOOK] = 500;
	}

	private void InitGeneralAchievementDictionaryDescription()
	{
		generalAchievementDictionaryDescription[Achievement.GENERAL_SHARE_SCREENSHOT_CAR_ON_FACEBOOK] = "screenshot_car_facebook";
	}

	public bool ApplyAchivement(Achievement achievement)
	{
		return false;
	}

	private void CheckIfCategoryCompleted(Achievement achievement)
	{
		AchievementCategory category = GetCategory(achievement);
		List<Achievement> list = achievementsByCategory[category];
		int num = 0;
		foreach (Achievement item in list)
		{
			if (!CanClaimReward(item))
			{
				num++;
			}
		}
		if (num >= list.Count)
		{
			categoriesCompleted[category] = true;
			AchievementCategoryCompleted(category);
			SettingsManager.SetAchievementCategory(category.ToString());
		}
	}

	public bool IsCategoryCompleted(AchievementCategory category)
	{
		return SettingsManager.GetAchievementCategory(category.ToString()) == 1;
	}

	private void ManagePnAchievement()
	{
		if (instantiatedPanelAchievement == null)
		{
			instantiatedPanelAchievement = (GameObject)UnityEngine.Object.Instantiate(prefabPanelAchievement);
		}
		else
		{
			NGUITools.SetActive(instantiatedPanelAchievement, true);
		}
		Invoke("HideAchievementPanel", 4f);
	}

	private void HideAchievementPanel()
	{
		NGUITools.SetActive(instantiatedPanelAchievement, false);
	}

	public bool CanClaimReward(Achievement achievementType)
	{
		return SettingsManager.GetAchievement(achievementType.ToString()) == 0;
	}

	public AchievementCategory GetCategory(Achievement achievement)
	{
		AchievementCategory result = AchievementCategory.GENERAL;
		if (achievement.ToString().StartsWith("MULTIPLAYER_"))
		{
			result = AchievementCategory.MULTIPLAYER;
		}
		return result;
	}
}
