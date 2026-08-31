using UnityEngine;

public class GamePersistentInfo : MonoBehaviour
{
	public GameObject collectablesAllZones;

	public GameObject[] networkBounds;

	public GameObject[] lods;

	private void Start()
	{
		MySingleton<Manager>.Instance.gameLogic.persistentCollectablesParent = collectablesAllZones;
	}

	public void EnableNetworkBounds()
	{
		GameObject[] array = networkBounds;
		foreach (GameObject gameObject in array)
		{
			gameObject.gameObject.SetActive(true);
		}
	}
}
