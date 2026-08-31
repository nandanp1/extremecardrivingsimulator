using UnityEngine;

public class RewardPaintjob : RewardItem
{
	public override void GiveReward()
	{
		GUIWorkshopLogic gUIWorkshopLogic = (GUIWorkshopLogic)MySingleton<Manager>.Instance.guiCurrentLogic;
		if (gUIWorkshopLogic != null)
		{
			gUIWorkshopLogic.SaveColor();
			gUIWorkshopLogic.BackToDriving();
		}
		else
		{
			Debug.LogError("Paintjobs can only be given in the workshop.");
		}
	}
}
