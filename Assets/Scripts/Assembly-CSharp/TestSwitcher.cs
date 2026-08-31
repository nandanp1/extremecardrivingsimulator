using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TestSwitcher : MonoBehaviour
{
	public float radiusDetection = 30f;

	private void Awake()
	{
		base.gameObject.layer = LayerMask.NameToLayer("TrafficAI");
		((SphereCollider)base.gameObject.GetComponent<Collider>()).radius = radiusDetection;
		GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.tag == "carcollider")
		{
			TSCarLocomotionSwitcher componentInParent = c.transform.GetComponentInParent<TSCarLocomotionSwitcher>();
			if ((bool)componentInParent)
			{
				componentInParent.SwitchToSimple();
			}
		}
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.tag == "carcollider")
		{
			TSCarLocomotionSwitcher componentInParent = c.transform.GetComponentInParent<TSCarLocomotionSwitcher>();
			if ((bool)componentInParent)
			{
				componentInParent.SwitchToNoPhysics();
			}
		}
	}
}
