using UnityEngine;

public class GUIWorkshopButton : MonoBehaviour
{
	public enum WorkshopButtonFunction
	{
		None = 0,
		DeletePaintJob = 1,
		SavePaintJob = 2,
		BackToDriving = 3,
		DismissConfirmation = 4,
		BackToDrivingConfirmation = 5,
		DeletePaintJobConfirmation = 6,
		ApplyPaintJob_Currency = 11,
		HasVideoRewardPaintJob = 12,
		HasNotVideoRewardPaintJob = 13,
		DismissInfoNoVideoRewards = 14,
		OpenStore = 15,
		ScreenshotMode = 16,
		CancelScreenshot = 17,
		TakeScreenshot = 18,
		ResumeScreenshot = 19,
		MinimizeResumeScreenshot = 20,
		SubmitScreenshot = 21,
		CloseAchievementPanel = 22
	}

	public WorkshopButtonFunction function;

	private GUIWorkshopLogic guiWorkshopLogic;

	private void Start()
	{
		guiWorkshopLogic = MySingleton<Manager>.Instance.guiWorkshopLogic;
	}

	public void OnClick()
	{
		switch (function)
		{
		case WorkshopButtonFunction.DeletePaintJob:
			guiWorkshopLogic.DeletePaintJob();
			break;
		case WorkshopButtonFunction.SavePaintJob:
			guiWorkshopLogic.SaveColor();
			break;
		case WorkshopButtonFunction.BackToDriving:
			guiWorkshopLogic.BackToDriving();
			break;
		case WorkshopButtonFunction.DismissConfirmation:
			guiWorkshopLogic.DismissConfirmation();
			break;
		case WorkshopButtonFunction.BackToDrivingConfirmation:
			guiWorkshopLogic.BackToDrivingConfirmation();
			break;
		case WorkshopButtonFunction.DeletePaintJobConfirmation:
			guiWorkshopLogic.DeletePaintJobConfirmation();
			break;
		case WorkshopButtonFunction.ApplyPaintJob_Currency:
			guiWorkshopLogic.ApplyPaintJobForCurrency();
			break;
		case WorkshopButtonFunction.HasVideoRewardPaintJob:
			guiWorkshopLogic.ShowVideoRewardForPaintJob();
			break;
		case WorkshopButtonFunction.HasNotVideoRewardPaintJob:
			guiWorkshopLogic.ShowVideoNotAvailableInfo();
			break;
		case WorkshopButtonFunction.DismissInfoNoVideoRewards:
			guiWorkshopLogic.HideVideoNotAvailableInfo();
			break;
		case WorkshopButtonFunction.OpenStore:
			guiWorkshopLogic.OpenStore();
			break;
		case WorkshopButtonFunction.CloseAchievementPanel:
			guiWorkshopLogic.ActiveAchievementPanel(false);
			break;
		case WorkshopButtonFunction.ScreenshotMode:
		{
			string[] array = MySingleton<Manager>.Instance.currentLevelZoneName.ToString().Split('_');
			ManagerAnalytics.instance.FirePhotoMode(MySingleton<Manager>.Instance.guiWorkshopLogic.currentCar.GetComponent<InfoCar>().idName, "workshop", array[1]);
			GUIFacebookScreenshotMode.Instance().ScreenshotMode();
			break;
		}
		case WorkshopButtonFunction.CancelScreenshot:
			GUIFacebookScreenshotMode.Instance().CancelScreenshot();
			break;
		case WorkshopButtonFunction.TakeScreenshot:
			GUIFacebookScreenshotMode.Instance().TakeScreenshot();
			break;
		case WorkshopButtonFunction.ResumeScreenshot:
			GUIFacebookScreenshotMode.Instance().ResumeScreenshot();
			break;
		case WorkshopButtonFunction.MinimizeResumeScreenshot:
			GUIFacebookScreenshotMode.Instance().OnFacebookMinimizeResumeScreenshot();
			break;
		case WorkshopButtonFunction.SubmitScreenshot:
			GUIFacebookScreenshotMode.Instance().SubmitScreenshot();
			break;
		case (WorkshopButtonFunction)7:
		case (WorkshopButtonFunction)8:
		case (WorkshopButtonFunction)9:
		case (WorkshopButtonFunction)10:
			break;
		}
	}
}
