using System;
using UnityEngine;

public class CarEntryBehaviour : MonoBehaviour
{
	public PanelUnlockCars patherPanel;

	public UISprite carIcon;

	public GameObject unlockedView;

	public GameObject oneChallengeView;

	public GameObject twoChallengeView;

	public GameObject buyCarView;

	public UIProgressBar progressBarOneChallenge;

	public UIProgressBar progressBarTwoChallengeOne;

	public UIProgressBar progressBarTwoChallengeTwo;

	public UISprite OneChallengeIcon;

	public UISprite TwoChallengeIconOne;

	public UISprite TwoChallengeIconTwo;

	public UILabel OneChallengeDistanceLabel;

	public UILabel TwoChallengeDistancelabel;

	public UILabel OneChallengeLabel;

	public UILabel TwoChallengeLabelOne;

	public UILabel TwoChallengeLabelTwo;

	public UISprite collectablesCarIcon;

	public GameObject buyPackButton;

	public GameObject buyCarButton;

	public GameObject buyCreditsButton;

	public GameObject challengeProgressButton;

	public UILabel creditsLabel;

	public UILabel playerChallengeProgressLabel;

	public UISprite challengeBuyViewIcon;

	private int CarId;

	private InfoCar infoCar;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(CarEntryInfo carInfo)
	{
		if (carIcon != null)
		{
			carIcon.spriteName = carInfo.carIconName;
			carIcon.MakePixelPerfect();
		}
		infoCar = new InfoCar();
		infoCar = carInfo.infoCar;
		CarId = carInfo.carID;
		switch (carInfo.entryView)
		{
		case CarEntryView.OneChallenge:
			SetOneChallengeView(carInfo);
			break;
		case CarEntryView.TwoChallenges:
			SetTwoChallengeView(carInfo);
			break;
		case CarEntryView.Unlocked:
			SetUnlockedView(carInfo);
			break;
		case CarEntryView.BuyOption:
			SetPurchaseView(carInfo);
			break;
		case CarEntryView.BuyPack:
			SetPurchasePackView(carInfo);
			break;
		case CarEntryView.BuyPackAndChallenge:
			SetPurchasePackAndChallengeView(carInfo);
			break;
		case CarEntryView.BuyOnlyChallenge:
			SetPuchaseOnlyChallengeView(carInfo);
			break;
		}
	}

	private void SetOneChallengeView(CarEntryInfo carInfo)
	{
		UnlockInfo unlockInfo = carInfo.unlockInfo[0];
		if (unlockInfo.challengePogress > unlockInfo.challengeTarget || unlockInfo.challengeTarget == 0f)
		{
			Debug.LogError("ERROR - wrong challenge progress values");
			return;
		}
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(true);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(false);
		OneChallengeDistanceLabel.text = Converter.GetDistanceUnit();
		SetIndividualChallenge(unlockInfo.challengeKind, unlockInfo.iconUnlockOptionName, unlockInfo.challengePogress, unlockInfo.challengeTarget, progressBarOneChallenge, OneChallengeIcon, OneChallengeLabel);
	}

	private void SetTwoChallengeView(CarEntryInfo carInfo)
	{
		UnlockInfo unlockInfo = carInfo.unlockInfo[0];
		UnlockInfo unlockInfo2 = carInfo.unlockInfo[1];
		collectablesCarIcon.spriteName = carInfo.carIconName;
		if (unlockInfo.challengePogress > unlockInfo.challengeTarget || unlockInfo.challengeTarget == 0f || unlockInfo2.challengePogress > unlockInfo2.challengeTarget || unlockInfo2.challengeTarget == 0f)
		{
		}
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(true);
		buyCarView.SetActive(false);
		TwoChallengeDistancelabel.text = Converter.GetDistanceUnit();
		SetIndividualChallenge(unlockInfo.challengeKind, unlockInfo.iconUnlockOptionName, unlockInfo.challengePogress, unlockInfo.challengeTarget, progressBarTwoChallengeOne, TwoChallengeIconOne, TwoChallengeLabelOne);
		SetIndividualChallenge(unlockInfo2.challengeKind, unlockInfo2.iconUnlockOptionName, unlockInfo2.challengePogress, unlockInfo2.challengeTarget, progressBarTwoChallengeTwo, TwoChallengeIconTwo, TwoChallengeLabelTwo);
	}

