using UnityEngine;

public class InstanceTraffic : MonoBehaviour
{
	public GameObject trafficPrefab;

	public LayerMask layerMask = -1;

	private AIContoller ACScript;

	[HideInInspector]
	public GameObject carsParent;

	private void Awake()
	{
		MySingleton<Manager>.Instance.gameLogic.instanceTraffic = this;
	}

	private void Start()
	{
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic)
		{
			trafficPrefab.SetActive(true);
			if ((bool)MySingleton<Manager>.Instance.gameLogic.trafficModeInstantiater)
			{
				MySingleton<Manager>.Instance.gameLogic.trafficModeInstantiater.SetActive(true);
			}
			Debug.Log(base.gameObject.name);
			trafficPrefab.GetComponent<AIContoller>().InstantiateCars();
			carsParent = MySingleton<Manager>.Instance.gameLogic.carsParent;
			carsParent.SetActive(true);
			if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
			{
				trafficPrefab.SetActive(false);
				carsParent.SetActive(false);
			}
		}
	}

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.isChallangeAvailable)
		{
			MySingleton<Manager>.Instance.challangeModeManager._aiContollerCity = Object.FindFirstObjectByType<AIContoller>();
		}
	}

	private void OnDisable()
	{
		if (MySingleton<Manager>.Instance == null || MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Traffic)
		{
			return;
		}
		if ((bool)trafficPrefab)
		{
			trafficPrefab.SetActive(false);
		}
		if (!carsParent)
		{
			return;
		}
		carsParent.SetActive(false);
		foreach (Transform item in carsParent.transform)
		{
			item.gameObject.SetActive(false);
			Object.Destroy(item.gameObject);
		}
	}
}
