using System.Collections;
using UnityEngine;

public class LevelZone : MonoBehaviour
{
	public enum StatusLevel
	{
		Unloaded = 1,
		Loaded = 2,
		Loading = 3,
		DontTouch = -1
	}

	public GameObject lod;

	public bool isBigZone;

	public string nameSceneZone;

	public string[] childsZones;

	public Transform spawnTransformZone;

	public StatusLevel statusLevel;

	public bool needInstall;

	public bool needMiniLoading;

	private bool neededByCar;

	private static int basicColliderLayer = -1;

	private static int BasicColliderLayer
	{
		get
		{
			if (basicColliderLayer < 0)
			{
				// Resolve lazily: Unity 6 rejects engine API calls from MonoBehaviour field initializers.
				basicColliderLayer = LayerMask.NameToLayer("BasicCollider");
			}
			return basicColliderLayer;
		}
	}

	private bool alreadyInstalled
	{
		get
		{
			return SettingsManager.GetBool("key" + nameSceneZone + "installed", false);
		}
		set
		{
			SettingsManager.SetBool("key" + nameSceneZone + "installed", value);
		}
	}

	public bool isLoaded
	{
		get
		{
			return statusLevel == StatusLevel.Loaded;
		}
	}

	public bool isLoading
	{
		get
		{
			return statusLevel == StatusLevel.Loading;
		}
	}

	public bool isUnloaded
	{
		get
		{
			return statusLevel == StatusLevel.Unloaded;
		}
	}

	public void ForceLoadLevel()
	{
		statusLevel = StatusLevel.Loaded;
		if ((bool)lod)
		{
			lod.gameObject.SetActive(false);
		}
		if (isBigZone)
		{
			MySingleton<Manager>.Instance.levelZone = this;
		}
		LegacySceneManager.LoadLevelAdditive(nameSceneZone);
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer)
		{
			MySingleton<Manager>.Instance.gameLogic.SendMessage("DisableOfflineElements", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ForceUnloadLevel()
	{
		UnloadZone();
	}

	public static void UnloadAllZones(LevelZone[] allZones)
	{
		foreach (LevelZone levelZone in allZones)
		{
			levelZone.UnloadZone();
		}
	}

	public void ProcessTriggerEvent(LevelZoneTrigger zone, Collider other, bool isTriggerEnter)
	{
		if (other.CompareTag("TrialCamera"))
		{
			if (isLoading && zone.isTriggerForLimitLoadingZone && isTriggerEnter)
			{
				StartCoroutine(WaitingLoadingZone());
			}
			else if (isUnloaded && !isLoading && zone.isTriggerForLoad && isTriggerEnter)
			{
				StartCoroutine(LoadZone());
			}
			else if (isLoaded && zone.isTriggerForUnload && !isTriggerEnter && !neededByCar)
			{
				UnloadZone();
			}
			return;
		}
		Manager manager = MySingleton<Manager>.Instance;
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (other.gameObject.layer != BasicColliderLayer || attachedRigidbody == null || manager == null || manager.gameLogic == null || manager.gameLogic.instantiatedCar == null || attachedRigidbody.gameObject != manager.gameLogic.instantiatedCar)
		{
			// Unity 6 can report unrelated rigidbody contacts here; only the local car owns zone state.
			return;
		}
		if (isLoading && zone.isTriggerForLimitLoadingZone && isTriggerEnter)
		{
			StartCoroutine(WaitingLoadingZone());
		}
		else if (isUnloaded && !isLoading && zone.isTriggerForLoad && isTriggerEnter)
		{
			StartCoroutine(LoadZone());
		}
		else if (isLoaded && zone.isTriggerForUnload && !isTriggerEnter)
		{
			UnloadZone();
			neededByCar = false;
		}
		if (isLoading || isLoaded)
		{
			neededByCar = true;
		}
		else
		{
			neededByCar = false;
		}
	}

	private IEnumerator WaitingLoadingZone()
	{
		MySingleton<Manager>.Instance.gameLogic.guiLogic.HideHUD();
		Time.timeScale = 0f;
		while (isLoading)
		{
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.15f));
		}
		Time.timeScale = 1f;
		MySingleton<Manager>.Instance.guiGameLogic.ShowHUD();
	}

