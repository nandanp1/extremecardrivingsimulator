using System.Collections;
using UnityEngine;

public class PlayersPanel : MonoBehaviour
{
	public UIGrid grid;

	public UIScrollView scrollView;

	public GameObject playerEntryPrefab;

	public Color tintColorLocalPlayer;

	private void Start()
	{
		MySingleton<Manager>.Instance.networkManager.OnAIMPlayerConnected += OnPlayerConnectedCallback;
		MySingleton<Manager>.Instance.networkManager.OnAIMPlayerDisconnected += OnPlayerDisconnectedCallback;
	}

	public void RefreshPlayerList()
	{
		StartCoroutine(RefreshList());
	}

	private IEnumerator RefreshList()
	{
		ClearAllEntries();
		NetworkPlayer[] playerList = MySingleton<Manager>.Instance.networkManager.playerList;
		foreach (NetworkPlayer player in playerList)
		{
			AddEntry(player);
		}
		yield return null;
		grid.Reposition();
		yield return new WaitForSeconds(0.1f);
		scrollView.ResetPosition();
	}

	private void AddEntry(NetworkPlayer player)
	{
		GameObject gameObject = NGUITools.AddChild(grid.gameObject, playerEntryPrefab);
		PlayerEntry component = gameObject.GetComponent<PlayerEntry>();
		component.SetPlayer(player);
		if (player.isLocal)
		{
			component.backgroundEntry.color = tintColorLocalPlayer;
		}
	}

	private void ClearAllEntries()
	{
		scrollView.ResetPosition();
		foreach (Transform child in grid.GetChildList())
		{
			grid.RemoveChild(child);
			Object.DestroyImmediate(child.gameObject);
		}
		scrollView.ResetPosition();
	}

	public void OnPlayerConnectedCallback(NetworkPlayer p)
	{
		if (base.gameObject.activeSelf)
		{
			RefreshPlayerList();
		}
	}

	public void OnPlayerDisconnectedCallback(NetworkPlayer p)
	{
		if (base.gameObject.activeSelf)
		{
			RefreshPlayerList();
		}
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerConnected -= OnPlayerConnectedCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerDisconnected -= OnPlayerDisconnectedCallback;
		}
	}
}
