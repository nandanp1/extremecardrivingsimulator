using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class Ads : MonoBehaviour
{
	private const string KEY_MEDIATION_COUNTER = "keyAdColonyCounter";

	private const string KEY_REGULAR_COUNTER = "key_regular_counter";

	private const int TIME_TO_AD = 3;

	private static int mediationCounter;

	[HideInInspector]
	public float lastTimeAds;

	[HideInInspector]
	public float timeFromLastPlay;

	private int adTemp;

	public UIPanel PnAdsComing;

	public UILabel LbTime;

	[HideInInspector]
	public bool disabledAds;

	public Admob_Universal admobUniversal;

	public Heyzap_Controller heyzapController;

	public bool admobAdActive = true;

	private int amountA_B;

	private int amountB_B;

	private bool step1_B = true;

	private int regularCounter;

	private static int occurredEventsCounter;

	private bool disableFromChallange;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Init();
	}

	public void Init()
	{
		disabledAds = true;
		regularCounter = PlayerPrefs.GetInt("key_regular_counter", 0);
		adTemp = 3;
		CancelInvoke();
	}

	public void ShowAdmobOrHeyzap(bool fromChallangeCircuit, bool admob = true, bool enabledAdplacement = true)
	{
		if (disabledAds || !enabledAdplacement || (!(lastTimeAds <= 0f) && !(Time.realtimeSinceStartup - lastTimeAds >= (float)MySingleton<Manager>.Instance.remoteConfiguration.timeBetweenRegularAds)))
		{
			return;
		}
		if (LegacySceneManager.LevelCount == 2 && !fromChallangeCircuit)
		{
			RestartCounter();
		}
		if (admobAdActive)
		{
			Debug.Log("Admob turn");
			if (MySingleton<Manager>.Instance.admobInterstitialIsReady && admob)
			{
				admobUniversal.ShowIntersitialAd();
			}
			else if (heyzapController.GetHeyzapInterstitialReady())
			{
				heyzapController.ShowHeyzapInterstitial();
			}
		}
		else
		{
			Debug.Log("Heyzap turn");
			if (heyzapController.GetHeyzapInterstitialReady())
			{
				heyzapController.ShowHeyzapInterstitial();
			}
			else if (MySingleton<Manager>.Instance.admobInterstitialIsReady && admob)
			{
				admobUniversal.ShowIntersitialAd();
			}
		}
		CheckNextRegularAd();
		MySingleton<Manager>.Instance.adsFromPause = false;
	}

	private void CheckNextRegularAd()
	{
		if (MySingleton<Manager>.Instance.remoteConfiguration.startRegularWithAdmob)
		{
			amountA_B = MySingleton<Manager>.Instance.remoteConfiguration.consecutiveAdmobRegularAds;
			amountB_B = MySingleton<Manager>.Instance.remoteConfiguration.consecutiveHeyzapRegularAds;
		}
		else
		{
			amountA_B = MySingleton<Manager>.Instance.remoteConfiguration.consecutiveHeyzapRegularAds;
			amountB_B = MySingleton<Manager>.Instance.remoteConfiguration.consecutiveAdmobRegularAds;
		}
		regularCounter++;
		Debug.Log("STEP_:" + step1_B + " AdmobCounter=" + regularCounter);
		switch (step1_B)
		{
		case true:
			if (regularCounter >= amountA_B)
			{
				Debug.Log("Pasamos a step2");
				step1_B = false;
				admobAdActive = !admobAdActive;
			}
			break;
		case false:
			if (regularCounter >= amountA_B + amountB_B)
			{
				Debug.Log("Reiniciamos y pasamos a step 1");
				step1_B = true;
				admobAdActive = !admobAdActive;
				regularCounter = 0;
			}
			break;
		}
		PlayerPrefs.SetInt("key_regular_counter", regularCounter);
	}

	private void ShowActionPhaseAd()
	{
		if (disabledAds || !MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacement321)
		{
			return;
		}
		if (lastTimeAds <= 0f || Time.realtimeSinceStartup - lastTimeAds >= (float)MySingleton<Manager>.Instance.remoteConfiguration.timeBetween321Ads)
		{
			if (heyzapController.GetHeyzapInterstitialReady())
			{
				heyzapController.ShowHeyzapInterstitial();
			}
			RestartCounter();
		}
		MySingleton<Manager>.Instance.adsFromPause = false;
	}

	private void OnLevelWasLoaded(int level)
	{
		if (!disabledAds)
		{
			if (level == 2)
			{
				RestartCounter();
			}
			else
			{
				CancelInvoke("ShowCounterAds");
			}
		}
	}

	public void RestartCounter()
	{
		if (!disabledAds && !disableFromChallange)
		{
			float time = MySingleton<Manager>.Instance.remoteConfiguration.timeBetween321Ads;
			CancelInvoke("ShowCounterAds");
			InvokeRepeating("ShowCounterAds", time, MySingleton<Manager>.Instance.remoteConfiguration.timeBetween321Ads);
		}
	}

	private void ShowCounterAds()
	{
		if (!disabledAds && MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacement321)
		{
			if (heyzapController.GetHeyzapInterstitialReady())
			{
				NGUITools.SetActive(PnAdsComing.gameObject, true);
				InvokeRepeating("AdTime", 1f, 1f);
				Invoke("ShowActionPhaseAd", 3f);
			}
			else
			{
				RestartCounter();
				Debug.LogWarning("Try 3,2,1 but no fill");
			}
		}
	}

	public void CancelPnMsg()
	{
		if (!disabledAds)
		{
			CancelInvoke("ShowAdmobOrHeyzap");
			CancelInvoke("ShowActionPhaseAd");
			CancelInvoke("AdTime");
			adTemp = 3;
			LbTime.text = string.Format("AD IN ... {0}", adTemp);
			NGUITools.SetActive(PnAdsComing.gameObject, false);
		}
	}

	private void AdTime()
	{
		if (!disabledAds)
		{
			if (adTemp >= 0)
			{
				adTemp--;
			}
			if (adTemp < 0)
			{
				CancelInvoke("AdTime");
				adTemp = 3;
				NGUITools.SetActive(PnAdsComing.gameObject, false);
			}
			LbTime.text = string.Format("AD IN ... {0}", adTemp);
		}
	}

	public void DisableAllAds()
	{
		disabledAds = true;
		CancelInvoke();
	}

	public void ShowBanner()
	{
		if (!disabledAds && MySingleton<Manager>.Instance.remoteConfiguration.globalBanner)
		{
			admobUniversal.ShowBanner();
		}
	}

	public void HideBanner()
	{
	}

	public void CheckForTimeInterstitial()
	{
		if (!disabledAds && MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementGoToGame && MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementGoToGame && ((timeFromLastPlay == 0f) | (Time.realtimeSinceStartup > timeFromLastPlay + (float)MySingleton<Manager>.Instance.remoteConfiguration.timeBetweenGoToGameAds)))
		{
			admobUniversal.ShowIntersitialAd();
			timeFromLastPlay = Time.realtimeSinceStartup;
		}
	}

	public void ResetOccurredEvents()
	{
		occurredEventsCounter = 0;
	}

	public void ShowAdmobOrHeyzapAboutOccurredEvents()
	{
		if (!disabledAds)
		{
			occurredEventsCounter++;
			if (occurredEventsCounter % MySingleton<Manager>.Instance.remoteConfiguration.maxChallengeTrysAds == 0)
			{
				MySingleton<Manager>.Instance.adsFromPause = false;
				ShowAdmobOrHeyzap(true, MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementFailRetryChallenges, MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementFailRetryChallenges);
			}
		}
	}

	public void ShowAdsFromChallange()
	{
		if (!disabledAds)
		{
			MySingleton<Manager>.Instance.adsFromPause = false;
			ShowAdmobOrHeyzap(true, MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementStartEndChallenges, MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementStartEndChallenges);
		}
	}

	public void DisableAdsFromChallange()
	{
		if (!disabledAds)
		{
			CancelInvoke("ShowActionPhaseAd");
			CancelInvoke("AdTime");
			CancelInvoke("ShowCounterAds");
			CancelPnMsg();
			disableFromChallange = true;
		}
	}

	public void EnableAds()
	{
		if (!disabledAds)
		{
			disableFromChallange = false;
			RestartCounter();
		}
	}
}
