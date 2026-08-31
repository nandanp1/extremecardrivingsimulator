using UnityEngine;

public class RandomSpawns : MonoBehaviour
{
	public Transform[] spawns;

	private void Awake()
	{
		if (spawns == null || spawns.Length == 0)
		{
			spawns = GetComponentsInChildren<Transform>();
		}
	}

	public Transform GetSpawn()
	{
		int num = Random.Range(0, spawns.Length);
		for (int i = 0; i < spawns.Length; i++)
		{
			Transform transform = spawns[num];
			if (Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Car")).Length == 0)
			{
				return transform;
			}
			num = (num + 1) % spawns.Length;
		}
		return spawns[0];
	}
}
