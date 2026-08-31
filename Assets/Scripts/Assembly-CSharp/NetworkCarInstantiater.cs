using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarInstantiater : NetworkBehaviour
{
	protected override void Awake()
	{
		base.Awake();
		if (base.isMine)
		{
			InstantiateMyCar();
		}
	}

	private void InstantiateMyCar()
	{
		int num = PhotonNetwork.AllocateViewID();
		InfoCar infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar)];
		networkView.RPC("InstantiateCarEveryWhere", PhotonTargets.AllBuffered, infoCar.idName, base.transform.position, base.transform.rotation, num, networkView.owner);
	}

	[RPC]
	private void InstantiateCarEveryWhere(string idName, Vector3 position, Quaternion rotation, int viewID, PhotonPlayer sender)
	{
		bool flag = false;
		GameObject gameObject = Resources.Load(idName) as GameObject;
		if (gameObject == null)
		{
			gameObject = Resources.Load("NullCar") as GameObject;
			flag = true;
		}
		gameObject.GetComponent<PhotonView>().viewID = viewID;
		GameObject gameObject2 = Object.Instantiate(gameObject, position, rotation) as GameObject;
		if (flag)
		{
			gameObject2.GetComponent<NetworkCarInitializer>().isNullCar = flag;
		}
		if (base.isMine)
		{
			MySingleton<Manager>.Instance.gameLogic.instantiatedCar = gameObject2;
			MySingleton<Manager>.Instance.gameLogic.currentCar = gameObject2.transform;
			MySingleton<Manager>.Instance.gameLogic.carControl = gameObject2.GetComponent<CarControl>();
			MySingleton<Manager>.Instance.gameLogic.carSound = gameObject2.GetComponent<CarSound>();
			Camera.main.GetComponent<VehicleCamera>().ChangeTarget(MySingleton<Manager>.Instance.gameLogic.currentCar);
		}
		else
		{
			gameObject.GetComponent<PhotonView>().TransferOwnership(sender);
		}
		base.transform.SetParent(gameObject2.transform);
		base.gameObject.name = "NetworkCarLink";
	}
}
