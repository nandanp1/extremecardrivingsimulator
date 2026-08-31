using UnityEngine;

public class AndroidMessage : BaseAndroidPopup
{
	public string ok;

	public static AndroidMessage Create(string title, string message)
	{
		return Create(title, message, "Ok");
	}

	public static AndroidMessage Create(string title, string message, string ok)
	{
		AndroidMessage androidMessage = new GameObject("AndroidPopUp").AddComponent<AndroidMessage>();
		androidMessage.title = title;
		androidMessage.message = message;
		androidMessage.ok = ok;
		androidMessage.init();
		return androidMessage;
	}

	public void init()
	{
		AndroidNative.showMessage(title, message, ok);
	}

	public void onPopUpCallBack(string buttonIndex)
	{
		dispatch("complete");
		Object.Destroy(base.gameObject);
	}
}
