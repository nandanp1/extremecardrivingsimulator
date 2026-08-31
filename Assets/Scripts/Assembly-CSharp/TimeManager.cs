using System;
using System.Collections;
using System.Collections.Generic;
using ANMiniJSON;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

[RequireComponent(typeof(InternetTimeUtility))]
public class TimeManager : MonoBehaviour
{
	public delegate void NewDay();

	public delegate void InternetAvaliableCallBack();

	private string TAG = "ManagerTime";

	public bool debugOnNewDayLaunched;

	public bool debug;

	public InternetTimeUtility internetTimeUtiliy;

	public DateTime currentDateTime;

	public DateTime lastDateTime;

	private DateTime dateTimeAux;

	private float timeToIncrease = 1f;

	[SerializeField]
	private long tickLastDateTime;

	[SerializeField]
	private double elapsedTime;

	public bool isInternetAvailable;

	public float TEMP_timeInPause;

	public double TEMP_timeInLastTime;

	public DateTime TEMP_timeBeforeAddSeconds;

	public DateTime TEMPlastDateTime;

	private Coroutine timeCoroutine;

	private float lastTime = -1f;

	private string PLAYER_PREFS_KEY
	{
		get
		{
			return "com.aim.racing_ManagerTime";
		}
	}

	public int today { get; set; }

	public static event NewDay OnNewDay;

	public static event InternetAvaliableCallBack OnInternetAvaliableCallBack;

	public void LauncNewDay()
	{
		if (OnNewDay != null)
		{
			OnNewDay();
		}
	}

	private void Awake()
	{
		internetTimeUtiliy = GetComponent<InternetTimeUtility>();
	}

	private void OnEnable()
	{
		InternetTimeUtility.OnInternetTimeIsReady += OnInternetUtilityReady;
		isInternetAvailable = false;
		timeCoroutine = null;
		Load();
		today = -1;
		internetTimeUtiliy.Init();
	}

	private void OnDisable()
	{
		InternetTimeUtility.OnInternetTimeIsReady -= OnInternetUtilityReady;
		if (timeCoroutine != null)
		{
			StopCoroutine(timeCoroutine);
			timeCoroutine = null;
		}
	}

	private void Save()
	{
		ObscuredPrefs.SetString(PLAYER_PREFS_KEY, lastDateTime.ToString());
	}

	private void Load()
	{
		string s = ObscuredPrefs.GetString(PLAYER_PREFS_KEY, DateTime.MinValue.ToString());
		TEMPlastDateTime = (lastDateTime = DateTime.Parse(s));
		Debug.Log(TAG + "Hora recuperada de player prefs: " + lastDateTime);
	}

	public string[] GetTimeServers()
	{
		string[] array = null;
		string timeServers = MySingleton<Manager>.Instance.remoteConfiguration.timeServers;
		Dictionary<string, object> dictionary = Json.Deserialize(timeServers) as Dictionary<string, object>;
		List<object> list = dictionary["servers"] as List<object>;
		List<string> list2 = list.ConvertAll((object x) => x.ToString());
		return list2.ToArray();
	}

	public bool HasTimeFromInternet()
	{
		return internetTimeUtiliy.HasTimeFromInternet();
	}

	public DateTime GetTimeFromInternet()
	{
		DateTime minValue = DateTime.MinValue;
		return lastDateTime;
	}

	public DateTime GetTimeFromInternetPlusLocalTimenZone()
	{
		return GetTimeFromInternet().AddHours(DateTime.Now.Subtract(DateTime.UtcNow).Hours);
	}

	private IEnumerator UpdateTime()
	{
		while (isInternetAvailable)
		{
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(timeToIncrease));
			IncreaseTimes(timeToIncrease);
		}
	}

	private void IncreaseTimes(float timeToAdd)
	{
		dateTimeAux = lastDateTime.AddSeconds(timeToAdd);
		dateTimeAux.AddSeconds(timeToAdd);
		CheckDateToday(dateTimeAux);
		elapsedTime += timeToAdd;
		lastDateTime = dateTimeAux;
		tickLastDateTime = lastDateTime.Ticks;
		Save();
	}

	private void OnInternetUtilityReady(DateTime timeFromInternet)
	{
		if (timeCoroutine != null)
		{
			return;
		}
		if (debug)
		{
			if (!debugOnNewDayLaunched)
			{
				DateTime utcNow = DateTime.UtcNow;
				currentDateTime = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0);
				currentDateTime = currentDateTime.AddSeconds(-120.0);
				Debug.Log(TAG + "Fecha de debug: " + currentDateTime);
				debugOnNewDayLaunched = true;
			}
		}
		else
		{
			currentDateTime = timeFromInternet;
			Debug.Log(TAG + "Fecha de currentDateTime: " + currentDateTime);
		}
		if (today < 0)
		{
			today = currentDateTime.Day;
		}
		Debug.Log(string.Concat(TAG, "CurrentDateTime: ", currentDateTime, " lastDateTime: ", lastDateTime, ", today: ", today));
		MySingleton<Manager>.Instance.stats.ChekPlayedDays(currentDateTime, lastDateTime);
		if (lastDateTime != DateTime.MinValue)
		{
			elapsedTime = currentDateTime.Subtract(lastDateTime).TotalSeconds;
		}
		TEMP_timeInLastTime = elapsedTime;
		Debug.Log(TAG + "Han pasado " + elapsedTime + " segundos desde la ultima vez");
		CheckDateToday(lastDateTime);
		elapsedTime = 0.0;
		lastDateTime = currentDateTime;
		Debug.Log(TAG + "lastDateTime " + lastDateTime.ToString() + string.Empty);
		TEMP_timeBeforeAddSeconds = lastDateTime;
		isInternetAvailable = true;
		timeCoroutine = StartCoroutine(UpdateTime());
		if (OnInternetAvaliableCallBack != null)
		{
			OnInternetAvaliableCallBack();
		}
	}

	private void CheckDateToday(DateTime dateTime)
	{
		if (dateTime.Day != today && dateTime.Day > today)
		{
			Debug.Log(TAG + "**************** Nuevo dia ****************");
			today = dateTime.Day;
			if (OnNewDay != null)
			{
				OnNewDay();
			}
		}
	}
}
