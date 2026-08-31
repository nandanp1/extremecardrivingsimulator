using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class LocalNotificationManager : MonoBehaviour
{
	public enum LocalNotificationType
	{
		None = 0,
		RewardReady = 1
	}

	private const string KEY_LOCAL_NOTIFICATION_FLAG = "local_notification_active";

	private const string KEY_LOCAL_NOTIFICATION_DAYS_FLAG = "local_notification_days_active";

	private int flag
	{
		get
		{
			return ObscuredPrefs.GetInt("local_notification_active", -1);
		}
		set
		{
			ObscuredPrefs.SetInt("local_notification_active", value);
		}
	}

	private int lastDay
	{
		get
		{
			return ObscuredPrefs.GetInt("local_notification_days_active", -1);
		}
		set
		{
			ObscuredPrefs.SetInt("local_notification_days_active", value);
		}
	}

	private void Awake()
	{
		flag = 0;
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void DecideIfNotificationsAreActive()
	{
		if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			flag = 1;
		}
		else
		{
			flag = 0;
		}
	}

	public void CreateLocalNotification(LocalNotificationType type)
	{
	}

	private void CreateRewardReadyNotification()
	{
	}

	private void ClearNotification(int id)
	{
	}

	private void ClearNotification(LocalNotificationType type)
	{
	}

	private void ClearAllNotifications()
	{
	}

	private int GetIDByEnum(LocalNotificationType type)
	{
		return (int)type;
	}

	private bool HasPassedAWeek()
	{
		bool result = false;
		if (lastDay != -1 && lastDay <= DateTime.Now.DayOfYear - 7)
		{
			result = true;
		}
		return result;
	}

	private void OnApplicationQuit()
	{
		lastDay = DateTime.Now.DayOfYear;
	}
}
