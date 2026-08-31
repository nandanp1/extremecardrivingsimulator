using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Uniject;
using UnityEngine;

public class UnibillUnityUtil : MonoBehaviour, IUtil
{
	private static List<Action> callbacks = new List<Action>();

	private static volatile bool callbacksPending;

	private static List<RuntimePlatform> PCControlledPlatforms = new List<RuntimePlatform>
	{
		RuntimePlatform.LinuxPlayer,
		RuntimePlatform.OSXEditor,
		RuntimePlatform.OSXPlayer,
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.WindowsPlayer
	};

	public DateTime currentTime
	{
		get
		{
			return DateTime.Now;
		}
	}

	public string persistentDataPath
	{
		get
		{
			return Application.persistentDataPath;
		}
	}

	public RuntimePlatform Platform
	{
		get
		{
			return Application.platform;
		}
	}

	public bool IsEditor
	{
		get
		{
			return Application.isEditor;
		}
	}

	public string DeviceModel
	{
		get
		{
			return SystemInfo.deviceModel;
		}
	}

	public string DeviceName
	{
		get
		{
			return SystemInfo.deviceName;
		}
	}

	public DeviceType DeviceType
	{
		get
		{
			return SystemInfo.deviceType;
		}
	}

	public string OperatingSystem
	{
		get
		{
			return SystemInfo.operatingSystem;
		}
	}

	object IUtil.InitiateCoroutine(IEnumerator start)
	{
		return StartCoroutine(start);
	}

	void IUtil.InitiateCoroutine(IEnumerator start, int delay)
	{
		delayedCoroutine(start, delay);
	}

	public T[] getAnyComponentsOfType<T>() where T : class
	{
		GameObject[] array = (GameObject[])UnityEngine.Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.InstanceID);
		List<T> list = new List<T>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components)
			{
				if (monoBehaviour is T)
				{
					list.Add(monoBehaviour as T);
				}
			}
		}
		return list.ToArray();
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public string loadedLevelName()
	{
		return LegacySceneManager.LoadedLevelName;
	}

	public static T findInstanceOfType<T>() where T : MonoBehaviour
	{
		return (T)UnityEngine.Object.FindFirstObjectByType(typeof(T));
	}

	public static T loadResourceInstanceOfType<T>() where T : MonoBehaviour
	{
		return ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(typeof(T).ToString()))).GetComponent<T>();
	}

	public static bool pcPlatform()
	{
		return PCControlledPlatforms.Contains(Application.platform);
	}

	public static void DebugLog(string message, params object[] args)
	{
		try
		{
			Debug.Log(string.Format("com.ballatergames.debug - {0}", string.Format(message, args)));
		}
		catch (ArgumentNullException message2)
		{
			Debug.Log(message2);
		}
		catch (FormatException message3)
		{
			Debug.Log(message3);
		}
	}

	private IEnumerator delayedCoroutine(IEnumerator coroutine, int delay)
	{
		yield return new WaitForSeconds(delay);
		StartCoroutine(coroutine);
	}

	public void RunOnThreadPool(Action runnable)
	{
		ThreadPool.QueueUserWorkItem((object x) =>
		{
			runnable();
		});
	}

	private void Update()
	{
		if (!callbacksPending)
		{
			return;
		}
		Action[] array;
		lock (callbacks)
		{
			if (callbacks.Count == 0)
			{
				return;
			}
			array = new Action[callbacks.Count];
			callbacks.CopyTo(array);
			callbacks.Clear();
			callbacksPending = false;
		}
		Action[] array2 = array;
		foreach (Action action in array2)
		{
			action();
		}
	}

	public void RunOnMainThread(Action runnable)
	{
		lock (callbacks)
		{
			callbacks.Add(runnable);
			callbacksPending = true;
		}
	}

	public object getWaitForSeconds(int seconds)
	{
		return new WaitForSeconds(seconds);
	}
}
