using System;
using UnityEngine;

public class AndroidTwitterManager : Singleton<AndroidTwitterManager>, IDispatcher, TwitterManagerInterface
{
	private bool _IsAuthed;

	private bool _IsInited;

	private TwitterUserInfo _userInfo;

	public bool IsAuthed
	{
		get
		{
			return _IsAuthed;
		}
	}

	public bool IsInited
	{
		get
		{
			return _IsInited;
		}
	}

	public TwitterUserInfo userInfo
	{
		get
		{
			return _userInfo;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Init()
	{
		Init(SocialPlatfromSettings.Instance.TWITTER_CONSUMER_KEY, SocialPlatfromSettings.Instance.TWITTER_CONSUMER_SECRET);
	}

	public void Init(string consumer_key, string consumer_secret)
	{
		if (!_IsInited)
		{
			_IsInited = true;
			AndroidNative.TwitterInit(consumer_key, consumer_secret);
		}
	}

	public void AuthenticateUser()
	{
		if (_IsAuthed)
		{
			OnAuthSuccess();
		}
		else
		{
			AndroidNative.AuthificateUser();
		}
	}

	public void LoadUserData()
	{
		if (_IsAuthed)
		{
			AndroidNative.LoadUserData();
			return;
		}
		Debug.LogWarning("Auth user before loadin data, fail event generated");
		dispatch("user_data_failed_to_load", new TWResult(false, null));
	}

	public void Post(string status)
	{
		if (!_IsAuthed)
		{
			Debug.LogWarning("Auth user before posting data, fail event generated");
			dispatch("post_failed", new TWResult(false, null));
		}
		else
		{
			AndroidNative.TwitterPost(status);
		}
	}

	public void Post(string status, Texture2D texture)
	{
		if (!_IsAuthed)
		{
			Debug.LogWarning("Auth user before posting data, fail event generated");
			dispatch("post_failed", new TWResult(false, null));
		}
		else
		{
			byte[] inArray = texture.EncodeToPNG();
			string data = Convert.ToBase64String(inArray);
			AndroidNative.TwitterPostWithImage(status, data);
		}
	}

	public TwitterPostingTask PostWithAuthCheck(string status)
	{
		return PostWithAuthCheck(status, null);
	}

	public TwitterPostingTask PostWithAuthCheck(string status, Texture2D texture)
	{
		TwitterPostingTask twitterPostingTask = TwitterPostingTask.Cretae();
		twitterPostingTask.Post(status, texture, this);
		return twitterPostingTask;
	}

	public void LogOut()
	{
		_IsAuthed = false;
		AndroidNative.LogoutFromTwitter();
	}

	private void OnInited(string data)
	{
		if (data.Equals("1"))
		{
			_IsAuthed = true;
		}
		dispatch("twitter_inited", new TWResult(true, null));
	}

	private void OnAuthSuccess()
	{
		_IsAuthed = true;
		dispatch("authentication_succeeded", new TWResult(true, null));
	}

	private void OnAuthFailed()
	{
		dispatch("authentication_failed", new TWResult(false, null));
	}

	private void OnPostSuccess()
	{
		dispatch("post_succeeded", new TWResult(true, null));
	}

	private void OnPostFailed()
	{
		dispatch("post_failed", new TWResult(false, null));
	}

	private void OnUserDataLoaded(string data)
	{
		_userInfo = new TwitterUserInfo(data);
		dispatch("user_data_loaded", new TWResult(true, null));
	}

	private void OnUserDataLoadFailed()
	{
		dispatch("user_data_failed_to_load", new TWResult(false, null));
	}
}
