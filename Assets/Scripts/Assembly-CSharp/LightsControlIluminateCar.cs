using UnityEngine;

public class LightsControlIluminateCar : MonoBehaviour
{
	private int layerMask;

	private void Awake()
	{
		// Engine API calls in MonoBehaviour field initializers are rejected by
		// current Unity versions, so initialize the cached layer at runtime.
		layerMask = LayerMask.NameToLayer("Car");
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider myTrigger)
	{
		if (myTrigger.gameObject.layer == layerMask)
		{
			Transform parent = myTrigger.transform.parent;
			if (parent != null)
			{
				parent = parent.transform.parent;
				if (parent.gameObject.layer != layerMask)
				{
				}
			}
		}
		Debug.Log("Yeeeee");
	}

	private void OnTriggerExit(Collider myTrigger)
	{
		if (myTrigger.gameObject.layer != layerMask)
		{
			return;
		}
		Transform parent = myTrigger.transform.parent;
		if (parent != null)
		{
			parent = parent.transform.parent;
			if (parent.gameObject.layer != layerMask)
			{
			}
		}
	}
}
