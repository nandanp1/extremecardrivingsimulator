using UnityEngine;

namespace AxesInMotion.Network
{
	[RequireComponent(typeof(PhotonView))]
	public class NetworkBehaviour : MonoBehaviour
	{
		protected new PhotonView networkView;

		public NetworkPlayer owner
		{
			get
			{
				return new NetworkPlayer(networkView.owner);
			}
		}

		public object[] instantiationData
		{
			get
			{
				return networkView.instantiationData;
			}
		}

		public int viewID
		{
			get
			{
				return networkView.viewID;
			}
		}

		public int group
		{
			get
			{
				return networkView.group;
			}
		}

		public ViewSynchronization synchronization
		{
			get
			{
				return networkView.synchronization;
			}
		}

		public OwnershipOption ownershipTransfer
		{
			get
			{
				return networkView.ownershipTransfer;
			}
		}

		public bool isMine
		{
			get
			{
				return networkView.isMine;
			}
		}

		public Component observed
		{
			get
			{
				return networkView.observed;
			}
			set
			{
				networkView.observed = value;
			}
		}

		private void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			OnInstantiate(info);
		}

		protected virtual void Awake()
		{
			networkView = GetComponent<PhotonView>();
		}

		protected virtual void OnInstantiate(PhotonMessageInfo message)
		{
		}
	}
}
