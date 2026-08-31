using UnityEngine;

public class InfoCar : MonoBehaviour
{
	public enum ChallengeRewardType
	{
		NONE = 0,
		TRIAL = 1,
		RADAR = 2,
		CRASHABLE = 3
	}

	public enum PurchasablePack
	{
		none = 0,
		pack1 = 1,
		pack2 = 2,
		pack3 = 3,
		pack4 = 4
	}

	public string idName;

	public int id;

	public Color defaultColor;

	public bool paintable;

	public bool purchasable;

	public bool dailyReward;

	public ChallengeRewardType challegeRewardType;

	public ChallangeInfoCarExtension challangeExtension;

	public PurchasablePack purchasablePack;

	public float price;

	public Collectables.CollectableType collectableType;

	public int quantity;

	public bool mustBeLoggedInFacebook;

	public string iconName;

	public bool isChallangeReward
	{
		get
		{
			return challegeRewardType != ChallengeRewardType.NONE;
		}
	}

	[HideInInspector]
	public string keyCarForSettingsManager
	{
		get
		{
			return "keyCar" + id;
		}
	}

	[HideInInspector]
	public bool isCollectable
	{
		get
		{
			return collectableType != Collectables.CollectableType.None;
		}
	}

	public bool isBought
	{
		get
		{
			return MySingleton<Manager>.Instance.carsInfoPrefabs[0].id == id || SettingsManager.IsCarBought(keyCarForSettingsManager);
		}
		set
		{
			SettingsManager.SetCarBought(keyCarForSettingsManager, value);
		}
	}

	public void Initialize(InfoCar info)
	{
		id = info.id;
		price = info.price;
		collectableType = info.collectableType;
		quantity = info.quantity;
		mustBeLoggedInFacebook = info.mustBeLoggedInFacebook;
		idName = info.idName;
		defaultColor = info.defaultColor;
		paintable = info.paintable;
		iconName = info.iconName;
		purchasable = info.purchasable;
		purchasablePack = info.purchasablePack;
		challangeExtension = info.challangeExtension;
		dailyReward = info.dailyReward;
		challegeRewardType = info.challegeRewardType;
		if (paintable)
		{
			CarVisuals component = base.gameObject.GetComponent<CarVisuals>();
			component.Body.GetComponent<Renderer>().material.color = SettingsManager.GetCarColor(id, defaultColor);
			if (component.paintableLODMesh != null && component.paintableLODMesh.Length > 0)
			{
				for (int i = 0; i < component.paintableLODMesh.Length; i++)
				{
					component.paintableLODMesh[i].material.color = SettingsManager.GetCarColor(id, defaultColor);
				}
			}
		}
		if (isChallangeReward)
		{
			challangeExtension.LoadPlayerPref();
		}
	}
}
