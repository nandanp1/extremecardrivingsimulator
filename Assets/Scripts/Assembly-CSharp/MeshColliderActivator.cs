using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MeshColliderActivator : MonoBehaviour
{
	private const float TIME_FOR_DISABLE_MESH_COLLIDER = 1f;

	private const float STEP_FOR_CHECK = 0.1f;

	private float meshColliderActive;

	private bool coroutineActive;

	public Mesh meshCol;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Car"))
		{
			return;
		}
		meshColliderActive++;
		if (meshColliderActive - 1f == 0f)
		{
			if (coroutineActive)
			{
				StopCoroutine(DisableMeshCollider());
			}
			base.gameObject.GetComponent<MeshCollider>().enabled = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag("Car"))
		{
			return;
		}
		meshColliderActive--;
		if (meshColliderActive == 0f)
		{
			if (coroutineActive)
			{
				StopCoroutine(DisableMeshCollider());
			}
			coroutineActive = true;
			StartCoroutine(DisableMeshCollider());
		}
		if (meshColliderActive < 0f)
		{
			Debug.LogError("Estado incosistente en meshColliderActive, se han producidos mas TriggerExit que TriggerEnter GameObjet: " + base.gameObject.name + " En: " + base.gameObject.transform.position);
		}
	}

	private IEnumerator DisableMeshCollider()
	{
		for (float total = 0f; total < 1f; total += 0.1f)
		{
			yield return new WaitForSeconds(0.1f);
		}
		base.gameObject.GetComponent<MeshCollider>().enabled = false;
		coroutineActive = false;
		yield return null;
	}
}
