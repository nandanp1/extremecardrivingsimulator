using System.Collections;
using Unibill;
using UnityEngine;

public class GUIStoreLogic : GUILogic
{
	public enum CameraMovement
	{
		FORWARD = 0,
		BACKWARD = 1,
		LEFT = 2,
		RIGHT = 3
	}

	private const string UI_ROOT_NAME_GAME_OBJECT = "UI Root";

	public UIPanel pnStarterPackBought;

	public GameObject gridStarterPack;

	public UIPanel pnStarterPack;

	public DailyRewardLogic dailyRewardLogic;

	private GameObject currentUIRootDisabled;

	[SerializeField]
	private UIPanel pnInfoReward;

	public UIPanel pnPackStore;

	[SerializeField]
	private UIPanel pnPurchaseInfo;

	[SerializeField]
	private UIPanel pnStore;

	[SerializeField]
	private UIPanel pnInfoPurchaseFailed;

	[SerializeField]
	private UIPanel pnNoInternet;

	[SerializeField]
	private UILabel lbQuantity;

	[SerializeField]
	private UILabel lbCreditsStore;

	[SerializeField]
	private UILabel lbCreditsStorePack;

	[SerializeField]
	private UILabel lbCreditsDailyRewards;

	[SerializeField]
	private UIPanel pnDailyRewards;

	[SerializeField]
	private UIPanel pnInfo;

	public UILabel lbInfoTitle;

	public UILabel lbInfoText;

	[SerializeField]
	private UIWidget btShowVideo;

	[SerializeField]
	private UIWidget lbVideoNotAvailable;

	public string dailyRewardsCarSpriteName;

	public int dailyRewardCarID;

	public BackButtonStore backButton;

	public AudioSource rewardClaimedSound;

	public UIGrid carsGrid;

	public InfoCar.PurchasablePack currentPack;

	public UILabel lbPurchaseInfo;

	public Transform[] spinner;

	public UIWidget btBuyPack;

	public UIWidget btBuyCar;

	public UIWidget btPackBought;

	public UIPanel pnScrollViewCars;

	public UISprite spSoloPack;

	public UILabel lbPackName;

	public UIPanel pnScrollView;

	public UISprite spriteCar;

	public UISprite spriteCredits;

	public UIWidget wdContainerCreditsReward;

	public Camera mainCamera;

	public UIDailyRewardSpriteLogic[] dailyRewardSprites;

	public UIWidget btClaimDailyReward;

	public UILabel lbDailyReward;

	public UILabel lbDailyRewardsInfo;

	public UISprite spCarReward;

	public UIWidget btOkInfoReward;

	public UISprite guiBackButton;

	public bool flagReset;

	public GameObject currentCar;

	public UISprite leftArrow;

	public UISprite rigthArrow;

	public UISprite upArrow;

	public UISprite downArrow;

	public GameObject[] currentPrefabs;

	public GameObject[] currentCars;

	public TweenTransform tweenTransform;

	public Transform[] stopPointsCamera;

	public Transform backPosition;

	private bool isCameraMoving;

	private CameraMovement currentCameraState;

	private bool fromStore;

	public InfoCar[] currentInfoCars;

	private int arrowIndex = -1;

	private float starterPackTime;

	private float savedStarterPackTime;

	public UILabel lbCounterofferPopup;

