using System.Collections;
using AxesInMotion.Network;
using UnityEngine;

public class ConnectToMultiplayeModePanel : MonoBehaviour
{
	public UILabel lbMessage;

	public UILabel lbError;

	private void Start()
	{
		MySingleton<Manager>.Instance.networkManager.OnAIMFailedToConnect += OnFailedConnectToServersCallback;
		MySingleton<Manager>.Instance.networkManager.OnAIMJoinedLobby += OnConnecToLobbyCallback;
	}

	private void OnEnable()
	{
		NGUITools.SetActive(lbError.gameObject, false);
		NGUITools.SetActive(lbMessage.gameObject, true);
		if (!MySingleton<Manager>.Instance.connection.connected)
		{
			MySingleton<Manager>.Instance.connection.Connect(true, false);
		}
		else
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowMenu();
		}
	}

	private void OnFailedConnectToServersCallback(DisconnectionCause cause)
	{
		NGUITools.SetActive(lbError.gameObject, true);
		NGUITools.SetActive(lbMessage.gameObject, false);
		StartCoroutine(WaitAndGoToMenu());
	}

	private IEnumerator WaitAndGoToMenu()
	{
		yield return new WaitForSeconds(2f);
		MySingleton<Manager>.Instance.guiMenuLogic.ShowMenu();
	}

	private void OnConnecToLobbyCallback()
	{
		MySingleton<Manager>.Instance.guiMenuLogic.ShowMultiplayerPanel();
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			MySingleton<Manager>.Instance.networkManager.OnAIMFailedToConnect -= OnFailedConnectToServersCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMJoinedLobby -= OnConnecToLobbyCallback;
		}
	}
}
