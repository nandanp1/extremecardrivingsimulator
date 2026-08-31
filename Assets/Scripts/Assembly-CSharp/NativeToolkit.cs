using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SecondFury.NativeToolkit.MiniJSON;
using UnityEngine;

public class NativeToolkit : MonoBehaviour
{
	private enum ImageType
	{
		IMAGE = 0,
		SCREENSHOT = 1
	}

	private enum SaveStatus
	{
		NOTSAVED = 0,
		SAVED = 1,
		DENIED = 2,
		TIMEOUT = 3
	}

	private static NativeToolkit instance;

	private static GameObject go;

	private static AndroidJavaClass obj;

	public static NativeToolkit Instance
	{
		get
		{
			if (instance == null)
			{
				go = new GameObject();
				go.name = "NativeToolkit";
				instance = go.AddComponent<NativeToolkit>();
				if (Application.platform == RuntimePlatform.Android)
				{
					obj = new AndroidJavaClass("com.secondfury.nativetoolkit.Main");
				}
			}
			return instance;
		}
	}

	public static event Action<Texture2D> OnScreenshotTaken;

	public static event Action<string> OnScreenshotSaved;

	public static event Action<string> OnImageSaved;

	public static event Action<Texture2D, string> OnImagePicked;

	public static event Action<bool> OnDialogComplete;

	public static event Action<string> OnRateComplete;

	public static event Action<Texture2D, string> OnCameraShotComplete;

	public static event Action<string, string, string> OnContactPicked;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static void SaveScreenshot(string fileName, string albumName = "MyScreenshots", string fileType = "jpg", Rect screenArea = default(Rect))
	{
		Debug.Log("Save screenshot to gallery " + fileName);
		if (screenArea == default(Rect))
		{
			screenArea = new Rect(0f, 0f, Screen.width, Screen.height);
		}
		Instance.StartCoroutine(Instance.GrabScreenshot(fileName, albumName, fileType, screenArea));
	}

