using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarTimeout : NetworkBehaviour
{
	public float timeout = 120f;

	public float velocityThreshold = 5f;

	public float lastTimeUpdated;

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			Object.Destroy(this);
		}
		else if (!base.isMine)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (base.isMine)
		{
			if (GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > velocityThreshold || lastTimeUpdated == 0f)
			{
				lastTimeUpdated = Time.unscaledTime;
			}
			if (Time.unscaledTime - lastTimeUpdated > timeout)
			{
				lastTimeUpdated = 0f;
				OnTimeOut();
			}
		}
	}

	private void OnTimeOut()
	{
		MySingleton<Manager>.Instance.connection.Disconnect(DisconnectionCause.Inactivity);
		MySingleton<Manager>.Instance.connection.connectionLost = true;
		MySingleton<Manager>.Instance.connection.lostInGame = MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer;
		MySingleton<Manager>.Instance.connection.connectionLostCause = DisconnectionCause.Inactivity;
	}
}
