using System.Collections.Generic;
using UnityEngine;

public class AchievementsPanel : MonoBehaviour
{
	public UIGrid grid;

	public UIScrollView scrollView;

	public GameObject achievementEntryPrefab;

	public UILocalize achievementCategoryLocalize;

	public void Start()
	{
	}

	public void Init(AchievementManager.AchievementCategory achievementCategory)
	{
		switch (achievementCategory)
		{
		case AchievementManager.AchievementCategory.GENERAL:
			achievementCategoryLocalize.key = "general_achievements_category";
			LoadGeneralAchievements();
			break;
		case AchievementManager.AchievementCategory.MULTIPLAYER:
			achievementCategoryLocalize.key = "multiplayer_achievements_category";
			LoadMultiplayerAchievements();
			break;
		}
		base.gameObject.SetActive(true);
	}

	public void LoadMultiplayerAchievements()
	{
		ClearAllEntries();
		Dictionary<AchievementManager.Achievement, int> multiplayerAchievementDictionaryMoney = MySingleton<AchievementManager>.Instance.multiplayerAchievementDictionaryMoney;
		Dictionary<AchievementManager.Achievement, string> multiplayerAchievementDictionaryDescription = MySingleton<AchievementManager>.Instance.multiplayerAchievementDictionaryDescription;
		int num = 0;
		foreach (KeyValuePair<AchievementManager.Achievement, string> item in multiplayerAchievementDictionaryDescription)
		{
			AchievementEntryPanel component = NGUITools.AddChild(grid.gameObject, achievementEntryPrefab).GetComponent<AchievementEntryPanel>();
			component.enabled = true;
			component.gameObject.SetActive(true);
			component.Initialize(item.Value, multiplayerAchievementDictionaryMoney[item.Key], item.Key);
			component.ReAllocateEntry(num);
			num++;
		}
		scrollView.ResetPosition();
	}

	public void LoadGeneralAchievements()
	{
		ClearAllEntries();
		Dictionary<AchievementManager.Achievement, int> generalAchievementDictionaryMoney = MySingleton<AchievementManager>.Instance.generalAchievementDictionaryMoney;
		Dictionary<AchievementManager.Achievement, string> generalAchievementDictionaryDescription = MySingleton<AchievementManager>.Instance.generalAchievementDictionaryDescription;
		int num = 0;
		foreach (KeyValuePair<AchievementManager.Achievement, string> item in generalAchievementDictionaryDescription)
		{
			AchievementEntryPanel component = NGUITools.AddChild(grid.gameObject, achievementEntryPrefab).GetComponent<AchievementEntryPanel>();
			component.enabled = true;
			component.gameObject.SetActive(true);
			component.Initialize(item.Value, generalAchievementDictionaryMoney[item.Key], item.Key);
			component.ReAllocateEntry(num);
			num++;
		}
		scrollView.ResetPosition();
	}

	private void ClearAllEntries()
	{
		scrollView.ResetPosition();
		foreach (Transform child in grid.GetChildList())
		{
			grid.RemoveChild(child);
			Object.DestroyImmediate(child.gameObject);
		}
		scrollView.ResetPosition();
	}
}