	private IEnumerator GrabScreenshot(string fileName, string albumName, string fileType, Rect screenArea)
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture = new Texture2D((int)screenArea.width, (int)screenArea.height, TextureFormat.RGB24, false);
		texture.ReadPixels(screenArea, 0, 0);
		texture.Apply();
		byte[] bytes;
		string fileExt;
		if (fileType == "png")
		{
			bytes = texture.EncodeToPNG();
			fileExt = ".png";
		}
		else
		{
			bytes = texture.EncodeToJPG();
			fileExt = ".jpg";
		}
		if (OnScreenshotTaken != null)
		{
			OnScreenshotTaken(texture);
		}
		else
		{
			UnityEngine.Object.Destroy(texture);
		}
		string date = DateTime.Now.ToString("hh-mm-ss_dd-MM-yy");
		string screenshotFilename = fileName + "_" + date + fileExt;
		string path = Application.persistentDataPath + "/" + screenshotFilename;
		if (Application.platform == RuntimePlatform.Android)
		{
			path = Path.Combine(path2: Path.Combine(albumName, screenshotFilename), path1: Application.persistentDataPath);
			string pathonly = Path.GetDirectoryName(path);
			Directory.CreateDirectory(pathonly);
		}
		Instance.StartCoroutine(Instance.Save(bytes, fileName, path, ImageType.SCREENSHOT));
	}

	public static void SaveImage(Texture2D texture, string fileName, string fileType = "jpg")
	{
		Debug.Log("Save image to gallery " + fileName);
		Instance.Awake();
		byte[] bytes;
		string text;
		if (fileType == "png")
		{
			bytes = texture.EncodeToPNG();
			text = ".png";
		}
		else
		{
			bytes = texture.EncodeToJPG();
			text = ".jpg";
		}
		string path = Application.persistentDataPath + "/" + fileName + text;
		Instance.StartCoroutine(Instance.Save(bytes, fileName, path, ImageType.IMAGE));
	}

	private IEnumerator Save(byte[] bytes, string fileName, string path, ImageType imageType)
	{
		int count = 0;
		SaveStatus saved = SaveStatus.NOTSAVED;
		if (Application.platform == RuntimePlatform.Android)
		{
			File.WriteAllBytes(path, bytes);
			while (saved == SaveStatus.NOTSAVED)
			{
				count++;
				saved = ((count <= 30) ? ((SaveStatus)obj.CallStatic<int>("addImageToGallery", new object[1] { path })) : SaveStatus.TIMEOUT);
				yield return Instance.StartCoroutine(Instance.Wait(0.5f));
			}
		}
		switch (saved)
		{
		case SaveStatus.DENIED:
			path = "DENIED";
			break;
		case SaveStatus.TIMEOUT:
			path = "TIMEOUT";
			break;
		}
		switch (imageType)
		{
		case ImageType.IMAGE:
			if (OnImageSaved != null)
			{
				OnImageSaved(path);
			}
			break;
		case ImageType.SCREENSHOT:
			if (OnScreenshotSaved != null)
			{
				OnScreenshotSaved(path);
			}
			break;
		}
	}

	public static void PickImage()
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("pickImageFromGallery");
		}
	}

	public void OnPickImage(string path)
	{
		StartCoroutine(LoadPickImage(path));
	}

	private IEnumerator LoadPickImage(string path)
	{
		Texture2D texture = LoadImageFromFile(path);
		if (OnImagePicked != null)
		{
			OnImagePicked(texture, path);
		}
		yield return 0;
	}

	public static void TakeCameraShot()
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("takeCameraShot");
		}
	}

	public void OnCameraFinished(string path)
	{
		StartCoroutine(LoadCameraImage(path));
	}

	private IEnumerator LoadCameraImage(string path)
	{
		Texture2D texture = LoadImageFromFile(path);
		if (OnCameraShotComplete != null)
		{
			OnCameraShotComplete(texture, path);
		}
		yield return 0;
	}

	public static void PickContact()
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("pickContact");
		}
	}

	public void OnPickContactFinished(string data)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(data) as Dictionary<string, object>;
		string arg = string.Empty;
		string arg2 = string.Empty;
		string arg3 = string.Empty;
		if (dictionary.ContainsKey("name"))
		{
			arg = dictionary["name"].ToString();
		}
		if (dictionary.ContainsKey("number"))
		{
			arg2 = dictionary["number"].ToString();
		}
		if (dictionary.ContainsKey("email"))
		{
			arg3 = dictionary["email"].ToString();
		}
		if (OnContactPicked != null)
		{
			OnContactPicked(arg, arg2, arg3);
		}
	}

	public static void SendEmail(string subject, string body, string pathToImageAttachment = "", string to = "", string cc = "", string bcc = "")
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("sendEmail", to, cc, bcc, subject, body, pathToImageAttachment);
		}
	}

	public static void ShowConfirm(string title, string message, Action<bool> callback = null, string positiveBtnText = "Ok", string negativeBtnText = "Cancel")
	{
		Instance.Awake();
		OnDialogComplete = callback;
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("showConfirm", title, message, positiveBtnText, negativeBtnText);
		}
	}

	public static void ShowAlert(string title, string message, Action<bool> callback = null, string btnText = "Ok")
	{
		Instance.Awake();
		OnDialogComplete = callback;
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("showAlert", title, message, btnText);
		}
	}

	public void OnDialogPress(string result)
	{
		if (OnDialogComplete != null)
		{
			if (result == "Yes")
			{
				OnDialogComplete(true);
			}
			else if (result == "No")
			{
				OnDialogComplete(false);
			}
		}
	}

	public static void RateApp(string title = "Rate This App", string message = "Please take a moment to rate this App", string positiveBtnText = "Rate Now", string neutralBtnText = "Later", string negativeBtnText = "No, Thanks", string appleId = "", Action<string> callback = null)
	{
		Instance.Awake();
		OnRateComplete = callback;
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("rateThisApp", title, message, positiveBtnText, neutralBtnText, negativeBtnText);
		}
	}

	public void OnRatePress(string result)
	{
		if (OnRateComplete != null)
		{
			OnRateComplete(result);
		}
	}

	public static string GetCountryCode()
	{
		Instance.Awake();
		string result = null;
		if (Application.platform == RuntimePlatform.Android)
		{
			result = obj.CallStatic<string>("getLocale", new object[0]);
		}
		return result;
	}

	public static void ScheduleLocalNotification(string title, string message, int id = 0, int delayInMinutes = 0, string sound = "default_sound", bool vibrate = false, string smallIcon = "ic_notification", string largeIcon = "ic_notification_large")
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("scheduleLocalNotification", title, message, id, delayInMinutes, sound, vibrate, smallIcon, largeIcon);
		}
	}

	public static void ClearLocalNotification(int id)
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("clearLocalNotification", id);
		}
	}

	public static void ClearAllLocalNotifications()
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			obj.CallStatic("clearAllLocalNotifications");
		}
	}

	public static bool WasLaunchedFromNotification()
	{
		Instance.Awake();
		if (Application.platform == RuntimePlatform.Android)
		{
			return obj.CallStatic<bool>("wasLaunchedFromNotification", new object[0]);
		}
		return false;
	}

	public static Texture2D LoadImageFromFile(string path)
	{
		if (path == "Cancelled")
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(128, 128, TextureFormat.RGB24, false);
		byte[] data = File.ReadAllBytes(path);
		texture2D.LoadImage(data);
		return texture2D;
	}

	private IEnumerator Wait(float delay)
	{
		float pauseTarget = Time.realtimeSinceStartup + delay;
		while (Time.realtimeSinceStartup < pauseTarget)
		{
			yield return null;
		}
	}
}
