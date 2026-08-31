using System.Collections;
using UnityEngine;

public class Lifeguard : MonoBehaviour
{
	private float distanMaxAllow = -78f;

	private Transform myTransform;

	private Rigidbody myRigidbody;

	private bool isLifeguardActive;

	private void OnEnable()
	{
		myRigidbody = GetComponent<Rigidbody>();
		myTransform = base.transform;
		isLifeguardActive = true;
		StartCoroutine(CheckYPosition());
	}

	private void OnDisable()
	{
		isLifeguardActive = false;
		StopCoroutine(CheckYPosition());
	}

	private IEnumerator CheckYPosition()
	{
		if (myTransform.position.y < distanMaxAllow)
		{
			myRigidbody.linearVelocity = Vector3.zero;
			myRigidbody.angularVelocity = Vector3.zero;
			LevelZone levelZone = MySingleton<Manager>.Instance.levelZone;
			yield return null;
			yield return null;
			if ((bool)levelZone)
			{
				LevelZone.UnloadAllZones(MySingleton<Manager>.Instance.gameLogic.allZones);
				yield return null;
				yield return null;
				yield return null;
				levelZone.ForceLoadLevel();
				yield return null;
			}
			myTransform.rotation = Quaternion.identity;
			myTransform.position = levelZone.spawnTransformZone.position;
		}
		yield return new WaitForSeconds(1f);
		if (isLifeguardActive)
		{
			StartCoroutine(CheckYPosition());
		}
	}
}
