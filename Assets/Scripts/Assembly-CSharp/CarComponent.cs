using System;
using UnityEngine;

public class CarComponent : MonoBehaviour
{
	[Serializable]
	public class Components
	{
		public Transform LeftDHandle;

		public Transform LeftDoor;

		public Transform LeftChair;
	}

	public Components CarComponents;
}
