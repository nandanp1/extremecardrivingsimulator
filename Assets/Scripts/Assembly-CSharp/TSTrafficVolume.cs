using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TSTrafficVolume : MonoBehaviour
{
	public int maxAllowedCars = 10;

	public List<TSTrafficAI> carsOnThisSection = new List<TSTrafficAI>();

	private void OnTriggerExit(Collider car)
	{
		if (car.gameObject.layer == LayerMask.NameToLayer("Traffic Cars"))
		{
			carsOnThisSection.Remove(car.GetComponent<TSTrafficAI>());
		}
	}
}