	private void UnloadZone()
	{
		if ((bool)lod)
		{
			lod.gameObject.SetActive(true);
		}
		GameObject gameObject = GameObject.Find(nameSceneZone);
		if ((bool)gameObject)
		{
			if (childsZones != null && childsZones.Length > 0)
			{
				string[] array = childsZones;
				foreach (string text in array)
				{
					GameObject gameObject2 = GameObject.Find(text);
					if ((bool)gameObject2)
					{
						Object.Destroy(gameObject2);
					}
				}
			}
			Object.Destroy(gameObject);
			statusLevel = StatusLevel.Unloaded;
			Resources.UnloadUnusedAssets();
		}
		else if (Debug.isDebugBuild)
		{
			Debug.LogError("Se ha intentado descargar la zona " + nameSceneZone + " que no se ha encontrado en la escena");
		}
		if (isBigZone && MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic && MySingleton<Manager>.Instance.trafficSystem == Manager.TrafficSystem.ITStraffic && MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			Transform transform = MySingleton<Manager>.Instance.gameLogic.instantiatedCar.transform.Find("TrafficITSZoneActivation");
			if (transform != null)
			{
				transform.GetComponent<TrafficITSZoneActivation>().DeleteTraffic();
			}
		}
	}

	private IEnumerator LoadZone()
	{
		if ((bool)lod)
		{
			lod.SetActive(false);
		}
		statusLevel = StatusLevel.Loading;
		if (Debug.isDebugBuild)
		{
			Debug.Log(nameSceneZone + "-Starting Load");
		}
		if (isBigZone && MySingleton<Manager>.Instance != null && MySingleton<Manager>.Instance.levelZone != null)
		{
			string lastZone = MySingleton<Manager>.Instance.levelZone.gameObject.name;
			MySingleton<Manager>.Instance.levelZone = this;
			ManagerAnalytics.instance.FireZoneChanged(MySingleton<Manager>.Instance.levelZone.gameObject.name, lastZone);
		}
		if (needInstall && !alreadyInstalled)
		{
			if (MySingleton<Manager>.Instance.gameLogic != null && MySingleton<Manager>.Instance.gameLogic.guiLogic != null)
			{
				MySingleton<Manager>.Instance.gameLogic.guiLogic.ShowChangeZonePanel();
				yield return null;
				MySingleton<Manager>.Instance.gameLogic.ResetCarInput();
				yield return null;
				LegacySceneManager.LoadLevelAdditive(nameSceneZone);
				statusLevel = StatusLevel.Loaded;
				yield return null;
				alreadyInstalled = true;
				MySingleton<Manager>.Instance.gameLogic.ResetCarInput();
				yield return null;
				MySingleton<Manager>.Instance.gameLogic.guiLogic.ShowHUD();
			}
		}
		else
		{
			AsyncOperation lAsync = LegacySceneManager.LoadLevelAdditiveAsync(nameSceneZone);
			if (needMiniLoading)
			{
				MySingleton<Manager>.Instance.gameLogic.guiLogic.ShowMiniLoading();
			}
			yield return null;
			while (!lAsync.isDone)
			{
				yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.15f));
			}
			if (needMiniLoading)
			{
				MySingleton<Manager>.Instance.gameLogic.guiLogic.HideMiniLoading();
			}
			yield return null;
			statusLevel = StatusLevel.Loaded;
			yield return lAsync;
		}
		if (isBigZone && MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic && MySingleton<Manager>.Instance.trafficSystem == Manager.TrafficSystem.ITStraffic && MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			Transform trafficZoneActivation = MySingleton<Manager>.Instance.gameLogic.instantiatedCar.transform.Find("TrafficITSZoneActivation");
			if (trafficZoneActivation != null)
			{
				trafficZoneActivation.GetComponent<TrafficITSZoneActivation>().DeleteTraffic();
			}
		}
	}

	public void ReloadChallengeScene()
	{
		StartCoroutine(LoadZone());
	}
}
