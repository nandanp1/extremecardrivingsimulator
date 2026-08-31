using System.Collections;
using AxesInMotion.Network;
using UnityEngine;

public class ConnectionTestPanel : MonoBehaviour
{
	public enum WarningReason
	{
		None = 0,
		ThreeG4G = 1,
		ThreeG4GAndHighLatency = 2,
		WIFIHighLatency = 3
	}

	public enum NetworkModeResult
	{
		None = 0,
		WIFI = 1,
		ThreeG4G = 2,
		Inadequate = 3
	}

	public enum DevicePerformanceResult
	{
		None = 0,
		GoodPerformance = 1,
		AcceptablePerformance = 2,
		PoorPerformance = 3
	}

	public enum QualityConnectionResult
	{
		None = 0,
		LowLatency = 1,
		HighLatency = 2,
		ToohighLatency = 3
	}

	public enum FinalResult
	{
		None = 0,
		Green = 1,
		Yellow = 2,
		Red = 3
	}

	public Color defaultColor;

	public Color badResultColor;

	public Color acceptableResultColor;

	public Color perfectResult;

	private int greenLatencyThreshold = 200;

	private int yellowLatencyThreshold = 900;

	public UILabel lbBack;

	public UILabel lbResume;

	public UILabel lbResult;

	public UILabel lbWarningText;

	public GameObject spTest;

	public GameObject spWarningPanel;

	public UISprite spNetworkModeTestPointer;

	public UISprite spDevicePerformanceTestPointer;

	public UISprite spQualityConnectionTestPointer;

	public UISprite spNetworkModeTestLight;

	public UISprite spDevicePerformanceTestLight;

	public UISprite spQualityConnectionTestLight;

	public Connection connection;

	public AutoQualityChooser autoQualityChooser;

	public NetworkModeResult networkModeResult;

	public DevicePerformanceResult devicePerformanceResult;

	public QualityConnectionResult qualityConnectiontResult;

	public FinalResult finalResult;

	public WarningReason warningReason;

	private void Start()
	{
		connection = MySingleton<Manager>.Instance.connection;
		autoQualityChooser = connection.gameObject.GetComponent<AutoQualityChooser>();
		MySingleton<Manager>.Instance.networkManager.OnAIMFailedToConnect += OnFailedToConnectCallback;
	}

	public void Show(bool startProccess = false)
	{
		ResetPanel();
		if (startProccess)
		{
			StartConnectingProcess();
		}
	}

	public void StartConnectingProcess()
	{
		StartCoroutine("StartTest");
	}

	private IEnumerator StartTest()
	{
		lbResume.text = Localization.Get("multiplayer_connectiontest_validating");
		yield return new WaitForSeconds(0.5f);
		NGUITools.SetActive(spNetworkModeTestPointer.gameObject, true);
		yield return new WaitForSeconds(0.5f);
		networkModeResult = DoNetworkModeTest();
		SetColorResult((byte)networkModeResult, spNetworkModeTestLight);
		NGUITools.SetActive(spNetworkModeTestPointer.gameObject, false);
		NGUITools.SetActive(spDevicePerformanceTestPointer.gameObject, true);
		yield return new WaitForSeconds(0.5f);
		devicePerformanceResult = DoDevicePerformanceTest();
		SetColorResult((byte)devicePerformanceResult, spDevicePerformanceTestLight);
		NGUITools.SetActive(spDevicePerformanceTestPointer.gameObject, false);
		NGUITools.SetActive(spQualityConnectionTestPointer.gameObject, true);
		yield return new WaitForSeconds(0.5f);
		qualityConnectiontResult = DoQualityConnectionTest();
		SetColorResult((byte)qualityConnectiontResult, spQualityConnectionTestLight);
		NGUITools.SetActive(spQualityConnectionTestPointer.gameObject, false);
		finalResult = CalculateFinalResult();
		NGUITools.SetActive(lbResult.gameObject, true);
		SetColorResult((byte)finalResult, lbResult);
		yield return new WaitForSeconds(1.5f);
		ExecuteDecisions();
	}

	private NetworkModeResult DoNetworkModeTest()
	{
		if (AndroidHelper.instance.IsWifiNetworkAvailable())
		{
			return NetworkModeResult.WIFI;
		}
		if (AndroidHelper.instance.IsMobileNetworkAvailable() && AndroidHelper.instance.IsFastConnection())
		{
			return NetworkModeResult.ThreeG4G;
		}
		return NetworkModeResult.Inadequate;
	}

	private DevicePerformanceResult DoDevicePerformanceTest()
	{
		return DevicePerformanceResult.GoodPerformance;
	}

	private QualityConnectionResult DoQualityConnectionTest()
	{
		int averagePing = MySingleton<Manager>.Instance.connection.averagePing;
		if (averagePing <= greenLatencyThreshold)
		{
			return QualityConnectionResult.LowLatency;
		}
		if (averagePing > greenLatencyThreshold && averagePing < yellowLatencyThreshold)
		{
			return QualityConnectionResult.HighLatency;
		}
		return QualityConnectionResult.ToohighLatency;
	}