	public UILabel lbCounterofferStore;

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiStoreLogic = this;
		MySingleton<Manager>.Instance.taskManager.taskWatchVideoForCredits = new Task(TaskManager.TaskId.WatchVideoForCredits.ToString(), new RewardCredit(75));
		if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			NGUITools.SetActive(gridStarterPack, true);
			if (!MySingleton<Manager>.Instance.starterPopUpAlreadySeen && !MySingleton<Manager>.Instance.storeInitData.packStoreFromPause)
			{
				if ((bool)MySingleton<Manager>.Instance.guiMenuLogic && !MySingleton<Manager>.Instance.guiMenuLogic.openingPackStoreFromGaraje)
				{
					MySingleton<Manager>.Instance.starterPackPanelIsActive = true;
					backButton.function = GUIStoreButton.StoreButtonFunction.CloseStarterPack;
					if (!MySingleton<Manager>.Instance.restorePurchases)
					{
						NGUITools.SetActive(pnStarterPack.gameObject, true);
					}
				}
			}
			else
			{
				MySingleton<Manager>.Instance.starterPopUpAlreadySeen = false;
			}
			if ((bool)MySingleton<Manager>.Instance.guiMenuLogic)
			{
				MySingleton<Manager>.Instance.guiMenuLogic.openingPackStoreFromGaraje = false;
			}
			StartCoroutine(RefreshCrono());
		}
		else
		{
			HideStarterPack();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		mainCamera = Camera.main;
		currentUIRootDisabled = GameObject.Find("UI Root");
		if (!currentUIRootDisabled)
		{
			Debug.LogError("Revisar que el UI Root de la escena se llama como indica la constante UI_ROOT_NAME_GAME_OBJECT");
		}
		NGUITools.SetActive(currentUIRootDisabled.gameObject, false);
		UpdateGUI();
		ShowInfoIfFlag();
		ShowPurchasablePack();
		ShowInfoPanel(MySingleton<Manager>.Instance.storeInitData.titleInfo, MySingleton<Manager>.Instance.storeInitData.textInfo, MySingleton<Manager>.Instance.storeInitData.InfoStore);
		if (MySingleton<Manager>.Instance.storeInitData.storeDailyRewards)
		{
			ShowDailyRewardsPanel(true);
			MySingleton<Manager>.Instance.storeInitData.storeDailyRewards = false;
		}
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent += StarterPackWasBought;
		if (MySingleton<Manager>.Instance.starterPackBought)
		{
			ShowStarterPackBought();
		}
	}

	private void OnDisable()
	{
		NGUITools.SetActive(currentUIRootDisabled.gameObject, true);
		currentUIRootDisabled = null;
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent -= StarterPackWasBought;
		StopCoroutine(RefreshCrono());
	}

	public void ShowStarterPackBought()
	{
		rewardClaimedSound.ignoreListenerVolume = true;
		rewardClaimedSound.Play();
		NGUITools.SetActive(pnStarterPackBought.gameObject, true);
		MySingleton<Manager>.Instance.starterPackBought = false;
	}

	public void CloseStarterPackBought()
	{
		NGUITools.SetActive(pnStarterPackBought.gameObject, false);
		backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
	}

	public void ShowInfoPanel(string title, string info, bool show)
	{
		if (title != null && title != string.Empty)
		{
			lbInfoTitle.text = title;
		}
		if (info != null && info != string.Empty)
		{
			lbInfoText.text = info;
		}
		MySingleton<Manager>.Instance.storeInitData.titleInfo = null;
		MySingleton<Manager>.Instance.storeInitData.textInfo = null;
		MySingleton<Manager>.Instance.storeInitData.InfoStore = false;
		if (show && !MySingleton<Manager>.Instance.starterPackBought)
		{
			NGUITools.SetActive(pnStore.gameObject, false);
			NGUITools.SetActive(pnInfo.gameObject, true);
		}
		else if (show && MySingleton<Manager>.Instance.starterPackBought)
		{
			rewardClaimedSound.ignoreListenerVolume = true;
			rewardClaimedSound.Play();
			NGUITools.SetActive(pnStore.gameObject, false);
			NGUITools.SetActive(pnStarterPackBought.gameObject, true);
			MySingleton<Manager>.Instance.starterPackBought = false;
		}
	}

	public void ShowInfoRewardPanel(string amount)
	{
		lbQuantity.text = amount;
		Debug.Log("Label sin UIAutoResizable", lbQuantity);
		UIAutoResizeLabel component = lbQuantity.GetComponent<UIAutoResizeLabel>();
		if ((bool)component)
		{
			component.RecenterInParent();
		}
		rewardClaimedSound.ignoreListenerVolume = true;
		rewardClaimedSound.Play();
		backButton.function = GUIStoreButton.StoreButtonFunction.HideInfoPanel;
		NGUITools.SetActive(pnInfoReward.gameObject, true);
	}

	public void HideInfoPanel()
	{
		NGUITools.SetActive(pnInfoReward.gameObject, false);
		NGUITools.SetActive(pnPurchaseInfo.gameObject, false);
		if (!MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
		}
		else
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.BackToPackStore;
		}
	}

	public void ShowInfoPurchaseFailedPanel()
	{
		NGUITools.SetActive(pnInfoPurchaseFailed.gameObject, true);
		backButton.function = GUIStoreButton.StoreButtonFunction.HideInfoPurchaseFailed;
	}

	public void HideInfoPurchaseFailedPanel()
	{
		NGUITools.SetActive(pnInfoPurchaseFailed.gameObject, false);
		backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
	}

	public void UpdateGUI()
	{
		UpdateCredits();
		UpdateRewards();
		OnVideoRewardsAvailabilityChanged(MySingleton<Manager>.Instance.ads.heyzapController.IsVideoRewardAvailable());
	}

	public void UpdateCredits()
	{
		lbCreditsStore.text = Unibiller.GetCurrencyBalance("credits").ToString();
		lbCreditsStorePack.text = Unibiller.GetCurrencyBalance("credits").ToString();
		lbCreditsDailyRewards.text = Unibiller.GetCurrencyBalance("credits").ToString();
	}

	public void UpdateRewards()
	{
		if (!SettingsManager.IsFirstTimeLastDayReward("keyFirstTimeLastDayDayliReward") || SettingsManager.IsCarBought("keyCar" + dailyRewardCarID))
		{
			NGUITools.SetActive(spriteCar.gameObject, false);
			NGUITools.SetActive(spriteCredits.gameObject, true);
		}
		else
		{
			spriteCar.atlas = MySingleton<Manager>.Instance.atlasCar;
			spriteCar.spriteName = dailyRewardsCarSpriteName;
		}
		if (SettingsManager.GetStarterPackBoughtOrEnded())
		{
			pnScrollView.topAnchor.absolute = -159;
			pnScrollView.UpdateAnchors();
			SpringPanel.Begin(pnScrollView.gameObject, new Vector3(0f, 43f), 7f);
		}
		else if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			pnScrollView.topAnchor.absolute = -274;
			pnScrollView.UpdateAnchors();
			SpringPanel.Begin(pnScrollView.gameObject, new Vector3(0f, -71f), 8f);
		}
		if (flagReset || dailyRewardLogic.UpdateDailyReward())
		{
			btClaimDailyReward.GetComponent<TweenColor>().enabled = true;
			btClaimDailyReward.GetComponent<TweenScale>().enabled = true;
			lbDailyReward.text = Localization.Get("claim");
			flagReset = false;
		}
		else
		{
			TweenColor component = btClaimDailyReward.GetComponent<TweenColor>();
			component.enabled = false;
			btClaimDailyReward.GetComponent<TweenScale>().enabled = false;
			btClaimDailyReward.GetComponent<UISprite>().color = component.from;
			btClaimDailyReward.transform.localScale = Vector3.one;
			lbDailyReward.text = Localization.Get("view");
		}
	}

	public override void UpdateLocalizeLabels()
	{
	}

	public void ShowVideoRewardForCredits()
	{
		if (MySingleton<Manager>.Instance.ads.heyzapController.IsVideoRewardAvailable())
		{
			MySingleton<Manager>.Instance.ads.heyzapController.ShowVideoReward(new Task(TaskManager.TaskId.WatchVideoForCredits.ToString(), new RewardCredit(75)));
			ManagerAnalytics.instance.FireRewardedVideo("IAP_shop");
		}
	}

	public void ShowInternetFailedPanel(bool show)
	{
		if (show && (bool)backButton)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.HideInternetFailPanel;
		}
		NGUITools.SetActive(pnNoInternet.gameObject, show);
	}

	public void ShowDailyRewardsPanel(bool show)
	{
		if (show)
		{
			dailyRewardLogic.UpdateDailyReward();
			backButton.function = GUIStoreButton.StoreButtonFunction.HideDailyRewardsPanel;
		}
		else if (!MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
		}
		else
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.BackToPackStore;
		}
		NGUITools.SetActive(pnDailyRewards.gameObject, show);
	}

	public void ShowEarnedCar()
	{
		MySingleton<Manager>.Instance.carUnlockedId = dailyRewardCarID;
		ExitStore();
		if (MySingleton<Manager>.Instance.storeInitData.lastSceneLoaded == 2)
		{
			MySingleton<Manager>.Instance.guiGameLogic.BackToMenu();
		}
		else
		{
			LegacySceneManager.LoadLevel(1);
		}
	}

	public void ClaimReward()
	{
		dailyRewardLogic.ClaimDay();
		UpdateGUI();
	}

	public void CanCollectLogic(int day)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < day)
			{
				dailyRewardSprites[i].ShowDone();
			}
			else if (i == day)
			{
				dailyRewardSprites[i].ShowToday();
			}
			else if (i == day + 1)
			{
				dailyRewardSprites[i].ShowTomorrow();
			}
			else
			{
				dailyRewardSprites[i].HideAll();
			}
		}
	}

	public void CannotCollectLogic(int day)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i < day)
			{
				dailyRewardSprites[i].ShowDone();
			}
			else if (i == day)
			{
				dailyRewardSprites[i].ShowTomorrow();
			}
			else
			{
				dailyRewardSprites[i].HideAll();
			}
		}
	}

	public void ResetLogic(bool lastDay)
	{
		if (!lastDay)
		{
			for (int i = 0; i < 5; i++)
			{
				switch (i)
				{
				case 0:
					dailyRewardSprites[i].ShowToday();
					break;
				case 1:
					dailyRewardSprites[i].ShowTomorrow();
					break;
				default:
					dailyRewardSprites[i].HideAll();
					break;
				}
			}
			return;
		}
		for (int j = 0; j < 5; j++)
		{
			if (j == 0)
			{
				dailyRewardSprites[j].ShowTomorrow();
			}
			else
			{
				dailyRewardSprites[j].HideAll();
			}
		}
	}

	public void CloseStarterPackPanel()
	{
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
		backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
	}

	public void HideStarterPack()
	{
		NGUITools.SetActive(gridStarterPack, false);
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
		UpdateGUI();
	}

	public void StarterPackWasBought()
	{
		HideStarterPack();
		ShowStarterPackBought();
	}

	public void ShowPanelInfoReward(decimal value)
	{
		backButton.function = GUIStoreButton.StoreButtonFunction.HideInfoPanel;
		lbQuantity.text = value.ToString();
		if (value < 0m)
		{
			spCarReward.atlas = MySingleton<Manager>.Instance.atlasCar;
			spCarReward.spriteName = dailyRewardsCarSpriteName;
			NGUITools.SetActive(spCarReward.gameObject, true);
			NGUITools.SetActive(wdContainerCreditsReward.gameObject, false);
		}
		else
		{
			NGUITools.SetActive(wdContainerCreditsReward.gameObject, true);
		}
		NGUITools.SetActive(pnInfoReward.gameObject, true);
		rewardClaimedSound.ignoreListenerVolume = true;
		rewardClaimedSound.Play();
	}

	public void TestAlmostADay()
	{
		dailyRewardLogic.TestAlmostADay();
	}

	public void TestMoreThanADay()
	{
		dailyRewardLogic.TestMoreThanADay();
	}

	public void ShowPurchasablePack()
	{
		if (MySingleton<Manager>.Instance.storeInitData.packID != InfoCar.PurchasablePack.none)
		{
			NGUITools.SetActive(pnStore.gameObject, false);
			NGUITools.SetActive(pnPackStore.gameObject, true);
			if (mainCamera == null)
			{
				mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			}
			NGUITools.SetActive(mainCamera.gameObject, false);
			ShowThisPack(MySingleton<Manager>.Instance.storeInitData.packID);
			MySingleton<Manager>.Instance.storeInitData.packID = InfoCar.PurchasablePack.none;
			MySingleton<Manager>.Instance.storeInitData.pack = null;
		}
		if (MySingleton<Manager>.Instance.starterPackPanelIsActive)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.CloseStarterPack;
		}
		else
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
		}
	}

	public void ShowThisPack(InfoCar.PurchasablePack pack, bool reloadCar = true, bool fromStore = false)
	{
		MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks = true;
		this.fromStore = fromStore;
		currentPack = pack;
		if ((bool)lbPackName)
		{
			NGUITools.SetActive(lbPackName.gameObject, true);
			lbPackName.text = Localization.Get(MySingleton<Manager>.Instance.GetPackNameKey(currentPack));
		}
		if (MySingleton<Manager>.Instance.selectedGame != Manager.SelectedGame.SanFrancisco)
		{
			foreach (Transform item in carsGrid.transform)
			{
				item.gameObject.SetActive(false);
			}
		}
		int num = 0;
		bool flag = true;
		bool flag2 = true;
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.purchasablePack != pack)
			{
				continue;
			}
			if (MySingleton<Manager>.Instance.selectedGame != Manager.SelectedGame.SanFrancisco)
			{
				carsGrid.GetChild(num).gameObject.SetActive(true);
				carsGrid.GetChild(num).GetComponentInChildren<UISprite>().atlas = MySingleton<Manager>.Instance.atlasCar;
				carsGrid.GetChild(num).GetComponentInChildren<UISprite>().spriteName = infoCar.iconName;
				carsGrid.GetChild(num).GetComponentInChildren<GUICarId>().carId = infoCar.id;
			}
			if ((flag && reloadCar) || MySingleton<Manager>.Instance.storeInitData.packStoreFromPause)
			{
				if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.SanFrancisco)
				{
					PutCarSpinnerNew(pack);
				}
				else
				{
					PutCarSpinner(infoCar.id);
				}
				flag = false;
				MySingleton<Manager>.Instance.storeInitData.packStoreFromPause = false;
			}
			if (!infoCar.isBought)
			{
				flag2 = false;
			}
			num++;
		}
		if (flag2)
		{
			HideBuyPackButton(true);
		}
		else
		{
			HideBuyPackButton(false);
		}
	}

	public void ShowThisPack(InfoPack infoPack, bool reloadCar = true, bool fromStore = false)
	{
		MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks = true;
		this.fromStore = fromStore;
		currentPack = infoPack.packId;
		if ((bool)lbPackName)
		{
			NGUITools.SetActive(lbPackName.gameObject, false);
			if ((bool)lbPackName)
			{
				NGUITools.SetActive(lbPackName.gameObject, true);
				lbPackName.text = Localization.Get(infoPack.packNameKey);
			}
		}
		foreach (Transform item in carsGrid.transform)
		{
			item.gameObject.SetActive(false);
		}
		int num = 0;
		bool flag = true;
		bool flag2 = true;
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.purchasablePack != infoPack.packId)
			{
				continue;
			}
			carsGrid.GetChild(num).gameObject.SetActive(true);
			carsGrid.GetChild(num).GetComponentInChildren<UISprite>().atlas = MySingleton<Manager>.Instance.atlasCar;
			carsGrid.GetChild(num).GetComponentInChildren<UISprite>().spriteName = infoCar.iconName;
			carsGrid.GetChild(num).GetComponentInChildren<GUICarId>().carId = infoCar.id;
			if ((flag && reloadCar) || MySingleton<Manager>.Instance.storeInitData.packStoreFromPause)
			{
				if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.SanFrancisco)
				{
					PutCarSpinnerNew(currentPack);
				}
				else
				{
					PutCarSpinner(infoCar.id);
				}
				flag = false;
				MySingleton<Manager>.Instance.storeInitData.packStoreFromPause = false;
			}
			if (!infoCar.isBought)
			{
				flag2 = false;
			}
			num++;
		}
		if (flag2)
		{
			HideBuyPackButton(true);
		}
		else
		{
			HideBuyPackButton(false);
		}
	}

	public void BuyThisPack()
	{
		if (Unibiller.GetCurrencyBalance("credits") < 5500m)
		{
			ShowOffersPanel(true, true);
			backButton.function = GUIStoreButton.StoreButtonFunction.HideOffersPanel;
			return;
		}
		MySingleton<Manager>.Instance.stats.AddCarPackBought();
		rewardClaimedSound.ignoreListenerVolume = true;
		rewardClaimedSound.Play();
		Unibiller.DebitBalance("credits", 5500m);
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.purchasablePack == currentPack && !infoCar.isBought)
			{
				infoCar.isBought = true;
				ManagerAnalytics.instance.FireCarUnlocked(infoCar.idName, "currency_pack");
			}
		}
		UpdateCredits();
		HideBuyPackButton(true);
		HideSoloPackButton(true);
		HideBuyCarButton(true);
	}

	public void BuyThisCar()
	{
		if (Unibiller.GetCurrencyBalance("credits") < 3500m)
		{
			ShowOffersPanel(true);
			backButton.function = GUIStoreButton.StoreButtonFunction.HideOffersPanel;
			return;
		}
		rewardClaimedSound.ignoreListenerVolume = true;
		rewardClaimedSound.Play();
		Unibiller.DebitBalance("credits", 3500m);
		currentCar.GetComponent<InfoCar>().isBought = true;
		ManagerAnalytics.instance.FireCarUnlocked(currentCar.GetComponent<InfoCar>().idName, "currency_car_from_pack");
		HideBuyCarButton(true);
		UpdateCredits();
	}

	public void ShowOffersPanel(bool show, bool isPack = false)
	{
		if (show)
		{
			int num = (int)(3500m - Unibiller.GetCurrencyBalance("credits"));
			if (isPack)
			{
				num = (int)(5500m - Unibiller.GetCurrencyBalance("credits"));
			}
			lbPurchaseInfo.text = string.Format(Localization.Get("need_more_credits_placeholder"), num.ToString());
		}
		NGUITools.SetActive(pnPurchaseInfo.gameObject, show);
		if (!show)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
		}
	}

	public void PutCarSpinner(int carId)
	{
		bool flag = true;
		MySingleton<Manager>.Instance.indexLastCarInstantiate = carId;
		if ((bool)currentCar && currentCar.GetComponent<InfoCar>().id != carId)
		{
			NGUITools.Destroy(currentCar);
		}
		if (!currentCar || ((bool)currentCar && currentCar.GetComponent<InfoCar>().id != carId))
		{
			InfoCar infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(carId)];
			GameObject original = Resources.Load(infoCar.idName) as GameObject;
			currentCar = Object.Instantiate(original, spinner[0].position, spinner[0].rotation) as GameObject;
			currentCar.GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)10;
			SetLayerCulled(currentCar, LayerMask.NameToLayer("Store"), LayerMask.NameToLayer("Interior"));
			InfoCar infoCar2 = currentCar.AddMissingComponent<InfoCar>();
			infoCar2.Initialize(infoCar);
			HideBuyCarButton(infoCar2.isBought || !infoCar2.purchasable);
			HideSoloPackButton(infoCar2.isBought || infoCar2.purchasable);
			Camera[] componentsInChildren = currentCar.GetComponentsInChildren<Camera>();
			Camera[] array = componentsInChildren;
			foreach (Camera camera in array)
			{
				camera.gameObject.SetActive(false);
			}
			flag = false;
			currentCar.transform.parent = spinner[0].transform;
		}
		pnScrollViewCars.GetComponent<UIScrollView>().ResetPosition();
	}

	public void PutCarSpinnerNew(InfoCar.PurchasablePack pack)
	{
		if (MySingleton<Manager>.Instance.indexLastPackInstantiate != InfoCar.PurchasablePack.none && MySingleton<Manager>.Instance.indexLastPackInstantiate != pack)
		{
			GameObject[] array = currentCars;
			foreach (GameObject obj in array)
			{
				NGUITools.Destroy(obj);
			}
			MySingleton<Manager>.Instance.alreadyLoaded = false;
		}
		if (!MySingleton<Manager>.Instance.alreadyLoaded)
		{
			int num = 0;
			int num2 = -1;
			InfoCar[] array2 = new InfoCar[currentCars.Length];
			InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
			foreach (InfoCar infoCar in carsInfoPrefabs)
			{
				if (infoCar.purchasablePack == pack)
				{
					array2[num] = infoCar;
					if (num2 == -1 && !infoCar.purchasable)
					{
						num2 = num;
					}
					num++;
				}
			}
			if (num2 != 1)
			{
				InfoCar infoCar2 = array2[1];
				array2[1] = array2[num2];
				array2[num2] = infoCar2;
			}
			for (int k = 0; k < array2.Length; k++)
			{
				GameObject original = Resources.Load(array2[k].idName) as GameObject;
				currentCars[k] = Object.Instantiate(original, spinner[k].position, spinner[k].rotation) as GameObject;
				currentCars[k].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				SetLayerCulled(currentCars[k], LayerMask.NameToLayer("Store"), LayerMask.NameToLayer("Interior"));
				InfoCar infoCar3 = currentCars[k].AddMissingComponent<InfoCar>();
				infoCar3.Initialize(array2[k]);
				Camera[] componentsInChildren = currentCars[k].GetComponentsInChildren<Camera>();
				Camera[] array3 = componentsInChildren;
				foreach (Camera camera in array3)
				{
					camera.gameObject.SetActive(false);
				}
				currentCars[k].transform.parent = spinner[k].transform;
				if (!fromStore && MySingleton<Manager>.Instance.guiMenuLogic.currentCar != null && currentCars[k].GetComponent<InfoCar>().id == MySingleton<Manager>.Instance.guiMenuLogic.currentCar.GetComponent<InfoCar>().id)
				{
					arrowIndex = k;
					MoveCamera(CameraMovement.FORWARD);
				}
			}
			if (arrowIndex < 0)
			{
				arrowIndex = 1;
				MoveCamera(CameraMovement.BACKWARD);
			}
			currentCar = currentCars[arrowIndex];
			MySingleton<Manager>.Instance.alreadyLoaded = true;
		}
		InfoCar component = currentCar.GetComponent<InfoCar>();
		HideBuyCarButton(component.isBought || !component.purchasable);
		if (upArrow.gameObject.activeSelf)
		{
			HideSoloPackButton(true);
			HideBuyCarButton(true);
		}
		else
		{
			HideSoloPackButton(!upArrow.gameObject.activeSelf && (component.isBought || component.purchasable));
		}
		MySingleton<Manager>.Instance.indexLastPackInstantiate = pack;
	}

	public static void SetLayerCulled(GameObject go, int layer, int layerCulled)
	{
		go.layer = layer;
		if (layer != 13)
		{
			Debug.Log(go.name);
			Debug.Log(layer);
		}
		Transform transform = go.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child.gameObject.layer != layerCulled)
			{
				SetLayerCulled(child.gameObject, layer, layerCulled);
			}
		}
	}

	public void ShowOrHideStore(bool show)
	{
		NGUITools.SetActive(pnStore.gameObject, show);
		NGUITools.SetActive(pnPurchaseInfo.gameObject, !show);
		NGUITools.SetActive(pnPackStore.gameObject, !show);
		if (!MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks)
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
			GetComponent<GUIStoreButton>().function = GUIStoreButton.StoreButtonFunction.ExitStore;
		}
		else
		{
			backButton.function = GUIStoreButton.StoreButtonFunction.BackToPackStore;
			GetComponent<GUIStoreButton>().function = GUIStoreButton.StoreButtonFunction.BackToPackStore;
		}
	}

	public void BackToPackStore()
	{
		NGUITools.SetActive(pnStore.gameObject, false);
		NGUITools.SetActive(pnPackStore.gameObject, true);
		NGUITools.SetActive(pnDailyRewards.gameObject, false);
		backButton.function = GUIStoreButton.StoreButtonFunction.ExitStore;
	}

	public void HideBuyPackButton(bool show)
	{
		NGUITools.SetActive(btBuyPack.gameObject, !show);
		NGUITools.SetActive(btPackBought.gameObject, show);
	}

	public void HideBuyCarButton(bool show)
	{
		NGUITools.SetActive(btBuyCar.gameObject, !show);
	}

	public void HideSoloPackButton(bool show)
	{
		NGUITools.SetActive(spSoloPack.gameObject, !show);
	}

	public override void PurchasesRestored(bool success)
	{
	}

	public override void PurchaseCompleted(PurchaseEvent purchase)
	{
		if (purchase.PurchasedItem.PurchaseType == PurchaseType.Consumable)
		{
			string amount = MySingleton<Manager>.Instance.billingManager.GetCreditsAmount(purchase.PurchasedItem.LocalId).ToString();
			ShowInfoRewardPanel(amount);
		}
		UpdateGUI();
	}

	public override void PurchaseCanceled(PurchasableItem item)
	{
	}

	public override void PurchaseFailed(PurchasableItem item)
	{
		ShowInfoPurchaseFailedPanel();
	}

	public override void BillerInitialized(UnibillState state)
	{
	}

	public override void OnRewardClaimed(RewardItem reward)
	{
		if (reward is RewardCredit)
		{
			ShowInfoRewardPanel(reward.rewardAmount.ToString());
		}
		UpdateGUI();
	}

	public override void OnVideoRewardsFinished(Task task)
	{
		if (task.IsComplete())
		{
			task.reward.GiveReward();
			ShowInfoRewardPanel(task.reward.rewardAmount.ToString());
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
		NGUITools.SetActive(lbVideoNotAvailable.gameObject, !isVideoAvailable);
		NGUITools.SetActive(btShowVideo.gameObject, isVideoAvailable);
	}

	private void ShowInfoIfFlag()
	{
		int num = MySingleton<Manager>.Instance.ConsumeStoreFlag();
		if (num > 0)
		{
			ShowInfoRewardPanel(num.ToString());
		}
	}

	public void SelectCar(int dir)
	{
		if (dir > 0)
		{
			if (arrowIndex < currentCars.Length - 1)
			{
				if (!isCameraMoving)
				{
					arrowIndex++;
					currentCar = currentCars[arrowIndex];
				}
				MoveCamera(CameraMovement.RIGHT);
			}
		}
		else if (arrowIndex > 0)
		{
			if (!isCameraMoving)
			{
				arrowIndex--;
				currentCar = currentCars[arrowIndex];
			}
			MoveCamera(CameraMovement.LEFT);
		}
		InfoCar component = currentCar.GetComponent<InfoCar>();
		HideBuyCarButton(component.isBought || !component.purchasable);
		HideSoloPackButton(component.isBought || component.purchasable);
	}

	public void UpDownArrow(int dir)
	{
		if (dir > 0)
		{
			MoveCamera(CameraMovement.FORWARD);
		}
		else if (dir < 0)
		{
			MoveCamera(CameraMovement.BACKWARD);
		}
	}

	public void MoveCamera(CameraMovement cameraMovement)
	{
		if (isCameraMoving)
		{
			return;
		}
		currentCameraState = cameraMovement;
		switch (cameraMovement)
		{
		case CameraMovement.BACKWARD:
			tweenTransform.from = stopPointsCamera[arrowIndex];
			tweenTransform.to = backPosition;
			NGUITools.SetActive(upArrow.gameObject, true);
			NGUITools.SetActive(leftArrow.gameObject, false);
			NGUITools.SetActive(rigthArrow.gameObject, false);
			NGUITools.SetActive(downArrow.gameObject, false);
			NGUITools.SetActive(btBuyCar.gameObject, false);
			HideSoloPackButton(true);
			break;
		case CameraMovement.FORWARD:
		{
			tweenTransform.from = backPosition;
			tweenTransform.to = stopPointsCamera[arrowIndex];
			NGUITools.SetActive(upArrow.gameObject, false);
			NGUITools.SetActive(leftArrow.gameObject, true);
			NGUITools.SetActive(rigthArrow.gameObject, true);
			NGUITools.SetActive(downArrow.gameObject, true);
			NGUITools.SetActive(leftArrow.gameObject, arrowIndex != 0);
			NGUITools.SetActive(rigthArrow.gameObject, arrowIndex != currentCars.Length - 1);
			if ((bool)currentCars[arrowIndex])
			{
				NGUITools.SetActive(btBuyCar.gameObject, currentCars[arrowIndex].GetComponent<InfoCar>().purchasable);
			}
			InfoCar component = currentCars[arrowIndex].GetComponent<InfoCar>();
			HideSoloPackButton(component.isBought || component.purchasable);
			break;
		}
		case CameraMovement.RIGHT:
			tweenTransform.from = stopPointsCamera[arrowIndex - 1];
			tweenTransform.to = stopPointsCamera[arrowIndex];
			NGUITools.SetActive(leftArrow.gameObject, arrowIndex != 0);
			NGUITools.SetActive(rigthArrow.gameObject, arrowIndex != currentCars.Length - 1);
			break;
		case CameraMovement.LEFT:
			tweenTransform.from = stopPointsCamera[arrowIndex + 1];
			tweenTransform.to = stopPointsCamera[arrowIndex];
			NGUITools.SetActive(leftArrow.gameObject, arrowIndex != 0);
			NGUITools.SetActive(rigthArrow.gameObject, arrowIndex != currentCars.Length - 1);
			break;
		}
		tweenTransform.ResetToBeginning();
		tweenTransform.enabled = true;
		isCameraMoving = true;
	}

	public void SetIsCameraMoving(bool isMoving = false)
	{
		isCameraMoving = isMoving;
	}

	private IEnumerator RefreshCrono()
	{
		starterPackTime = MySingleton<Manager>.Instance.localChrono.GetRemainingUnescaled(MySingleton<Manager>.Instance.startingTime);
		while (starterPackTime > 0f)
		{
			if (starterPackTime > SettingsManager.STARTER_PACK_2ND_STEP)
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[0];
				lbCounterofferStore.color = MySingleton<Manager>.Instance.cronoColors[0];
			}
			else if (starterPackTime > SettingsManager.STARTER_PACK_3ND_STEP && starterPackTime <= SettingsManager.STARTER_PACK_2ND_STEP)
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[1];
				lbCounterofferStore.color = MySingleton<Manager>.Instance.cronoColors[1];
			}
			else
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[2];
				lbCounterofferStore.color = MySingleton<Manager>.Instance.cronoColors[2];
			}
			lbCounterofferPopup.text = Converter.SecondsToHhMmSs(starterPackTime);
			lbCounterofferStore.text = Converter.SecondsToHhMmSs(starterPackTime);
			starterPackTime = MySingleton<Manager>.Instance.localChrono.GetRemainingUnescaled(MySingleton<Manager>.Instance.startingTime);
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(1f));
		}
		SettingsManager.SetStarterPackBoughtOrEnded(true);
		Debug.Log("counter finish");
		HideStarterPack();
	}
}