	private void SetUnlockedView(CarEntryInfo carInfo)
	{
		unlockedView.SetActive(true);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(false);
	}

	private void SetPurchaseView(CarEntryInfo carInfo)
	{
		challengeProgressButton.SetActive(false);
		buyPackButton.SetActive(false);
		if (carInfo.unlockInfo[0].playerMoney < carInfo.unlockInfo[0].carPrice)
		{
			buyCarButton.SetActive(false);
			buyCreditsButton.SetActive(true);
		}
		else
		{
			buyCarButton.SetActive(true);
			buyCreditsButton.SetActive(false);
		}
		creditsLabel.text = string.Empty + carInfo.unlockInfo[0].playerMoney + "/" + carInfo.unlockInfo[0].carPrice;
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(true);
	}

	private void SetPurchasePackView(CarEntryInfo carInfo)
	{
		challengeProgressButton.SetActive(false);
		buyCarButton.SetActive(false);
		if (carInfo.unlockInfo[0].playerMoney < carInfo.unlockInfo[0].carPrice)
		{
			buyPackButton.SetActive(false);
			buyCreditsButton.SetActive(true);
		}
		else
		{
			buyPackButton.SetActive(true);
			buyCreditsButton.SetActive(false);
		}
		creditsLabel.text = string.Empty + carInfo.unlockInfo[0].playerMoney + "/" + carInfo.unlockInfo[0].carPrice;
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(true);
	}

	private void SetPurchasePackAndChallengeView(CarEntryInfo carInfo)
	{
		challengeProgressButton.SetActive(true);
		playerChallengeProgressLabel.text = carInfo.unlockInfo[1].challengesCompleted + "/" + carInfo.unlockInfo[1].challengesTotal;
		challengeBuyViewIcon.spriteName = carInfo.unlockInfo[1].iconUnlockOptionName;
		buyCarButton.SetActive(false);
		if (carInfo.unlockInfo[0].playerMoney < carInfo.unlockInfo[0].carPrice)
		{
			buyPackButton.SetActive(false);
			buyCreditsButton.SetActive(true);
		}
		else
		{
			buyPackButton.SetActive(true);
			buyCreditsButton.SetActive(false);
		}
		creditsLabel.text = string.Empty + carInfo.unlockInfo[0].playerMoney + "/" + carInfo.unlockInfo[0].carPrice;
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(true);
	}

	private void SetPuchaseOnlyChallengeView(CarEntryInfo carInfo)
	{
		challengeProgressButton.SetActive(true);
		playerChallengeProgressLabel.text = carInfo.unlockInfo[0].challengesCompleted + "/" + carInfo.unlockInfo[0].challengesTotal;
		challengeBuyViewIcon.spriteName = carInfo.unlockInfo[0].iconUnlockOptionName;
		buyCarButton.SetActive(false);
		buyPackButton.SetActive(false);
		buyCreditsButton.SetActive(false);
		creditsLabel.gameObject.SetActive(false);
		unlockedView.SetActive(false);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(true);
	}

