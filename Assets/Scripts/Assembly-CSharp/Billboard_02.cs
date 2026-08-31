using UnityEngine;

public class Billboard_02 : MonoBehaviour
{
	private void OnEnable()
	{
		CancelInvoke("UpdateBillboarding");
	}

	private void OnDisable()
	{
		CancelInvoke("UpdateBillboarding");
	}

	private void OnBecameVisible()
	{
		InvokeRepeating("UpdateBillboarding", 0f, 0.05f);
	}

	private void OnBecameInvisible()
	{
		CancelInvoke("UpdateBillboarding");
	}

	private void UpdateBillboarding()
	{
		base.transform.LookAt(MySingleton<Manager>.Instance.gameLogic.currentCar.position);
	}
}
