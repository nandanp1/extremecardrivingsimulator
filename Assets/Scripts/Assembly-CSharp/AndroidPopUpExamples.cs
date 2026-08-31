using UnityEngine;

public class AndroidPopUpExamples : MonoBehaviour
{
	private string rateText = "If you enjoy using Google Earth, please take a moment to rate it. Thanks for your support!";

	private string rateUrl = "market://details?id=com.google.earth";

	private void RateDialogPopUp()
	{
		AndroidRateUsPopUp androidRateUsPopUp = AndroidRateUsPopUp.Create("Rate Us", rateText, rateUrl);
		androidRateUsPopUp.addEventListener("complete", OnRatePopUpClose);
	}

	private void DialogPopUp()
	{
		AndroidDialog androidDialog = AndroidDialog.Create("Dialog Titile", "Dialog message");
		androidDialog.addEventListener("complete", OnDialogClose);
	}

	private void MessagePopUp()
	{
		AndroidMessage androidMessage = AndroidMessage.Create("Message Titile", "Message message");
		androidMessage.addEventListener("complete", OnMessageClose);
	}

	private void ShowPreloader()
	{
		Invoke("HidePreloader", 2f);
		AndroidNativeUtility.ShowPreloader("Loading", "Wait 2 seconds please");
	}

	private void HidePreloader()
	{
		AndroidNativeUtility.HidePreloader();
	}

	private void OnRatePopUpClose(CEvent e)
	{
		(e.dispatcher as AndroidRateUsPopUp).removeEventListener("complete", OnRatePopUpClose);
		string text = e.data.ToString();
		AndroidNative.showMessage("Result", text + " button pressed");
	}

	private void OnDialogClose(CEvent e)
	{
		(e.dispatcher as AndroidDialog).removeEventListener("complete", OnDialogClose);
		switch ((AndroidDialogResult)(int)e.data)
		{
		case AndroidDialogResult.YES:
			Debug.Log("Yes button pressed");
			break;
		case AndroidDialogResult.NO:
			Debug.Log("Yes button pressed");
			break;
		}
		AndroidNative.showMessage("Result", e.data.ToString() + " button pressed");
	}

	private void OnMessageClose(CEvent e)
	{
		(e.dispatcher as AndroidMessage).removeEventListener("complete", OnMessageClose);
		AndroidNative.showMessage("Result", "Message Closed");
	}
}
