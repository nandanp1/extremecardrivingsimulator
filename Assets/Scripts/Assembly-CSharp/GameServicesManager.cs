using AxesInMotion.SocialPlugin;
using UnityEngine;

public class GameServicesManager : MonoBehaviour
{
	public bool tryLoginOnce = true;

	private ISocial _social;

	private string _leaderboardId;

	private bool _hasTryLogin;

	private bool _paused;

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

	[HideInInspector]
	public event GameServicesCallback loginSucceedBeforeShowLeaderboardEvent;

	[HideInInspector]
	public event GameServicesCallback loginSucceedBeforeShowAllLeaderboardEvent;

	private void Start()
	{
		MySingleton<Manager>.Instance.gameServicesManager = this;
		MySingleton<Manager>.Instance.isGooglePlayServicesAvailable = false;
		MySingleton<Manager>.Instance.isLeaderboardsAvailable = false;
	}

	private void InitializeCallback(ISocialEventArgs args)
	{
		if (args.Error)
		{
			Debug.LogError("InitializeCallback :: por alguna razón ocurrió un error al inicializarse");
		}
	}

	public void CheckAndLogin(SocialLoginCallbackDelegate callback)
	{
		_hasTryLogin = true;
		if (callback != null)
		{
			callback(CreateUnavailableArgs());
		}
	}

	public void Login(SocialLoginCallbackDelegate callback)
	{
		CheckAndLogin(callback);
	}

	public void ShowAllLeaderboardOrLogin()
	{
	}

	public void ShowLeaderboardOrLogin(string leaderboard)
	{
		_leaderboardId = leaderboard;
	}

	public void ShowAllLeaderboard()
	{
	}

	public void ShowLeaderboard(string leaderboard)
	{
	}

	public void ShareLeaderboardScore(string leaderboard, long score)
	{
	}

	private void LoginFromShowLeaderboardCallback(ISocialEventArgs args)
	{
	}

	private void LoginFromShowAllLeaderboardCallback(ISocialEventArgs args)
	{
	}

	public void SimpleTest()
	{
	}

	private static ISocialEventArgs CreateUnavailableArgs()
	{
		SocialEventArgs socialEventArgs = new SocialEventArgs(SocialEventType.LoginFailed);
		socialEventArgs.Error = true;
		socialEventArgs.Message = "Google Play Games is unavailable in the offline build.";
		return socialEventArgs;
	}
}
