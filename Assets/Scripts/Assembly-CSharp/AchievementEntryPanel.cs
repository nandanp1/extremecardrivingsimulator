using UnityEngine;

public class AchievementEntryPanel : MonoBehaviour
{
	public UILabel description;

	public UILabel moneyAmount;

	public GameObject pending;

	public GameObject tick;

	public UILocalize descriptionLocalize;

	public UIWidget backgroundEntry;

	private AchievementManager.Achievement achievementType;

	public void Initialize(string description, int moneyAmount, AchievementManager.Achievement achievementType)
	{
		descriptionLocalize.key = description;
		this.moneyAmount.text = moneyAmount.ToString();
		if (SettingsManager.GetAchievement(achievementType.ToString()) == 1)
		{
			NGUITools.SetActive(pending, false);
			NGUITools.SetActive(tick, true);
		}
		else
		{
			NGUITools.SetActive(pending, true);
			NGUITools.SetActive(tick, false);
		}
	}

	public void ReAllocateEntry(int i)
	{
		MonoBehaviour.print("Cambiando anclas");
		backgroundEntry.bottomAnchor.absolute -= i * 53;
		backgroundEntry.topAnchor.absolute -= i * 53;
	}
}
