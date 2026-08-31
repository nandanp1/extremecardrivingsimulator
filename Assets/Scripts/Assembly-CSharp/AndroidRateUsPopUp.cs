using System;
using UnityEngine;

public class AndroidRateUsPopUp : BaseAndroidPopup
{
	public string yes;

	public string later;

	public string no;

	public string url;

	public static AndroidRateUsPopUp Create(string title, string message, string url)
	{
		return Create(title, message, url, "Rate app", "Later", "No, thanks");
	}

	public static AndroidRateUsPopUp Create(string title, string message, string url, string yes, string later, string no)
	{
		AndroidRateUsPopUp androidRateUsPopUp = new GameObject("AndroidRateUsPopUp").AddComponent<AndroidRateUsPopUp>();
		androidRateUsPopUp.title = title;
		androidRateUsPopUp.message = message;
		androidRateUsPopUp.url = url;
		androidRateUsPopUp.yes = yes;
		androidRateUsPopUp.later = later;
		androidRateUsPopUp.no = no;
		androidRateUsPopUp.init();
		return androidRateUsPopUp;
	}

	public void init()
	{
		AndroidNative.showRateDialog(title, message, yes, later, no, url);
	}

	public void onPopUpCallBack(string buttonIndex)
	{
		switch ((int)Convert.ToInt16(buttonIndex))
		{
		case 0:
			dispatch("complete", AndroidDialogResult.RATED);
			break;
		case 1:
			dispatch("complete", AndroidDialogResult.REMIND);
			break;
		case 2:
			dispatch("complete", AndroidDialogResult.DECLINED);
			break;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
