using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Network;
using AxesInMotion.SocialPlugin;
using Crosstales.BadWord;
using UnityEngine;

public class MultiplayerPanel : MonoBehaviour
{
	public string[] invalidSymbols = new string[2] { "_", "-" };

	public bool logged;

	public UIGrid grid;

	public GameObject friendEntryPrefab;

	public GameObject spLoginWithFacebook;

	public GameObject btRefresh;

	public UILabel lbNoPlayers;

	public GameObject btJoin;

	public GameObject spNickname;

	public UIInput inputNickname;

	public GameObject spFriends;

	public UIScrollView pnFriendList;

	private List<FriendEntry> friendEntries;

	private void Start()
	{
		MySingleton<Manager>.Instance.networkFriends.OnRefreshFriendList += OnFriendListRefreshedCallback;
		MySingleton<Manager>.Instance.networkManager.OnAIMJoinedLobby += OnJoinedLobbyCallback;
		inputNickname.value = SettingsManager.GetPlayername();
		CheckNicknameIsValidAndFix();
		CheckNicknameIsEmptyAndFix();
		SettingsManager.SetPlayername(inputNickname.value);
		MySingleton<Manager>.Instance.networkManager.player.name = SettingsManager.GetPlayername();
		RefreshGUI();
	}

	private void Update()
	{
		if (MySingleton<Manager>.Instance.facebookManager != null)
		{
			logged = MySingleton<Manager>.Instance.facebookManager.isLoggedIn;
		}
	}

	public void OnChangeNickname()
	{
		CheckNicknameIsValidAndFix();
		SettingsManager.SetPlayername(inputNickname.value);
		MySingleton<Manager>.Instance.networkManager.player.name = SettingsManager.GetPlayername();
	}

	private void CheckNicknameIsValidAndFix()
	{
		for (int i = 0; i < invalidSymbols.Length; i++)
		{
			inputNickname.value = inputNickname.value.Replace(invalidSymbols[i], string.Empty);
		}
		inputNickname.value = inputNickname.value.TrimStart();
		inputNickname.value = BadWordManager.ReplaceAll(inputNickname.value, "spanish", "english", "portuguese", "french", "japanese", "korean", "dutch", "polish", "russian", "chinese");
	}

	public void CheckNicknameIsEmptyAndFix()
	{
		if (inputNickname.value.Trim().Length == 0)
		{
			inputNickname.value = "New Player";
		}
	}

	public void RefreshGUI()
	{
		HideAll();
		NGUITools.SetActive(btJoin, true);
		NGUITools.SetActive(spNickname, true);
		if (MySingleton<Manager>.Instance.facebookManager.isLoggedIn)
		{
			NGUITools.SetActive(spLoginWithFacebook, false);
			NGUITools.SetActive(spFriends, true);
			NGUITools.SetActive(btRefresh, true);
			NGUITools.SetActive(pnFriendList.gameObject, true);
			if (MySingleton<Manager>.Instance.networkFriends.friends.Count == 0)
			{
				NGUITools.SetActive(lbNoPlayers.gameObject, true);
			}
		}
		else
		{
			NGUITools.SetActive(spLoginWithFacebook, true);
			NGUITools.SetActive(spFriends, false);
			NGUITools.SetActive(pnFriendList.gameObject, false);
			NGUITools.SetActive(lbNoPlayers.gameObject, false);
		}
	}

	private void HideAll()
	{
		NGUITools.SetActive(spLoginWithFacebook, false);
		NGUITools.SetActive(btRefresh, false);
		NGUITools.SetActive(lbNoPlayers.gameObject, false);
		NGUITools.SetActive(btJoin, false);
		NGUITools.SetActive(spNickname, false);
		NGUITools.SetActive(spFriends, false);
		NGUITools.SetActive(pnFriendList.gameObject, false);
	}