	private void SetIndividualChallenge(Challenge challengeOption, string challengeIconName, float challengeProgress, float challengeObjective, UIProgressBar progressBar, UISprite challengeSprite, UILabel challengeLabel)
	{
		string text = string.Empty;
		float num = 0f;
		switch (challengeOption)
		{
		case Challenge.Collectables:
		{
			int num8 = Convert.ToInt32(challengeProgress);
			int num9 = Convert.ToInt32(challengeObjective);
			text = string.Empty + num8 + "/" + num9;
			break;
		}
		case Challenge.Crasheable:
		{
			int num6 = Convert.ToInt32(challengeProgress);
			int num7 = Convert.ToInt32(challengeObjective);
			text = string.Empty + num6 + "/" + num7;
			break;
		}
		case Challenge.Distance:
		{
			string text2 = Converter.MetersToCurrentUnits(challengeProgress, false);
			string text3 = Converter.MetersToCurrentUnits(challengeObjective, false);
			text = string.Empty + text2 + "/" + text3;
			break;
		}
		case Challenge.Radar:
		{
			int num4 = Convert.ToInt32(challengeProgress);
			int num5 = Convert.ToInt32(challengeObjective);
			text = string.Empty + num4 + "/" + num5;
			break;
		}
		case Challenge.Trial:
		{
			int num2 = Convert.ToInt32(challengeProgress);
			int num3 = Convert.ToInt32(challengeObjective);
			text = string.Empty + num2 + "/" + num3;
			break;
		}
		}
		if (challengeLabel != null)
		{
			challengeLabel.text = text;
		}
		num = challengeProgress / challengeObjective;
		progressBar.value = num;
	}

	public void OnButtonBuyCreditsClicked()
	{
		MySingleton<Manager>.Instance.carUnlockedId = CarId;
		if (LegacySceneManager.LoadedLevelName != "1Menu")
		{
			MySingleton<Manager>.Instance.guiGameLogic.OpenCompleteTask();
			MySingleton<Manager>.Instance.guiGameLogic.ActivePauseMenu(false);
		}
		else
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowOffersPanel(true);
		}
		patherPanel.GetComponent<PanelUnlockCars>().HidePanel();
	}

	public void OnButtonBuyCarClicked()
	{
		ShowCarAtGarage();
	}

	public void OnButtonBuyPackClicked()
	{
		MySingleton<Manager>.Instance.indexLastCarInstantiate = CarId;
		MySingleton<Manager>.Instance.carUnlockedId = CarId;
		if (LegacySceneManager.LoadedLevelName != "1Menu")
		{
			MySingleton<Manager>.Instance.guiGameLogic.ActivePauseMenu(false);
			patherPanel.GetComponent<PanelUnlockCars>().HidePanel();
		}
		MySingleton<Manager>.Instance.guiMenuLogic.OpenPackStore(infoCar.purchasablePack);
	}

	public void OnButtonShowCollectablesClicked()
	{
		patherPanel.GoToCollectables();
	}

	public void OnButtonShowChallengesClicked()
	{
		patherPanel.GoToChallenges();
	}

	public void ShowCarAtGarage()
	{
		MySingleton<Manager>.Instance.carUnlockedId = CarId;
		if (LegacySceneManager.LoadedLevelName != "1Menu")
		{
			MySingleton<Manager>.Instance.guiGameLogic.BackToMenu();
		}
		else
		{
			MySingleton<Manager>.Instance.guiMenuLogic.LoadUnlockedCar();
		}
		patherPanel.GetComponent<PanelUnlockCars>().HidePanel();
	}

	public void SetToDefault()
	{
		if (carIcon != null)
		{
			carIcon.spriteName = string.Empty;
		}
		CarId = 0;
		infoCar = null;
		unlockedView.SetActive(true);
		oneChallengeView.SetActive(false);
		twoChallengeView.SetActive(false);
		buyCarView.SetActive(false);
		OneChallengeLabel.text = "0/0";
		TwoChallengeLabelOne.text = "0/0";
		TwoChallengeLabelTwo.text = "0/0";
		creditsLabel.text = "0/0";
		playerChallengeProgressLabel.text = "0/0";
		progressBarOneChallenge.value = 0f;
		progressBarTwoChallengeOne.value = 0f;
		progressBarTwoChallengeTwo.value = 0f;
	}
}
