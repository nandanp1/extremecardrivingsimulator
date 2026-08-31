using System;
using System.Collections;
using UnityEngine;

public class Admob_Universal : MonoBehaviour
{
	[HideInInspector]
	public bool intersitialLoaded;

	public static string ADMOB_BANNER_ID = "ca-app-pub-1450045239311598/1039312267";

	public static string ADMOB_INTERSTITIAL_ID = "ca-app-pub-1450045239311598/2516045464";

	public bool debug = true;

	private static bool _instanceFound;

	private bool initialized;

	public bool admobFromPauseFlag;

	public bool handleOnAdBannerFailedToLoadFlag;

	public bool handleOnAdBannerLoadedFlag;

	public bool handeOnAdInterstitialClosedFlag;

	private bool bannerWasSuccesfullyLoaded;

	public void Init()
	{
		initialized = false;
		intersitialLoaded = false;
	}

	public void RequestBanner()
	{
	}

	public void RequestInterstitial()
	{
	}

	public bool IsAdmobAdLoaded()
	{
		return false;
	}

	public void ShowIntersitialAd()
	{
	}

	private void _ShowAdmobInterstitial()
	{
	}

	private IEnumerator SecurityPnPauseEnabled()
	{
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(2f));
		if (MySingleton<Manager>.Instance.paused)
		{
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnPause.gameObject, true);
		}
	}

	private IEnumerator SecurityPnGarajeEnabled()
	{
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(1f));
		NGUITools.SetActive(MySingleton<Manager>.Instance.guiMenuLogic.pnSelectCar.gameObject, true);
	}

	public void ShowBanner()
	{
	}

	private IEnumerator ShowBannerCoroutine()
	{
		yield return null;
	}

	public void HideBanner()
	{
	}

	public void DisableAds()
	{
	}

	public void HandleOnAdBannerFailedToLoad(object sender, EventArgs args)
	{
		handleOnAdBannerFailedToLoadFlag = true;
	}

	public void HandleOnAdBannerLoaded(object sender, EventArgs args)
	{
		handleOnAdBannerLoadedFlag = true;
	}

	public void HandleOnAdInterstitialFailedToLoad(object sender, EventArgs args)
	{
	}

	public void HandleOnAdInterstitialLoaded(object sender, EventArgs args)
	{
	}

	public void HandleOnAdInterstitialShow(object sender, EventArgs args)
	{
	}

	public void HandeOnAdInterstitialClosed(object sender, EventArgs args)
	{
		handeOnAdInterstitialClosedFlag = true;
	}

	private void Update()
	{
		if (handeOnAdInterstitialClosedFlag)
		{
			handeOnAdInterstitialClosedFlag = false;
			AdmobClosedGuiLogic();
		}
		if (handleOnAdBannerFailedToLoadFlag && !bannerWasSuccesfullyLoaded)
		{
			handleOnAdBannerFailedToLoadFlag = false;
			CancelInvoke("RequestBanner");
			Invoke("RequestBanner", 5f);
		}
		if (handleOnAdBannerLoadedFlag)
		{
			CancelInvoke("RequestBanner");
			handleOnAdBannerLoadedFlag = false;
			bannerWasSuccesfullyLoaded = true;
		}
	}

	private void AdmobClosedGuiLogic()
	{
		if (MySingleton<Manager>.Instance.admobFromPause)
		{
			if ((bool)MySingleton<Manager>.Instance.guiGameLogic && (bool)MySingleton<Manager>.Instance.guiGameLogic.pnPause)
			{
				NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnPause.gameObject, true);
			}
			MySingleton<Manager>.Instance.admobFromPause = false;
		}
		if (MySingleton<Manager>.Instance.admobFromMenu)
		{
			if ((bool)MySingleton<Manager>.Instance.guiMenuLogic && (bool)MySingleton<Manager>.Instance.guiMenuLogic.pnSelectCar)
			{
				NGUITools.SetActive(MySingleton<Manager>.Instance.guiMenuLogic.pnSelectCar.gameObject, true);
			}
			MySingleton<Manager>.Instance.admobFromMenu = false;
		}
		MySingleton<Manager>.Instance.AdsEnded();
	}

	private void RetryLoadBanner()
	{
		if (!bannerWasSuccesfullyLoaded)
		{
			CancelInvoke("RequestBanner");
			Invoke("RequestBanner", 5f);
		}
	}
}
