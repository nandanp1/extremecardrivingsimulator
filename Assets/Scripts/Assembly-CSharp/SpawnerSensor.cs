using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnerSensor : MonoBehaviour
{
	[HideInInspector]
	public SpawnerMaster spawnerMaster;

	private void Awake()
	{
		spawnerMaster = GetComponentInParent<SpawnerMaster>();
	}

	private void OnTriggerEnter(Collider other)
	{
		spawnerMaster.OnSpawnerSensorTriggerEnter(this, other);
	}
}
