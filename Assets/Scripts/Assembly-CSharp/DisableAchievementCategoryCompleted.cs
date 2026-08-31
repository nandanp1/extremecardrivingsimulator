using UnityEngine;

public class DisableAchievementCategoryCompleted : MonoBehaviour
{
	public AchievementManager.AchievementCategory categoryToCheck;

	private void Start()
	{
		MySingleton<AchievementManager>.Instance.AchievementCategoryCompleted += AchievementCategoryCompleted;
		if (MySingleton<AchievementManager>.Instance.IsCategoryCompleted(categoryToCheck))
		{
			base.gameObject.SetActive(false);
		}
	}

	private void AchievementCategoryCompleted(AchievementManager.AchievementCategory category)
	{
		if (category == categoryToCheck)
		{
			base.gameObject.SetActive(false);
		}
	}
}
