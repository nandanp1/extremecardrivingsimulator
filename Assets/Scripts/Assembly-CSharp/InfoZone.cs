using UnityEngine;

public class InfoZone : MonoBehaviour
{
	public InfoWorkshop[] infoWorkshopArray;

	public GameObject collectablesZoneParent;

	public GameObject signs;

	public void DisableWorkshops()
	{
		InfoWorkshop[] array = infoWorkshopArray;
		foreach (InfoWorkshop infoWorkshop in array)
		{
			infoWorkshop.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		MySingleton<Manager>.Instance.gameLogic.actualZoneCollectablesParent = collectablesZoneParent;
		if (MySingleton<Manager>.Instance.isChallangeAvailable)
		{
			if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress && (bool)collectablesZoneParent)
			{
				collectablesZoneParent.SetActive(false);
			}
			if (MySingleton<Manager>.Instance.trafficSystem == Manager.TrafficSystem.Old)
			{
				if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic && (bool)MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab)
				{
					MySingleton<Manager>.Instance.challangeModeManager._aiContollerCity = MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
				}
			}
			else if (MySingleton<Manager>.Instance.trafficSystem != Manager.TrafficSystem.ITStraffic)
			{
			}
			if ((bool)MySingleton<Manager>.Instance.guiGameLogic && (bool)MySingleton<Manager>.Instance.guiGameLogic.pnLevelLoading)
			{
				NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnLevelLoading.gameObject, false);
			}
			if ((bool)MySingleton<Manager>.Instance.guiChallangeModeLogic)
			{
				MySingleton<Manager>.Instance.guiChallangeModeLogic.signs = signs;
			}
		}
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic && MySingleton<Manager>.Instance.trafficSystem == Manager.TrafficSystem.ITStraffic && !MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			GameObject gameObject = GameObject.Find("TrafficITSZoneActivation");
			if (gameObject != null)
			{
				gameObject.GetComponent<TrafficITSZoneActivation>().DeleteTraffic();
				gameObject.GetComponent<TrafficITSZoneActivation>().CreateTraffic();
			}
		}
	}
}
