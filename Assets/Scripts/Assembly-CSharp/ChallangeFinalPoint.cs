using UnityEngine;

public class ChallangeFinalPoint : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic.OnPassThroughAllCheckpoint(MySingleton<Manager>.Instance.guiChallangeModeLogic.radarManager.lastCheckpoint.GetComponent<CheckpointScript>());
	}
}
