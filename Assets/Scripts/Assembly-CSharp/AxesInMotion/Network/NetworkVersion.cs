using System;
using ExitGames.Client.Photon;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class NetworkVersion
	{
		private static TimeSpan ignoredTimePeriod = TimeSpan.FromDays(1.0);

		public static int currentVersion
		{
			get
			{
				return SettingsManager.GetMultiplayerVersion();
			}
		}

		public static int newestVersion
		{
			get
			{
				return SettingsManager.GetNewestVersion();
			}
		}

		public static bool isObsolete
		{
			get
			{
				return SettingsManager.GetIsObsolete();
			}
		}

		public static DateTime currentDateVersion
		{
			get
			{
				return DateTimeFromString(SettingsManager.GetMultiplayerDateVersion());
			}
		}

		public static DateTime newestDateVersion
		{
			get
			{
				return DateTimeFromString(SettingsManager.GetNewestDateVersion());
			}
		}

		public static void LoadVersion(string data)
		{
			string[] array = data.Split('_');
			if (array.Length != 3)
			{
				Debug.LogError("Formato incorrecto. " + data);
				return;
			}
			int multiplayerVersion = int.Parse(array[1]);
			string multiplayerDateVersion = array[2];
			int num = SettingsManager.GetNewestVersion();
			SettingsManager.SetMultiplayerVersion(multiplayerVersion);
			SettingsManager.SetMultiplayerDateVersion(multiplayerDateVersion);
		}

		public static void CheckPlayerVersionOf(NetworkPlayer player)
		{
			Hashtable properties = player.properties;
			Debug.LogWarning("Chequeando version del jugador " + player.name + " version " + player.GetVersion());
			int version = player.GetVersion();
			string dateVersion = player.GetDateVersion();
			if (SettingsManager.GetNewestVersion() < version)
			{
				SettingsManager.SetNewestVersion(version);
				SettingsManager.SetNewestDateVersion(dateVersion);
				if (!SettingsManager.GetIsObsolete())
				{
					CheckVersion();
				}
			}
		}

		public static void CheckVersion()
		{
			if (newestVersion > currentVersion)
			{
				if (DateTime.UtcNow.Subtract(newestDateVersion).TotalDays >= ignoredTimePeriod.TotalDays)
				{
					SettingsManager.SetIsObsolete(true);
				}
			}
			else
			{
				SettingsManager.SetIsObsolete(false);
			}
		}

		private static DateTime DateTimeFromString(string data)
		{
			string[] array = data.Split('-');
			if (array == null || array.Length != 5)
			{
				Debug.Log("Error de formato de la fecha. " + data);
				return DateTime.UtcNow;
			}
			DateTime value = new DateTime(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3]), int.Parse(array[4]), 0);
			return DateTime.SpecifyKind(value, DateTimeKind.Utc);
		}
	}
}
