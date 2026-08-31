using UnityEngine;

public class NativeToolkitExample : MonoBehaviour
{
	public UILabel console;

	public Texture2D texture;

	private string imagePath = string.Empty;

	private void Start()
	{
		console.text = string.Empty;
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nDevice country: " + NativeToolkit.GetCountryCode();
		UILabel uILabel2 = console;
		uILabel2.text = uILabel2.text + "\nLaunched from notification: " + NativeToolkit.WasLaunchedFromNotification();
	}

	private void OnEnable()
	{
		NativeToolkit.OnScreenshotSaved += ScreenshotSaved;
		NativeToolkit.OnImageSaved += ImageSaved;
		NativeToolkit.OnImagePicked += ImagePicked;
		NativeToolkit.OnCameraShotComplete += CameraShotComplete;
		NativeToolkit.OnContactPicked += ContactPicked;
	}

	private void OnDisable()
	{
		NativeToolkit.OnScreenshotSaved -= ScreenshotSaved;
		NativeToolkit.OnImageSaved -= ImageSaved;
		NativeToolkit.OnImagePicked -= ImagePicked;
		NativeToolkit.OnCameraShotComplete -= CameraShotComplete;
		NativeToolkit.OnContactPicked -= ContactPicked;
	}

	private void Update()
	{
		console.text = string.Empty;
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nDevice country: " + NativeToolkit.GetCountryCode();
		UILabel uILabel2 = console;
		uILabel2.text = uILabel2.text + "\nLaunched from notification: " + NativeToolkit.WasLaunchedFromNotification();
	}

	public void OnSaveScreenshotPress()
	{
		NativeToolkit.SaveScreenshot("MyScreenshot", "MyScreenshotFolder", "jpeg");
	}

	public void OnSaveImagePress()
	{
		NativeToolkit.SaveImage(texture, "MyImage", "png");
	}

	public void OnPickImagePress()
	{
		NativeToolkit.PickImage();
	}

	public void OnEmailSharePress()
	{
		NativeToolkit.SendEmail("Hello there", "<html><body><b>This is an email sent from my App!</b></body></html>", imagePath, string.Empty, string.Empty, string.Empty);
	}

	public void OnCameraPress()
	{
		NativeToolkit.TakeCameraShot();
	}

	public void OnPickContactPress()
	{
		NativeToolkit.PickContact();
	}

	public void OnShowAlertPress()
	{
		NativeToolkit.ShowAlert("Native Toolkit", "This is an alert dialog!", DialogFinished);
	}

	public void OnShowDialogPress()
	{
		NativeToolkit.ShowConfirm("Native Toolkit", "This is a confirm dialog!", DialogFinished);
	}

	public void OnLocalNotificationPress()
	{
		Debug.Log("NOTIFICATION!");
		string text = "This is a local notification! This is a super long one to show how long we can make a notification. On Android this will appear as an extended notification.";
		NativeToolkit.ScheduleLocalNotification("Titulo de prueba", "Esto es un mensaje de prueba", 1, 1);
	}

	public void OnClearNotificationsPress()
	{
		NativeToolkit.ClearAllLocalNotifications();
	}

	public void OnGetLocationPress()
	{
	}

	public void OnRateAppPress()
	{
		NativeToolkit.RateApp("Rate This App", "Please take a moment to rate this App", "Rate Now", "Later", "No, Thanks", "343200656", AppRated);
	}

	private void ScreenshotSaved(string path)
	{
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nScreenshot saved to: " + path;
	}

	private void ImageSaved(string path)
	{
		UILabel uILabel = console;
		string text = uILabel.text;
		uILabel.text = text + "\n" + texture.name + " saved to: " + path;
	}

	private void ImagePicked(Texture2D img, string path)
	{
		imagePath = path;
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nImage picked at: " + imagePath;
		Object.Destroy(img);
	}

	private void CameraShotComplete(Texture2D img, string path)
	{
		imagePath = path;
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nCamera shot saved to: " + imagePath;
		Object.Destroy(img);
	}

	private void DialogFinished(bool result)
	{
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nDialog returned: " + result;
	}

	private void AppRated(string result)
	{
		UILabel uILabel = console;
		uILabel.text = uILabel.text + "\nRate this app result: " + result;
	}

	private void ContactPicked(string name, string number, string email)
	{
		UILabel uILabel = console;
		string text = uILabel.text;
		uILabel.text = text + "\nContact Details:\nName:" + name + ", number:" + number + ", email:" + email;
	}
}
