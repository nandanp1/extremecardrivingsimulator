using AxesInMotion.Network.Minigames;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class NetworkCarInitializer : NetworkBehaviour
	{
		public bool isNullCar;

		public bool isStoreCar;

		protected override void Awake()
		{
			base.Awake();
			if (isStoreCar)
			{
				DisableCarComponents();
				Object.DestroyImmediate(this);
				Object.DestroyImmediate(networkView);
			}
			else if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
			{
				Object.DestroyImmediate(this);
			}
		}

		private void Start()
		{
			if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer && !isStoreCar)
			{
				SetName();
				MySingleton<Manager>.Instance.networkGame.RemoteCarInstantiated(base.gameObject, base.owner);
				if (base.isMine)
				{
					SetAsMyCar();
				}
				else
				{
					SetAsOtherCar();
				}
				SetCommonProperties();
			}
			Object.Destroy(this);
		}

		private void SetName()
		{
			string text = base.gameObject.name;
			text = text.Replace("(Clone)", string.Empty);
			text = text + "-" + networkView.ownerId + "-" + base.owner.name;
			base.gameObject.name = text;
		}

		private void SetCommonProperties()
		{
			base.gameObject.AddMissingComponent<NetworkCarSynchronizer>();
			if (MySingleton<Manager>.Instance.networkGame.miniGameSelected == MiniGameLogic.MiniGameType.Racing)
			{
				base.gameObject.GetComponent<NetworkCarSynchronizer>().synchronizationType = NetworkCarSynchronizer.SynchronizationType.None;
			}
			base.gameObject.AddMissingComponent<Chat>();
			if (base.isMine)
			{
				base.gameObject.GetComponent<Chat>().carTimeout = base.gameObject.GetComponent<NetworkCarTimeout>();
			}
		}

		private void SetAsMyCar()
		{
			InfoCar info = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar)];
			InfoCar infoCar = base.gameObject.AddMissingComponent<InfoCar>();
			infoCar.Initialize(info);
			GetComponent<CarControl>().FindGameLogic();
			base.gameObject.AddMissingComponent<NetworkCarTimeout>();
		}

		private void SetAsOtherCar()
		{
			CarVisuals component = base.gameObject.GetComponent<CarVisuals>();
			component.interiorRoot.SetActive(true);
			Transform transform = component.exteriorRoot.transform;
			transform.gameObject.layer = LayerMask.NameToLayer("RemoteCar");
			ReplaceLayerHierarchically(transform, "Car", "RemoteCar");
			NetworkPlayer networkPlayer = base.owner;
			Color color = networkPlayer.GetColor();
			if (!isNullCar)
			{
				component.SetCarColor(color);
			}
			component.colliderBase.gameObject.tag = "RemotePlayer";
			component.interiorRoot.SetActive(false);
			component.radarMarker = base.gameObject.AddMissingComponent<NetworkCarRadarMarker>().Initialize(color);
			component.nameMarker = base.gameObject.AddMissingComponent<NetworkCarNameMarker>().Initialize(color);
			if (MySingleton<Manager>.Instance.networkGame.miniGameSelected == MiniGameLogic.MiniGameType.Racing)
			{
				component.iconMarker = base.gameObject.AddMissingComponent<NetworkCarIconMarker>().Initialize(color);
				component.iconMarker.SetActive(false);
			}
			DisableCarComponents();
		}

		private void DisableCarComponents()
		{
			CarWheel[] componentsInChildren = GetComponentsInChildren<CarWheel>();
			foreach (CarWheel obj in componentsInChildren)
			{
				Object.DestroyImmediate(obj);
			}
			WheelCollider[] componentsInChildren2 = GetComponentsInChildren<WheelCollider>();
			foreach (WheelCollider obj2 in componentsInChildren2)
			{
				Object.DestroyImmediate(obj2);
			}
			Object.DestroyImmediate(GetComponent<Rigidbody>());
			base.gameObject.GetComponent<CarControl>().enabled = false;
			base.gameObject.GetComponent<CarCameras>().enabled = false;
			base.gameObject.GetComponent<CarSettings>().enabled = false;
			CarAntiRollBar[] components = base.gameObject.GetComponents<CarAntiRollBar>();
			foreach (CarAntiRollBar carAntiRollBar in components)
			{
				carAntiRollBar.enabled = false;
			}
			base.gameObject.GetComponent<CarDamage>().enabled = false;
			base.gameObject.GetComponent<CarVisuals>().enabled = false;
			base.gameObject.GetComponent<CarExternalInputRandom>().enabled = false;
			base.gameObject.GetComponent<CarSound>().enabled = false;
			base.gameObject.GetComponent<CarLogic>().enabled = false;
		}

		private void OnEnable()
		{
			if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
			{
				Object.Destroy(this);
			}
		}

		private void ReplaceLayerHierarchically(Transform parentTransform, string previousLayerName, string newLayerName)
		{
			for (int i = 0; i < parentTransform.transform.childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				if (LayerMask.LayerToName(child.gameObject.layer).Equals(previousLayerName))
				{
					child.gameObject.layer = LayerMask.NameToLayer(newLayerName);
				}
				ReplaceLayerHierarchically(child, previousLayerName, newLayerName);
			}
		}
	}
}
