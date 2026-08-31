using System;
using UnityEngine;

public class GooglePlayConnection : Singleton<GooglePlayConnection>
{
	public const string CONNECTION_STATE_CHANGED = "connection_state_changed";

	public const string CONNECTION_RESULT_RECEIVED = "connection_result_received";

	public const string PLAYER_CONNECTED = "player_connected";

	public const string PLAYER_DISCONNECTED = "player_disconnected";

	private bool _isInitialized;

	private static GPConnectionState _state;

	public static GPConnectionState state
	{
		get
		{
			return _state;
		}
	}

	public bool isInitialized
	{
		get
		{
			return _isInitialized;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void init()
	{
		string text = string.Empty;
		if (AndroidNativeSettings.Instance.EnableGamesAPI)
		{
			text += "GamesAPI";
		}
		if (AndroidNativeSettings.Instance.EnableAppStateAPI)
		{
			text += "AppStateAPI";
		}
		AndroidNative.playServiceInit(text);
		_isInitialized = true;
	}

	public void connect()
	{
		if (_state != GPConnectionState.STATE_CONNECTED && _state != GPConnectionState.STATE_CONNECTING)
		{
			OnStateChange(GPConnectionState.STATE_CONNECTING);
			if (!_isInitialized)
			{
				Singleton<GooglePlayManager>.instance.Create();
				init();
			}
			AndroidNative.playServiceConnect();
		}
	}

	public void disconnect()
	{
		if (_state != GPConnectionState.STATE_DISCONNECTED && _state != GPConnectionState.STATE_CONNECTING)
		{
			OnStateChange(GPConnectionState.STATE_DISCONNECTED);
			AndroidNative.playServiceDisconnect();
		}
	}

	public static bool CheckState()
	{
		switch (_state)
		{
		case GPConnectionState.STATE_CONNECTED:
			return true;
		case GPConnectionState.STATE_DISCONNECTED:
			Singleton<GooglePlayConnection>.instance.connect();
			return false;
		default:
			return false;
		}
	}

	private void OnConnectionResult(string data)
	{
		string[] array = data.Split("|"[0]);
		GooglePlayConnectionResult googlePlayConnectionResult = new GooglePlayConnectionResult();
		googlePlayConnectionResult.code = (GooglePlayConnectionResultCode)Convert.ToInt32(array[0]);
		if (Convert.ToInt32(array[1]) == 1)
		{
			googlePlayConnectionResult.HasResolution = true;
		}
		else
		{
			googlePlayConnectionResult.HasResolution = false;
		}
		if (googlePlayConnectionResult.IsSuccess)
		{
			OnStateChange(GPConnectionState.STATE_CONNECTED);
		}
		else if (!googlePlayConnectionResult.HasResolution)
		{
			OnStateChange(GPConnectionState.STATE_DISCONNECTED);
		}
		dispatch("connection_result_received", googlePlayConnectionResult);
	}

	private void OnStateChange(GPConnectionState connectionState)
	{
		_state = connectionState;
		switch (_state)
		{
		case GPConnectionState.STATE_CONNECTED:
			dispatch("player_connected");
			break;
		case GPConnectionState.STATE_DISCONNECTED:
			dispatch("player_disconnected");
			break;
		}
		dispatch("connection_state_changed", _state);
		Debug.Log("Play Serice Connection State -> " + _state);
	}
}