	public void LoginFacebook()
	{
		HideAll();
		MySingleton<Manager>.Instance.facebookManager.Login(LoginSuccessCallback, LoginFailedCallback, LoginCancelCallback, new SocialScope[1] { SocialScope.Friends });
	}

	public void LoginSuccessCallback()
	{
		if (SettingsManager.GetUPID() != MySingleton<Manager>.Instance.facebookManager.ID)
		{
			Debug.Log("Reconectando por UPID nuevo\nAnterior: " + SettingsManager.GetUPID() + "\nNuevo: " + MySingleton<Manager>.Instance.facebookManager.ID);
			SettingsManager.SetUPID(MySingleton<Manager>.Instance.facebookManager.ID);
			MySingleton<Manager>.Instance.networkManager.player.UPID = SettingsManager.GetUPID();
			MySingleton<Manager>.Instance.connection.Disconnect();
			MySingleton<Manager>.Instance.guiMenuLogic.ShowConnectingToMultiplayerModePanel();
		}
		else
		{
			RefreshGUI();
			RefreshFriendsList();
		}
	}

	public void LoginFailedCallback()
	{
		RefreshGUI();
	}

	public void LoginCancelCallback()
	{
		RefreshGUI();
	}

	public void Logout()
	{
		Debug.Log("Logout");
		MySingleton<Manager>.Instance.facebookManager.Logout();
		StartCoroutine(WaitAndRefreshAfterLogout());
	}

	private IEnumerator WaitAndRefreshAfterLogout()
	{
		yield return new WaitForSeconds(0.5f);
		RefreshGUI();
	}

	public void RefreshFriendsList()
	{
		if (MySingleton<Manager>.Instance.facebookManager.isLoggedIn)
		{
			MySingleton<Manager>.Instance.facebookManager.RequestGameFriends(RequestGameFriendsCallback);
		}
		else
		{
			MySingleton<Manager>.Instance.networkFriends.RefreshOnlineFriends();
		}
	}

	public void RequestGameFriendsCallback(FBFriend[] friends)
	{
		foreach (FBFriend fbFriend in friends)
		{
			if (!MySingleton<Manager>.Instance.networkFriends.ContainsFBFriend(fbFriend))
			{
				MySingleton<Manager>.Instance.networkFriends.AddFriend(fbFriend);
			}
		}
		MySingleton<Manager>.Instance.networkFriends.RefreshOnlineFriends();
	}

	private void OnJoinedLobbyCallback()
	{
		RefreshFriendsList();
	}

	public void OnFriendListRefreshedCallback()
	{
		ClearAllEntries();
		StartCoroutine(RefreshList());
	}

	private IEnumerator RefreshList()
	{
		NGUITools.SetActive(lbNoPlayers.gameObject, MySingleton<Manager>.Instance.networkFriends.friends.Count == 0);
		foreach (Friend friend in MySingleton<Manager>.Instance.networkFriends.friends)
		{
			if (!(friend.upid == MySingleton<Manager>.Instance.networkManager.player.UPID))
			{
				AddEntry(friend);
			}
		}
		yield return null;
		grid.Reposition();
		yield return new WaitForSeconds(0.1f);
		pnFriendList.ResetPosition();
	}

	private void AddEntry(Friend friend)
	{
		GameObject gameObject = NGUITools.AddChild(grid.gameObject, friendEntryPrefab);
		gameObject.GetComponent<FriendEntry>().SetFriend(friend);
	}

	private void ClearAllEntries()
	{
		pnFriendList.ResetPosition();
		foreach (Transform child in grid.GetChildList())
		{
			grid.RemoveChild(child);
			Object.DestroyImmediate(child.gameObject);
		}
		pnFriendList.ResetPosition();
	}

	private void OnDestroy()
	{
		if (MySingleton<Manager>.Instance != null)
		{
			MySingleton<Manager>.Instance.networkFriends.OnRefreshFriendList -= OnFriendListRefreshedCallback;
		}
	}
}
