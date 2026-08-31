using UnityEngine;

public class BaseAndroidPopup : EventDispatcher
{
	public string title;

	public string message;

	public void onDismissed(string data)
	{
		dispatch("complete", AndroidDialogResult.CLOSED);
		Object.Destroy(base.gameObject);
	}
}
