using Unibill;
using UnityEngine;

public class GUIWorkshopLogic : GUILogic, GUIFacebookScreenshotModeExtension
{
	private enum WorkshopConfimationMessage
	{
		None = 0,
		DeletePaintJob = 1,
		UnappliedPaintJob = 2
	}

	[HideInInspector]
	public BackButtonWorkshop backButton;

	[SerializeField]
	private UIPanel pnConfirmation;

	[SerializeField]
	private UIPanel pnNoVideoInfo;

	[SerializeField]
	private UILabel lbConfirmationTitle;

	[SerializeField]
	private UILabel lbConfirmationText;

	[SerializeField]
	private UILabel lbDistance;

	[SerializeField]
	private UILabel lbDistanceUnit;

	[SerializeField]
	private UILabel lbCredits;

	[SerializeField]
	public UILabel lbGameTitleFacebook;

	[SerializeField]
	private GUIWorkshopButton btYes;

	[SerializeField]
	private GUIWorkshopButton btVideoRewardForJob;

	[SerializeField]
	private Color defaultColor;

	[SerializeField]
	private Color currentColor;

	public Color pickerColor;

	private InfoCar infoCar;

	[HideInInspector]
	public GameObject currentCar;

	[SerializeField]
	public Transform carContainer;

	[SerializeField]
	private UIPanel pnWorkshop;

