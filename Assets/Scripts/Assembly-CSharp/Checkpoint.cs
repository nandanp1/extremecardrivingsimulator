using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	private CheckpointLogic logic;

	private void Start()
	{
		logic = base.transform.parent.GetComponent<CheckpointLogic>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Car")
		{
			logic.DoCheckpoint();
		}
	}
}
