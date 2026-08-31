using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class InternetTimeUtility : MonoBehaviour
{
	private const float TIME_FOR_TRY_AGAIN = 5f;

	private const float TIME_BETWEEN_SERVER = 0.5f;

	private const float TIME_OUT_IN_SECONDS = 4.5f;

	public string[] servers;

	private float timeWhenTimeUpdated = -1f;

	private DateTime? timeFromInternet;

	private bool receiving;

	private float currentTimeStartTimeOut;

	private DateTime? dateStatic;

	public static event InternetTimeIsReady OnInternetTimeIsReady;

	public void Init()
	{
		StopAllCoroutines();
		servers = new string[0];
		receiving = false;
		dateStatic = DateTime.UtcNow;
		timeFromInternet = dateStatic;
		timeWhenTimeUpdated = Time.realtimeSinceStartup;
		StartCoroutine(PublishLocalTime());
	}

	private IEnumerator PublishLocalTime()
	{
		// The original NTP implementation completed asynchronously. Keep that
		// lifecycle contract so every Awake/OnEnable can finish before consumers
		// enter Manager-owned startup state from the readiness callback.
		yield return null;
		if (OnInternetTimeIsReady != null)
		{
			OnInternetTimeIsReady(GetTimeFromInternet());
		}
	}

	private void OnEnable()
	{
		RemoteConfiguration.OnRemoteConfigurationLoad += Init;
	}

	private void OnDisable()
	{
		RemoteConfiguration.OnRemoteConfigurationLoad -= Init;
		timeFromInternet = null;
		timeWhenTimeUpdated = -1f;
		StopAllCoroutines();
	}

	public bool HasTimeFromInternet()
	{
		DateTime? dateTime = timeFromInternet;
		return dateTime.HasValue;
	}

	public DateTime GetTimeFromInternet()
	{
		DateTime result = DateTime.MinValue;
		DateTime? dateTime = timeFromInternet;
		if (dateTime.HasValue)
		{
			DateTime? dateTime2 = timeFromInternet;
			result = dateTime2.Value.AddSeconds(Time.realtimeSinceStartup - timeWhenTimeUpdated);
		}
		else
		{
			Debug.LogError("Se esta intentando usar la hora de internet cuando no se ha podido obtener. Usar HasTimeFromInternet antes de llamar a este metodo");
		}
		return result;
	}

	public DateTime GetTimeFromInternetPlusLocalTimenZone()
	{
		return GetTimeFromInternet().AddHours(DateTime.Now.Subtract(DateTime.UtcNow).Hours);
	}

	private IEnumerator UpdateTimeFromInternet()
	{
		while (true)
		{
			DateTime? dateTime = timeFromInternet;
			if (dateTime.HasValue)
			{
				break;
			}
			for (int i = 0; i != servers.Length; i++)
			{
				float startTime = Time.realtimeSinceStartup;
				yield return StartCoroutine(GetNetworkTime(servers[i]));
				yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.5f));
				DateTime? dateTime2 = dateStatic;
				if (dateTime2.HasValue)
				{
					timeFromInternet = dateStatic;
					StopCoroutine(UpdateTimeFromInternet());
					timeWhenTimeUpdated = Time.realtimeSinceStartup;
					MonoBehaviour.print(string.Concat("Son las ", GetTimeFromInternet(), " UTC desde ", servers[i], " En ", Time.realtimeSinceStartup - startTime));
					if (OnInternetTimeIsReady != null)
					{
						OnInternetTimeIsReady(GetTimeFromInternet());
					}
					StopCoroutine(UpdateTimeFromInternet());
					yield return null;
					break;
				}
			}
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(5f));
		}
	}

	private IEnumerator GetNetworkTime(string server)
	{
		bool error = false;
		bool timeOutExpired = false;
		Socket socket = null;
		byte[] ntpData = new byte[48];
		ntpData[0] = 27;
		try
		{
			IPAddress[] addresses = Dns.GetHostEntry(server).AddressList;
			IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Connect(ipEndPoint);
			socket.SendTimeout = 3500;
			socket.Send(ntpData);
			socket.ReceiveTimeout = 3500;
			receiving = true;
			timeOutExpired = false;
			currentTimeStartTimeOut = Time.realtimeSinceStartup;
			socket.BeginReceive(ntpData, 0, ntpData.Length, SocketFlags.None, ReceiveCallback, ntpData);
		}
		catch (Exception ex)
		{
			Exception e = ex;
			if (socket != null)
			{
				socket.Close();
			}
			MonoBehaviour.print(server + " ERROR En primer catch" + e.ToString());
			dateStatic = null;
			error = true;
		}
		if (error)
		{
			yield break;
		}
		while (receiving)
		{
			if (Time.realtimeSinceStartup - currentTimeStartTimeOut > 4.5f)
			{
				timeOutExpired = true;
				MonoBehaviour.print("ERROR time out expired");
				break;
			}
			yield return null;
		}
		receiving = false;
		if (!timeOutExpired)
		{
			try
			{
				socket.Close();
				ulong intPart = ((ulong)ntpData[40] << 24) | ((ulong)ntpData[41] << 16) | ((ulong)ntpData[42] << 8) | ntpData[43];
				ulong fractPart = ((ulong)ntpData[44] << 24) | ((ulong)ntpData[45] << 16) | ((ulong)ntpData[46] << 8) | ntpData[47];
				ulong milliseconds = intPart * 1000 + fractPart * 1000 / 4294967296L;
				DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);
				if (networkDateTime != new DateTime(1900, 1, 1))
				{
					dateStatic = networkDateTime;
				}
			}
			catch (Exception ex2)
			{
				Exception e2 = ex2;
				if (socket != null)
				{
					socket.Close();
				}
				MonoBehaviour.print(server + " ERROR en el segundo catch" + e2.ToString());
				dateStatic = null;
				error = true;
			}
		}
		yield return null;
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		receiving = false;
	}
}