	private bool firstTime = true;

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiWorkshopLogic = this;
		MySingleton<Manager>.Instance.returningFromWorkshop = true;
		if (backButton == null)
		{
			backButton = base.gameObject.AddComponent<BackButtonWorkshop>();
		}
		AwakeFacebookScreenshotMode();
		UpdateLabels();
		LoadCar();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateLabels();
		OnVideoRewardsAvailabilityChanged(MySingleton<Manager>.Instance.ads.heyzapController.IsVideoRewardAvailable());
	}

	private void UpdateLabels()
	{
		lbDistance.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, false, true);
		lbDistanceUnit.text = Converter.GetDistanceUnit();
		lbCredits.text = Unibiller.GetCurrencyBalance("credits").ToString();
	}

	public override void UpdateLocalizeLabels()
	{
	}

	public void LoadCar()
	{
		this.infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar)];
		GameObject original = Resources.Load(this.infoCar.idName) as GameObject;
		currentCar = Object.Instantiate(original, carContainer.position, carContainer.rotation) as GameObject;
		InfoCar infoCar = currentCar.AddMissingComponent<InfoCar>();
		infoCar.Initialize(this.infoCar);
		currentCar.GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)10;
		Camera[] componentsInChildren = currentCar.GetComponentsInChildren<Camera>();
		Camera[] array = componentsInChildren;
		foreach (Camera camera in array)
		{
			camera.gameObject.SetActive(false);
		}
		LoadColor();
	}

	public void LoadColor()
	{
		defaultColor = infoCar.defaultColor;
		currentColor = SettingsManager.GetCarColor(infoCar.id, defaultColor);
		SetCarMaterialColor(currentColor);
		pickerColor = currentColor;
	}

	public void SaveColor()
	{
		currentColor = pickerColor;
		SetCarMaterialColor(currentColor);
		SettingsManager.SetCarColor(infoCar.id, currentColor);
	}

	public void DeletePaintJob()
	{
		pickerColor = defaultColor;
		currentColor = defaultColor;
		SetCarMaterialColor(defaultColor);
		SettingsManager.SetCarColor(infoCar.id, defaultColor);
		DismissConfirmation();
	}

	public void BackToDriving()
	{
		ShowLoading();
		SetCarMaterialColor(currentColor);
		Invoke("BackToSceneGame", 0.7f);
	}

	private void BackToSceneGame()
	{
		LegacySceneManager.LoadLevel(2);
	}

	private void ShowLoading()
	{
		Object.Instantiate(MySingleton<Manager>.Instance.UIRootLoading);
	}

	public void ShowVideoNotAvailableInfo()
	{
		NGUITools.SetActive(pnNoVideoInfo.gameObject, true);
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.DismissInfoNoVideoRewards;
	}

	public void HideVideoNotAvailableInfo()
	{
		NGUITools.SetActive(pnNoVideoInfo.gameObject, false);
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.BackToDrivingConfirmation;
	}

	public void OnColorChange()
	{
		if (!firstTime)
		{
			pickerColor = UIColorPicker.current.value;
			SetCarMaterialColor(pickerColor);
		}
		firstTime = false;
	}

	private void SetCarMaterialColor(Color color)
	{
		CarVisuals component = currentCar.GetComponent<CarVisuals>();
		component.Body.GetComponent<Renderer>().material.color = color;
		if (component.paintableLODMesh != null && component.paintableLODMesh.Length > 0)
		{
			for (int i = 0; i < component.paintableLODMesh.Length; i++)
			{
				component.paintableLODMesh[i].material.color = color;
			}
		}
		Resources.UnloadUnusedAssets();
	}

	public void ApplyPaintJobForCurrency()
	{
		if (Unibiller.GetCurrencyBalance("credits") >= 50m)
		{
			ManagerAnalytics.instance.FireCustomization(MySingleton<Manager>.Instance.guiWorkshopLogic.currentCar.GetComponent<InfoCar>().idName, "currency");
			Unibiller.DebitBalance("credits", 50m);
			SaveColor();
			BackToDriving();
		}
		else
		{
			OpenStore();
		}
	}

	public void ShowVideoRewardForPaintJob()
	{
		if (MySingleton<Manager>.Instance.ads.heyzapController.IsVideoRewardAvailable())
		{
			ManagerAnalytics.instance.FireCustomization(MySingleton<Manager>.Instance.guiWorkshopLogic.currentCar.GetComponent<InfoCar>().idName, "video");
			ManagerAnalytics.instance.FireRewardedVideo("workshop");
			MySingleton<Manager>.Instance.ads.heyzapController.ShowVideoReward(new Task(TaskManager.TaskId.WatchVideoForPaintjob.ToString(), new RewardPaintjob()));
		}
	}

	private void ShowConfirmation(WorkshopConfimationMessage message)
	{
		switch (message)
		{
		case WorkshopConfimationMessage.DeletePaintJob:
			lbConfirmationTitle.text = Localization.Get("workshop_delete_paintjob_confirmation_title");
			lbConfirmationText.text = Localization.Get("workshop_delete_paintjob_confirmation_text");
			btYes.function = GUIWorkshopButton.WorkshopButtonFunction.DeletePaintJob;
			break;
		case WorkshopConfimationMessage.UnappliedPaintJob:
			lbConfirmationTitle.text = Localization.Get("workshop_unapplied_paintjob_confirmation_title");
			lbConfirmationText.text = Localization.Get("workshop_unapplied_paintjob_confirmation_text");
			btYes.function = GUIWorkshopButton.WorkshopButtonFunction.BackToDriving;
			break;
		}
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.DismissConfirmation;
		NGUITools.SetActive(pnConfirmation.gameObject, true);
	}

	public void DismissConfirmation()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.BackToDrivingConfirmation;
		NGUITools.SetActive(pnConfirmation.gameObject, false);
	}

	public void BackToDrivingConfirmation()
	{
		if (HasChangesForApply())
		{
			ShowConfirmation(WorkshopConfimationMessage.UnappliedPaintJob);
		}
		else
		{
			BackToDriving();
		}
	}

	private bool HasChangesForApply()
	{
		return currentColor != pickerColor;
	}

	public void DeletePaintJobConfirmation()
	{
		ShowConfirmation(WorkshopConfimationMessage.DeletePaintJob);
	}

	public override void PurchasesRestored(bool success)
	{
	}

	public override void PurchaseCompleted(PurchaseEvent purchase)
	{
	}

	public override void PurchaseCanceled(PurchasableItem item)
	{
	}

	public override void PurchaseFailed(PurchasableItem item)
	{
	}

	public override void BillerInitialized(UnibillState state)
	{
	}

	public override void OnVideoRewardsFinished(Task task)
	{
		if (task.IsComplete())
		{
			task.reward.GiveReward();
		}
		OnVideoRewardsAvailabilityChanged(false);
		MySingleton<Manager>.Instance.ads.heyzapController.currentVideoAvailableStatus = false;
	}

	public override void OnVideoRewardsAvailabilityChanged(bool isAvailable)
	{
		SetBtVideoRewardActive(isAvailable);
	}

	private void SetBtVideoRewardActive(bool isVideoAvailable)
	{
		if (isVideoAvailable)
		{
			btVideoRewardForJob.function = GUIWorkshopButton.WorkshopButtonFunction.HasVideoRewardPaintJob;
		}
		else
		{
			btVideoRewardForJob.function = GUIWorkshopButton.WorkshopButtonFunction.HasNotVideoRewardPaintJob;
		}
	}

	private void AwakeFacebookScreenshotMode()
	{
		GUIFacebookScreenshotMode.Instance().guiExtension = this;
	}

	public void SetBackButtonFunctionToNone()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.None;
	}

	public void SetBackButtonFunctionToMinimizeResumeScreenshot()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.MinimizeResumeScreenshot;
	}

	public void SetBackButtonFunctionToCancelScreenshot()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.CancelScreenshot;
	}

	public void SetBackButtonFunctionToLast()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.BackToDriving;
	}

	public void SetBackButtonFunctionToResumeScreenshot()
	{
		backButton.function = GUIWorkshopButton.WorkshopButtonFunction.ResumeScreenshot;
	}

	public void PreInitMode()
	{
		NGUITools.SetActive(pnWorkshop.gameObject, false);
		GUIFacebookScreenshotMode.Instance().SetTarget(MySingleton<Manager>.Instance.guiWorkshopLogic.currentCar.transform);
	}

	public void PostFinalizeMode()
	{
		NGUITools.SetActive(pnWorkshop.gameObject, true);
	}

	public void BeforePerformCameraRotation()
	{
	}

	public Camera GetCurrentCamera()
	{
		return Camera.main;
	}

	public void ActiveAchievementPanel(bool active)
	{
		if (active)
		{
			pnAchievements.GetComponent<AchievementsPanel>().Init(AchievementManager.AchievementCategory.GENERAL);
		}
		NGUITools.SetActive(pnAchievements, active);
	}
}
