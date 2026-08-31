using UnityEngine;

public class LookAtChekpoint : MonoBehaviour
{
	public float speed = 8f;

	private Transform mTrans;

	private void Awake()
	{
		mTrans = base.transform;
	}

	private void OnEnable()
	{
		if (!MySingleton<Manager>.Instance.isChallangeAvailable || !MySingleton<Manager>.Instance.guiGameLogic)
		{
			base.enabled = false;
		}
		else
		{
			MySingleton<Manager>.Instance.guiGameLogic.cameraPointerTransform = mTrans;
		}
	}

	private void LateUpdate()
	{
		if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress && MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar && (bool)MySingleton<Manager>.Instance.guiChallangeModeLogic.radarManager)
		{
			Vector3 forward = MySingleton<Manager>.Instance.guiChallangeModeLogic.radarManager.nextRadar.position - mTrans.position;
			float magnitude = forward.magnitude;
			if (magnitude > 0.001f)
			{
				Quaternion to = Quaternion.LookRotation(forward);
				mTrans.rotation = Quaternion.Slerp(mTrans.rotation, to, Mathf.Clamp01(speed * Time.deltaTime));
			}
		}
	}
}
