using AxesInMotion.SocialPlugin;
using AxesInMotion.UnityHelpers;
using UnityEngine;

public class FacebookManager : MonoBehaviour
{
	public delegate void SocialBeforeFeedCallbackDelegate();

	public string inviteTitleLocalizationKey = "facebook_invite_title";

	public string inviteMessageLocalizationKey = "facebook_invite_message";

	public int inviteMaxRecipients = 20;

	private ISocial _social;

	private SocialCustomCallbackDelegate _onSuccess;

	private SocialCustomCallbackDelegate _onCancel;

	private SocialCustomCallbackDelegate _onFailed;

	private SocialCustomCallbackDelegate _onTakeScreenshotSuccess;

	private SocialCustomCallbackDelegate _onTakeScreenshotFailed;

	public bool isLoggedIn
	{
		get
		{
			return false;
		}
	}

	public bool isInitialized
	{
		get
		{
			return false;
		}
	}

	public string ID
	{
		get
		{
			return string.Empty;
		}
	}

	private void Start()
	{
		MySingleton<Manager>.Instance.facebookManager = this;
		MySingleton<Manager>.Instance.isFacebookAvailable = false;
	}

	private void Initialize(SocialInitializedCallbackDelegate initializeCallback)
	{
		if (initializeCallback != null)
		{
			SocialEventArgs socialEventArgs = new SocialEventArgs(SocialEventType.Initialized);
			socialEventArgs.Error = true;
			socialEventArgs.Message = "Facebook is unavailable in the offline build.";
			initializeCallback(socialEventArgs);
		}
	}

	private void InitializeCallback(ISocialEventArgs args)
	{
	}

	private void CustomLoginCallback(ISocialEventArgs args)
	{
		if (!args.Error)
		{
			if (args.Type == SocialEventType.LoginSuccess)
			{
				_onSuccess();
			}
			else if (args.Type == SocialEventType.LoginCanceled)
			{
				_onCancel();
			}
			else
			{
				Debug.LogError("args.Type no implementado");
			}
		}
		else
		{
			_onFailed();
		}
	}

	public void Login(SocialCustomCallbackDelegate successCallback, SocialCustomCallbackDelegate cancelCallback, SocialCustomCallbackDelegate failedCallback, SocialScope[] scopes = null)
	{
		_onSuccess = successCallback;
		_onFailed = failedCallback;
		_onCancel = cancelCallback;
		if (_onFailed != null)
		{
			_onFailed();
		}
	}

	public void Logout()
	{
	}

	public void FeedImage(string message, Texture2D texture, SocialCustomCallbackDelegate successCallback, SocialCustomCallbackDelegate failedCallback)
	{
		_onTakeScreenshotSuccess = successCallback;
		_onTakeScreenshotFailed = failedCallback;
		if (_onTakeScreenshotFailed != null)
		{
			_onTakeScreenshotFailed();
		}
	}

	private void TakeScreenshotCallback(ISocialEventArgs args)
	{
		if (!args.Error)
		{
			Debug.Log("Take OK");
			if (_onTakeScreenshotSuccess != null)
			{
				_onTakeScreenshotSuccess();
			}
		}
		else
		{
			Debug.Log("Take Error");
			if (_onTakeScreenshotFailed != null)
			{
				_onTakeScreenshotFailed();
			}
		}
		_onTakeScreenshotSuccess = null;
		_onTakeScreenshotFailed = null;
	}

	public void InviteFriends()
	{
	}

	private void OnInviteFriendsCallback(ISocialEventArgs args)
	{
		if (!args.Error)
		{
			Debug.Log("Facebook :: Inviting Friends");
		}
	}

	public void RequestGameFriends(SocialFriendsRequestCallbackDelegate callback)
	{
		if (callback != null)
		{
			callback(new FBFriend[0]);
		}
	}
}
