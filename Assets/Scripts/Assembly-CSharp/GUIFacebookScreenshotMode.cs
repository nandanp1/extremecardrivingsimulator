using UnityEngine;

public class GUIFacebookScreenshotMode : MonoBehaviour
{
	private static GUIFacebookScreenshotMode instance;

	[SerializeField]
	private UIPanel pnScreenshotMode;

	[SerializeField]
	private UIPanel pnScreenshotSending;

	[SerializeField]
	private UIPanel pnScreenshotSuccess;

	[SerializeField]
	private UIPanel pnScreenshotFailed;

	[SerializeField]
	public FacebookScreenshotMode facebookScreenshot;

	[SerializeField]
	private UIButton btScreenshot;

	[SerializeField]
	private UIButton btTakeScreenshot;

	[SerializeField]
	private UIPanel pnScreenshotDecorators;

	[SerializeField]
	private UIButton btResolverOnFBKMinimize;

	[SerializeField]
	private UIPanel pnScreenshotPreview;

	[SerializeField]
	private UIInput inMessage;

	private bool _isScreenshotSendingTimedOut;

	public GameObject arrow;

	public ChallangeModeManager challengeModeManager;

	private Camera _lastCamera;

	[SerializeField]
	public GUIFacebookScreenshotModeExtension guiExtension;

	public static GUIFacebookScreenshotMode Instance()
	{
		if (instance == null)
		{
			instance = Object.FindFirstObjectByType<GUIFacebookScreenshotMode>();
			if (!instance)
			{
				Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
			}
		}
		return instance;
	}

	private void Awake()
	{
	}

	private void Start()
	{
		challengeModeManager = Object.FindFirstObjectByType<ChallangeModeManager>();
		if (guiExtension == null)
		{
			Debug.LogError("guiExtension NECESITA INICIALIZARSE ANTES DE USARSE");
		}
	}

	public void ScreenshotMode()
	{
		MySingleton<Manager>.Instance.guiCurrentLogic.ShowAchievementsPanel(AchievementManager.AchievementCategory.GENERAL);
		SetActiveScreenshotMode(true);
		EnableScreenshotCamera();
		guiExtension.PreInitMode();
		guiExtension.SetBackButtonFunctionToCancelScreenshot();
		HideAllPanels();
		HideArrow();
		facebookScreenshot.Initialize(OnFacebookScreenshotModeInitialized, OnBeforePerformCameraRotation);
	}

	public void CancelScreenshot()
	{
		MySingleton<Manager>.Instance.ads.ShowBanner();
		HideAllPanels();
		ShowArrow();
		facebookScreenshot.CancelScreenshot(OnFacebookScreenshotModeFinalized);
	}

	public void TakeScreenshot()
	{
		MySingleton<Manager>.Instance.ads.HideBanner();
		guiExtension.SetBackButtonFunctionToNone();
		HideAllPanels();
		NGUITools.SetActive(pnScreenshotDecorators.gameObject, true);
		_isScreenshotSendingTimedOut = false;
		facebookScreenshot.TakeScreenshot(OnFacebookScreenshotModeTaken);
	}

	public void SubmitScreenshot()
	{
		HideAllPanels();
		facebookScreenshot.FeedScreenshot(inMessage.value, OnFacebookScreenshotModeFinalized, OnFacebookScreenshotSending, OnFacebookScreenshotSent, OnFacebookScreenshotFailed, OnFacebookScreenshotTimedout);
	}

	public void OnFacebookScreenshotModeFinalized()
	{
		DisableScreenshotCamera();
		HideAllPanels();
		guiExtension.SetBackButtonFunctionToLast();
		guiExtension.PostFinalizeMode();
		SetActiveScreenshotMode(false);
	}

	public void OnFacebookScreenshotModeInitialized()
	{
		MySingleton<Manager>.Instance.ads.ShowBanner();
		HideAllPanels();
		NGUITools.SetActive(pnScreenshotMode.gameObject, true);
		guiExtension.SetBackButtonFunctionToCancelScreenshot();
		inMessage.value = string.Empty;
	}

	public void OnFacebookScreenshotModeTaken()
	{
		HideAllPanels();
		NGUITools.SetActive(pnScreenshotPreview.gameObject, true);
	}

