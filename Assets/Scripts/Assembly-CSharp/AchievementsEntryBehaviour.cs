using UnityEngine;

public class AchievementsEntryBehaviour : MonoBehaviour
{
	public GameObject complete;

	public GameObject incomplete;

	public UILabel description;

	public UILabel reward;

	public void Init(AchievementsEntryInfo info)
	{
		if (info.completed)
		{
			incomplete.SetActive(false);
			complete.SetActive(true);
		}
		else
		{
			incomplete.SetActive(true);
			complete.SetActive(false);
		}
		description.text = info.description;
		reward.text = string.Empty + info.reward;
	}
}
