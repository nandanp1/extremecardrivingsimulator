using UnityEngine;

public class CheckpointLogic : MonoBehaviour
{
	public Transform[] checkpointsSorted;

	public int actualIndex;

	public void DoCheckpoint()
	{
		NGUITools.SetActive(checkpointsSorted[actualIndex].gameObject, false);
		actualIndex++;
		if (actualIndex > checkpointsSorted.Length - 1)
		{
			actualIndex = 0;
		}
		NGUITools.SetActive(checkpointsSorted[actualIndex].gameObject, true);
	}
}
