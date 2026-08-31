using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Collectables : MonoBehaviour
{
	[Serializable]
	public enum CollectableType
	{
		Raptor = -2,
		None = 0,
		Baylon = 1,
		Hommer = 2,
		Buhaira = 3,
		Limpeza = 4,
		Rubito = 5,
		Traptor = 6,
		Maletillas = 7,
		Ramon = 8,
		Grumete = 9,
		GLA = 10,
		SLS = 11
	}

	public const string ICON_NAME_BAYLON = "Icon_Baylon";

	public const string ICON_NAME_HOMMER = "Icon_Hommer";

	public const string ICON_NAME_BUHAIRA = "Icon_Buhaira";

	public const string ICON_NAME_LIMPEZA = "Icon_Limpeza";

	public const string ICON_NAME_RUBITO = "Icon_Rubito";

	public const string ICON_NAME_TRAPTOR = "Icon_Traptor";

	public const string ICON_NAME_MALETILLAS = "Icon_Maletillas";

	public const string ICON_NAME_RAMON = "Icon_Ramon";

	public const string ICON_NAME_GRUMETE = "Icon_Grumete";

	public const string ICON_NAME_GLAMOUR = "Icon_Glamour";

	public const string ICON_NAME_SLS = "Icon_SLS";

	private List<InfoCar> infoCarList = new List<InfoCar>();

	public int collectableId;

	public CollectableType collectableType;

	public GUIGameLogic guiGameLogic;

	private bool flagHited;

	public string key
	{
		get
		{
			return (int)collectableType + ":" + collectableId;
		}
	}

	private void Start()
	{
		Dictionary<CollectableType, List<int>> maxCollectables = MySingleton<Manager>.Instance.stats.maxCollectables;
		if (!maxCollectables.ContainsKey(collectableType))
		{
			List<int> value = new List<int>();
			maxCollectables.Add(collectableType, value);
		}
		if (!maxCollectables[collectableType].Contains(collectableId))
		{
			maxCollectables[collectableType].Add(collectableId);
		}
		Dictionary<CollectableType, List<int>> collectablesCounter = MySingleton<Manager>.Instance.stats.collectablesCounter;
		if (!collectablesCounter.ContainsKey(collectableType))
		{
			return;
		}
		List<int> list = collectablesCounter[collectableType];
		foreach (int item in list)
		{
			if (item == collectableId)
			{
				UnityEngine.Object.Destroy(base.transform.gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Car") && !flagHited)
		{
			ManagerAnalytics.instance.FireBlueprintFound(string.Concat(collectableType, "_", collectableId), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name, MySingleton<Manager>.Instance.gameMode.ToString(), MySingleton<Manager>.Instance.gameLogic.jump.ToString());
			flagHited = true;
			base.transform.gameObject.SetActive(false);
			UpdateCollectablesNUEVO();
			MySingleton<Manager>.Instance.guiGameLogic.UnlockTheseCars(GetInfoCarFor());
		}
	}

	public void UpdateCollectables()
	{
	}

	public void UpdateCollectablesNUEVO()
	{
		Dictionary<CollectableType, List<int>> collectablesCounter = MySingleton<Manager>.Instance.stats.collectablesCounter;
		if (collectablesCounter.ContainsKey(collectableType))
		{
			collectablesCounter[collectableType].Add(collectableId);
		}
		else
		{
			List<int> list = new List<int>();
			list.Add(collectableId);
			collectablesCounter.Add(collectableType, list);
		}
		SettingsManager.SetCollectablesStatus(MySingleton<Manager>.Instance.stats.collectablesCounter);
		MySingleton<Manager>.Instance.stats.collectablesCounter = collectablesCounter;
		if (!guiGameLogic)
		{
			guiGameLogic = MySingleton<Manager>.Instance.gameLogic.guiLogic;
		}
		guiGameLogic.ShowCollectablePanel(collectableType);
		MySingleton<Manager>.Instance.gameLogic.PlayAudioCollectableCollected();
	}

	public static string GetIconNameFor(CollectableType collecType)
	{
		string text = string.Empty;
		switch (collecType)
		{
		case CollectableType.Baylon:
			text = "Icon_Baylon";
			break;
		case CollectableType.Hommer:
			text = "Icon_Hommer";
			break;
		case CollectableType.Buhaira:
			text = "Icon_Buhaira";
			break;
		case CollectableType.Limpeza:
			text = "Icon_Limpeza";
			break;
		case CollectableType.Rubito:
			text = "Icon_Rubito";
			break;
		case CollectableType.Traptor:
			text = "Icon_Traptor";
			break;
		case CollectableType.Maletillas:
			text = "Icon_Maletillas";
			break;
		case CollectableType.Ramon:
			text = "Icon_Ramon";
			break;
		case CollectableType.Grumete:
			text = "Icon_Grumete";
			break;
		case CollectableType.GLA:
			text = "Icon_Glamour";
			break;
		case CollectableType.SLS:
			text = "Icon_SLS";
			break;
		case CollectableType.None:
			text = "NoCollectable";
			break;
		}
		if (text == string.Empty && Debug.isDebugBuild)
		{
			Debug.LogError("Revisar el emparejamiento de \"enumerado de tipos de collecionables\" con nombres de iconos para cada tipo");
		}
		return text;
	}

	public List<InfoCar> GetInfoCarFor()
	{
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.collectableType == collectableType)
			{
				infoCarList.Add(infoCar);
			}
		}
		return infoCarList;
	}
}
