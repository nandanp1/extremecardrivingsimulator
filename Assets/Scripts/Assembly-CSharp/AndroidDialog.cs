using System;
using UnityEngine;

public class AndroidDialog : BaseAndroidPopup
{
	public string yes;

	public string no;

	public static AndroidDialog Create(string title, string message)
	{
		return Create(title, message, "Yes", "No");
	}

	public static AndroidDialog Create(string title, string message, string yes, string no)
	{
		AndroidDialog androidDialog = new GameObject("AndroidPopUp").AddComponent<AndroidDialog>();
		androidDialog.title = title;
		androidDialog.message = message;
		androidDialog.yes = yes;
		androidDialog.no = no;
		androidDialog.init();
		return androidDialog;
	}

	public void init()
	{
		AndroidNative.showDialog(title, message, yes, no);
	}

	public void onPopUpCallBack(string buttonIndex)
	{
		switch ((int)Convert.ToInt16(buttonIndex))
		{
		case 0:
			dispatch("complete", AndroidDialogResult.YES);
			break;
		case 1:
			dispatch("complete", AndroidDialogResult.NO);
			break;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
