using UnityEngine;

public class AndroidHelper
{
	private static AndroidHelper _instance;

	private AndroidJavaObject androidHelper;

	public static AndroidHelper instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new AndroidHelper();
			}
			return _instance;
		}
	}

	private AndroidHelper()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.axesinmotion.androidhelper.Main");
		androidHelper = androidJavaClass2.CallStatic<AndroidJavaObject>("instance", new object[0]);
		androidHelper.Call("SetContext", androidJavaObject);
		Debug.Log("AndroidHelper v" + androidHelper.Get<double>("VERSION") + " inicializado.");
	}

	public bool IsWifiNetworkAvailable()
	{
		return androidHelper.Call<bool>("isWifiNetworkAvailable", new object[0]);
	}

	public bool IsMobileNetworkAvailable()
	{
		return androidHelper.Call<bool>("isMobileNetworkAvailable", new object[0]);
	}

	public bool IsFastConnection()
	{
		return androidHelper.Call<bool>("isConnectedFast", new object[0]);
	}

	public string GetNameOf(AndroidJavaObject obj)
	{
		return androidHelper.Call<string>("GetNameOf", new object[1] { obj });
	}
}
