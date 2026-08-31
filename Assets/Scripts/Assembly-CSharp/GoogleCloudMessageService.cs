using UnityEngine;

public class GoogleCloudMessageService : Singleton<GoogleCloudMessageService>
{
	public const string CLOUD_MESSAGE_SERVICE_REGISTRATION_FAILED = "cloud_message_service_registration_failed";

	public const string CLOUD_MESSAGE_SERVICE_REGISTRATION_RECIVED = "cloud_message_service_registration_recived";

	public const string CLOUD_MESSAGE_LOADED = "cloud_message_loaded";

	private string _lastMessage = string.Empty;

	private string _registrationId = string.Empty;

	public string registrationId
	{
		get
		{
			return _registrationId;
		}
	}

	public string lastMessage
	{
		get
		{
			return _lastMessage;
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void RgisterDevice(string senderId)
	{
		AndroidNative.GCMRgisterDevice(senderId);
	}

	public void LoadLastMessage()
	{
		AndroidNative.GCMLoadLastMessage();
	}

	private void OnLastMessageLoaded(string data)
	{
		_lastMessage = data;
		dispatch("cloud_message_loaded", lastMessage);
	}

	private void OnRegistrationReviced(string regId)
	{
		_registrationId = regId;
		dispatch("cloud_message_service_registration_recived", regId);
	}

	private void OnRegistrationFailed()
	{
		dispatch("cloud_message_service_registration_failed");
	}
}
