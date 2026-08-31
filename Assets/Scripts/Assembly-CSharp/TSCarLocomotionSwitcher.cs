using UnityEngine;

public class TSCarLocomotionSwitcher : MonoBehaviour
{
	public TSNoPhysicCar noPhysicsLocomotion;

	public TSSimpleCar simpleLocomotion;

	private void Awake()
	{
		noPhysicsLocomotion = base.transform.root.GetComponent<TSNoPhysicCar>();
		simpleLocomotion = base.transform.root.GetComponent<TSSimpleCar>();
	}

	private void Update()
	{
	}

	[ContextMenu("SwitchToNoPhysics")]
	public void SwitchToNoPhysics()
	{
		if (!simpleLocomotion.enabled)
		{
			return;
		}
		simpleLocomotion.enabled = false;
		noPhysicsLocomotion.speed = simpleLocomotion.speed;
		noPhysicsLocomotion.navigation.physicalLocomotion = false;
		simpleLocomotion.throttle = 0f;
		if ((bool)GetComponent<Rigidbody>())
		{
			if (Application.isPlaying)
			{
				Object.Destroy(GetComponent<Rigidbody>());
			}
			else
			{
				Object.DestroyImmediate(GetComponent<Rigidbody>());
			}
			simpleLocomotion.body = null;
			simpleLocomotion.bodies = new Rigidbody[0];
		}
		for (int i = 0; i < simpleLocomotion.FrontWheels.Length; i++)
		{
			simpleLocomotion.FrontWheels[i].GetComponentInChildren<TSSimpleCar_Wheel>().enabled = false;
			simpleLocomotion.FrontWheels[i].enabled = false;
		}
		for (int j = 0; j < simpleLocomotion.BackWheels.Length; j++)
		{
			simpleLocomotion.BackWheels[j].GetComponentInChildren<TSSimpleCar_Wheel>().enabled = false;
			simpleLocomotion.BackWheels[j].enabled = false;
		}
		noPhysicsLocomotion.enabled = true;
	}

	[ContextMenu("SwitchToSimple")]
	public void SwitchToSimple()
	{
		if (!simpleLocomotion.enabled)
		{
			noPhysicsLocomotion.navigation.physicalLocomotion = true;
			noPhysicsLocomotion.enabled = false;
			noPhysicsLocomotion.throttle = 0f;
			if (!GetComponent<Rigidbody>())
			{
				simpleLocomotion.body = base.gameObject.AddComponent<Rigidbody>();
				simpleLocomotion.bodies = new Rigidbody[1] { simpleLocomotion.body };
				simpleLocomotion.body.mass = 1500f;
				simpleLocomotion.body.linearVelocity = noPhysicsLocomotion.velo;
			}
			simpleLocomotion.enabled = true;
			for (int i = 0; i < simpleLocomotion.FrontWheels.Length; i++)
			{
				simpleLocomotion.FrontWheels[i].GetComponentInChildren<TSSimpleCar_Wheel>().enabled = true;
				simpleLocomotion.FrontWheels[i].enabled = true;
			}
			for (int j = 0; j < simpleLocomotion.BackWheels.Length; j++)
			{
				simpleLocomotion.BackWheels[j].GetComponentInChildren<TSSimpleCar_Wheel>().enabled = true;
				simpleLocomotion.BackWheels[j].enabled = true;
			}
		}
	}
}
