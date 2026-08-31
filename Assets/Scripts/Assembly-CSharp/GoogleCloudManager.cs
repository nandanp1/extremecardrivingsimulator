using System;
using System.Collections.Generic;
using UnityEngine;

public class GoogleCloudManager : Singleton<GoogleCloudManager>
{
	public const string STATE_DELETED = "key_deleted";

	public const string STATE_UPDATED = "state_updated";

	public const string STATE_LOADED = "state_loaded";

	public const string STATE_RESOLVED = "state_resolved";

	public const string STATE_CONFLICT = "state_conflict";

	public const string ALL_STATES_LOADED = "all_states_loaded";

	private int _maxStateSize = -1;

	private int _maxNumKeys = -1;

	private Dictionary<int, byte[]> _states = new Dictionary<int, byte[]>();

	public int maxStateSize
	{
		get
		{
			return _maxStateSize;
		}
	}

	public int maxNumKeys
	{
		get
		{
			return _maxNumKeys;
		}
	}

	public Dictionary<int, byte[]> states
	{
		get
		{
			return _states;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Create()
	{
		Debug.Log("GoogleCloudManager was created");
	}

	public void loadAllStates()
	{
		AndroidNative.listStates();
	}

	public void updateState(int stateKey, byte[] val)
	{
		string text = string.Empty;
		int num = val.Length;
		for (int i = 0; i < num; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += val[i];
		}
		AndroidNative.updateState(stateKey, text);
	}

	public void resolveState(int stateKey, byte[] resolvedData, string resolvedVersion)
	{
		string text = string.Empty;
		int num = resolvedData.Length;
		for (int i = 0; i < num; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += resolvedData[i];
		}
		AndroidNative.resolveState(stateKey, text, resolvedVersion);
	}

	public void deleteState(int stateKey)
	{
		AndroidNative.deleteState(stateKey);
	}

	public void loadState(int stateKey)
	{
		AndroidNative.loadState(stateKey);
	}

	public byte[] GetStateData(int stateKey)
	{
		if (_states.ContainsKey(stateKey))
		{
			return _states[stateKey];
		}
		return null;
	}

	private void OnAllStatesLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		Debug.Log("OnAllStatesLoaded");
		Debug.Log(data);
		GoogleCloudResult data2 = new GoogleCloudResult(array[0]);
		if (array.Length > 1)
		{
			_states.Clear();
			for (int i = 1; i < array.Length && !(array[i] == "endofline"); i += 2)
			{
				PushStateData(array[i], ConvertStringToCloudData(array[i + 1]));
			}
			Debug.Log("Loaded: " + _states.Count + " States");
		}
		Debug.Log("disp");
		dispatch("all_states_loaded", data2);
	}

	private void OnStateConflict(string data)
	{
		string[] array = data.Split("|"[0]);
		GoogleCloudResult googleCloudResult = new GoogleCloudResult("0", array[0]);
		if (googleCloudResult.isSuccess)
		{
			googleCloudResult.stateData = ConvertStringToCloudData(array[1]);
			googleCloudResult.serverConflictData = ConvertStringToCloudData(array[2]);
			googleCloudResult.resolvedVersion = array[3];
			PushStateData(array[0], googleCloudResult.stateData);
		}
		dispatch("state_conflict", googleCloudResult);
	}

	private void OnStateLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		GoogleCloudResult googleCloudResult = new GoogleCloudResult(array[0], array[1]);
		googleCloudResult.stateData = ConvertStringToCloudData(array[2]);
		PushStateData(array[1], googleCloudResult.stateData);
		dispatch("state_loaded", googleCloudResult);
	}

	private void OnStateResolved(string data)
	{
		string[] array = data.Split("|"[0]);
		GoogleCloudResult googleCloudResult = new GoogleCloudResult(array[0], array[1]);
		googleCloudResult.stateData = ConvertStringToCloudData(array[2]);
		PushStateData(array[1], googleCloudResult.stateData);
		dispatch("state_resolved", googleCloudResult);
	}

	private void OnStateUpdated(string data)
	{
		string[] array = data.Split("|"[0]);
		GoogleCloudResult googleCloudResult = new GoogleCloudResult(array[0], array[1]);
		googleCloudResult.stateData = ConvertStringToCloudData(array[2]);
		PushStateData(array[1], googleCloudResult.stateData);
		dispatch("state_updated", googleCloudResult);
	}

	private void OnKeyDeleted(string data)
	{
		string[] array = data.Split("|"[0]);
		GoogleCloudResult data2 = new GoogleCloudResult(array[0], array[1]);
		dispatch("key_deleted", data2);
	}

	private void OnCloudConnected(string data)
	{
		string[] array = data.Split("|"[0]);
		Debug.Log("Google Cloud is connected max state size: " + array[0] + " max state num " + array[1]);
		_maxNumKeys = Convert.ToInt32(array[1]);
		_maxStateSize = Convert.ToInt32(array[0]);
	}

	private void PushStateData(string stateKey, byte[] data)
	{
		PushStateData(Convert.ToInt32(stateKey), data);
	}

	private void PushStateData(int stateKey, byte[] data)
	{
		if (_states.ContainsKey(stateKey))
		{
			_states[stateKey] = data;
		}
		else
		{
			_states.Add(stateKey, data);
		}
	}

	private static byte[] ConvertStringToCloudData(string data)
	{
		if (data == null)
		{
			return null;
		}
		data = data.Replace("endofline", string.Empty);
		if (data.Equals(string.Empty))
		{
			return null;
		}
		string[] array = data.Split(","[0]);
		List<byte> list = new List<byte>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			Debug.Log(text);
			list.Add(Convert.ToByte(text));
		}
		return list.ToArray();
	}
}
