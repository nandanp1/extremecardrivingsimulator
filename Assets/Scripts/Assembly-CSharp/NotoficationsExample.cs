using UnityEngine;

public class NotoficationsExample : MonoBehaviour
{
	public const string SENDER_ID = "YOUR_SENDER_ID_HERE";

	private void Awake()
	{
		Singleton<GoogleCloudMessageService>.instance.addEventListener("cloud_message_service_registration_failed", OnRegFailed);
		Singleton<GoogleCloudMessageService>.instance.addEventListener("cloud_message_service_registration_recived", OnRegstred);
		Singleton<GoogleCloudMessageService>.instance.addEventListener("cloud_message_loaded", OnMessageLoaded);
	}

	private void Toast()
	{
		AndroidToast.ShowToastNotification("Hello Toast", 1);
	}

	private void Local()
	{
		AndroidNotificationManager.ScheduleLocalNotification("Hello", "This is local notification", 5);
	}

	private void Reg()
	{
		Singleton<GoogleCloudMessageService>.instance.RgisterDevice("YOUR_SENDER_ID_HERE");
	}

	private void LoadLastMessage()
	{
		Singleton<GoogleCloudMessageService>.instance.LoadLastMessage();
	}

	private void OnRegFailed()
	{
		AndroidNative.showMessage("Reg Failed", "GCM Registration failed :(");
	}

	private void OnRegstred()
	{
		AndroidNative.showMessage("Regstred", "GCM REG ID: " + Singleton<GoogleCloudMessageService>.instance.registrationId);
	}

	private void OnMessageLoaded()
	{
		AndroidNative.showMessage("Message Loaded", "Last GCM Message: " + Singleton<GoogleCloudMessageService>.instance.lastMessage);
	}
}
