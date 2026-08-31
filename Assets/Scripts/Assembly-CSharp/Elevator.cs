using UnityEngine;

public class Elevator : MonoBehaviour
{
	private bool reparented;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Collider>().tag == "Car" && !reparented)
		{
			reparented = true;
			other.transform.parent.transform.parent = base.transform;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<Collider>().tag == "Car" && reparented)
		{
			reparented = false;
			other.transform.parent.transform.parent = null;
		}
	}
}