	private FinalResult CalculateFinalResult()
	{
		FinalResult finalResult = FinalResult.None;
		finalResult = (FinalResult)networkModeResult;
		if ((int)finalResult < (int)devicePerformanceResult)
		{
			finalResult = (FinalResult)devicePerformanceResult;
		}
		if ((int)finalResult < (int)qualityConnectiontResult)
		{
			finalResult = (FinalResult)qualityConnectiontResult;
		}
		switch (finalResult)
		{
		case FinalResult.Green:
			lbResult.text = Localization.Get("multiplayer_connectiontest_resultmessage_perfect");
			if (networkModeResult == NetworkModeResult.ThreeG4G)
			{
				warningReason = WarningReason.ThreeG4G;
				lbWarningText.text = Localization.Get("multiplayer_connectiontest_warning_3G4G");
			}
			break;
		case FinalResult.Yellow:
			lbResult.text = Localization.Get("multiplayer_connectiontest_resultmessage_good");
			if (networkModeResult == NetworkModeResult.ThreeG4G)
			{
				warningReason = WarningReason.ThreeG4GAndHighLatency;
				lbWarningText.text = Localization.Get("multiplayer_connectiontest_warning_3g4g_highlatency");
			}
			else if (networkModeResult == NetworkModeResult.WIFI)
			{
				warningReason = WarningReason.WIFIHighLatency;
				lbWarningText.text = Localization.Get("multiplayer_connectiontest_warning_wifi_highlatency");
			}
			break;
		case FinalResult.Red:
			lbResult.text = Localization.Get("multiplayer_connectiontest_resultmessage_bad");
			if (networkModeResult == NetworkModeResult.ThreeG4G)
			{
				lbResume.text = Localization.Get("multiplayer_connectiontest_badresult_3g4g");
			}
			else if (networkModeResult == NetworkModeResult.WIFI)
			{
				lbResume.text = Localization.Get("multiplayer_connectiontest_badresult_wifi");
			}
			else
			{
				lbResume.text = Localization.Get("multiplayer_connectiontest_badresult_noconnected");
			}
			break;
		}
		return finalResult;
	}

	private void ExecuteDecisions()
	{
		if (warningReason != WarningReason.None)
		{
			NGUITools.SetActive(spTest, false);
			NGUITools.SetActive(spWarningPanel, true);
		}
		if (finalResult == FinalResult.Red)
		{
			NGUITools.SetActive(lbBack.gameObject, true);
		}
		if (finalResult == FinalResult.Green && warningReason == WarningReason.None)
		{
			MySingleton<Manager>.Instance.networkRooms.FindRoomByLatencyGroupAndPlay((byte)finalResult);
		}
	}

	public void PlayWithGoodResult()
	{
		lbResume.text = Localization.Get("multiplayer_connectiontest_joining");
		NGUITools.SetActive(spTest, true);
		NGUITools.SetActive(spWarningPanel, false);
		MySingleton<Manager>.Instance.networkRooms.FindRoomByLatencyGroupAndPlay((byte)finalResult);
	}

	public void PlayWithPerfectResult()
	{
		lbResume.text = Localization.Get("multiplayer_connectiontest_joining");
		MySingleton<Manager>.Instance.networkRooms.FindRoomByLatencyGroupAndPlay((byte)finalResult);
	}

	private void ResetPanel()
	{
		spNetworkModeTestLight.color = defaultColor;
		spDevicePerformanceTestLight.color = defaultColor;
		spQualityConnectionTestLight.color = defaultColor;
		networkModeResult = NetworkModeResult.None;
		devicePerformanceResult = DevicePerformanceResult.None;
		qualityConnectiontResult = QualityConnectionResult.None;
		finalResult = FinalResult.None;
		warningReason = WarningReason.None;
		NGUITools.SetActive(spTest, true);
		NGUITools.SetActive(spWarningPanel, false);
		NGUITools.SetActive(spNetworkModeTestPointer.gameObject, false);
		NGUITools.SetActive(spDevicePerformanceTestPointer.gameObject, false);
		NGUITools.SetActive(spQualityConnectionTestPointer.gameObject, false);
		NGUITools.SetActive(lbResult.gameObject, false);
		NGUITools.SetActive(lbBack.gameObject, false);
	}

	private void SetColorResult(byte result, UIWidget widget)
	{
		switch (result)
		{
		case 0:
			Debug.Log("Fallo al realizar el test");
			break;
		case 1:
			widget.color = perfectResult;
			break;
		case 2:
			widget.color = acceptableResultColor;
			break;
		case 3:
			widget.color = badResultColor;
			break;
		}
	}

	private void OnEnable()
	{
		ResetPanel();
	}

	private void OnFailedToConnectCallback(DisconnectionCause cause)
	{
		StopAllCoroutines();
		SetColorResult(3, spNetworkModeTestLight);
		NGUITools.SetActive(lbBack.gameObject, true);
		lbResume.text = Localization.Get("minimum_req_connection");
		Debug.Log("Fallo de conexion. Causa: " + cause);
	}

	private void OnDisable()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			MySingleton<Manager>.Instance.networkManager.OnAIMFailedToConnect -= OnFailedToConnectCallback;
			StopAllCoroutines();
		}
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			MySingleton<Manager>.Instance.networkManager.OnAIMFailedToConnect -= OnFailedToConnectCallback;
			StopAllCoroutines();
		}
	}
}