	public void OnFacebookScreenshotSending()
	{
		HideAllPanels();
		Debug.Log("FacebookSending");
		NGUITools.SetActive(pnScreenshotSending.gameObject, true);
	}

	public void OnFacebookScreenshotSent()
	{
		Debug.Log("timeout: " + _isScreenshotSendingTimedOut);
		if (!_isScreenshotSendingTimedOut)
		{
			HideAllPanels();
			Debug.Log("Facebook success");
			NGUITools.SetActive(pnScreenshotSuccess.gameObject, true);
			guiExtension.SetBackButtonFunctionToCancelScreenshot();
			if (MySingleton<AchievementManager>.Instance.ApplyAchivement(AchievementManager.Achievement.GENERAL_SHARE_SCREENSHOT_CAR_ON_FACEBOOK))
			{
				MySingleton<Manager>.Instance.guiGameLogic.ActiveAchievementPanel(true);
			}
		}
		_isScreenshotSendingTimedOut = false;
	}

	public void OnFacebookScreenshotFailed()
	{
		if (!_isScreenshotSendingTimedOut)
		{
			HideAllPanels();
			NGUITools.SetActive(pnScreenshotFailed.gameObject, true);
			guiExtension.SetBackButtonFunctionToResumeScreenshot();
		}
		_isScreenshotSendingTimedOut = false;
	}

	public void OnFacebookScreenshotTimedout()
	{
		Debug.Log("FacebookScreenshotTimedOut");
		_isScreenshotSendingTimedOut = true;
		ResumeScreenshot();
	}

	public void ResumeScreenshot()
	{
		HideAllPanels();
		NGUITools.SetActive(pnScreenshotMode.gameObject, true);
		MySingleton<Manager>.Instance.ads.ShowBanner();
		guiExtension.SetBackButtonFunctionToCancelScreenshot();
		facebookScreenshot.EnableTouch();
	}

	public void OnFacebookMinimizeResumeScreenshot()
	{
	}

	private void HideAllPanels()
	{
		Debug.Log("Facebook HideAll");
		NGUITools.SetActive(pnScreenshotSuccess.gameObject, false);
		NGUITools.SetActive(pnScreenshotFailed.gameObject, false);
		NGUITools.SetActive(pnScreenshotSending.gameObject, false);
		NGUITools.SetActive(pnScreenshotDecorators.gameObject, false);
		NGUITools.SetActive(pnScreenshotSuccess.gameObject, false);
		NGUITools.SetActive(pnScreenshotPreview.gameObject, false);
		NGUITools.SetActive(pnScreenshotMode.gameObject, false);
	}

	private void ShowArrow()
	{
		if (challengeModeManager != null && challengeModeManager.IsInCircuit() && MySingleton<Manager>.Instance.challangeModeManager.currentChallange.type == ChallangeType.Radar)
		{
			NGUITools.SetActive(arrow, true);
		}
	}

	private void HideArrow()
	{
		NGUITools.SetActive(arrow, false);
	}

	public void OnBeforePerformCameraRotation()
	{
		guiExtension.BeforePerformCameraRotation();
	}

	public void SetTarget(Transform target)
	{
		facebookScreenshot.SetTarget(target);
	}

	public Transform GetTarget()
	{
		return facebookScreenshot.GetTarget();
	}

	public Vector3 GetRelativeCameraToTarget()
	{
		return facebookScreenshot.camera.transform.position - facebookScreenshot.GetTarget().position;
	}

	private void EnableScreenshotCamera()
	{
		_lastCamera = ((!(Camera.main != null)) ? guiExtension.GetCurrentCamera() : Camera.main);
		facebookScreenshot.camera.transform.position = _lastCamera.transform.position;
		facebookScreenshot.camera.transform.rotation = _lastCamera.transform.rotation;
		facebookScreenshot.camera.gameObject.SetActive(true);
		_lastCamera.gameObject.SetActive(false);
	}

	private void DisableScreenshotCamera()
	{
		if (_lastCamera != null)
		{
			facebookScreenshot.camera.gameObject.SetActive(false);
			_lastCamera.gameObject.SetActive(true);
			_lastCamera = null;
		}
	}

	public void SetGroundLimit(Vector3 groundLimit)
	{
		facebookScreenshot.groundLimit = groundLimit;
	}

	public void SetActiveScreenshotMode(bool activate)
	{
		facebookScreenshot.gameObject.SetActive(activate);
	}
}
