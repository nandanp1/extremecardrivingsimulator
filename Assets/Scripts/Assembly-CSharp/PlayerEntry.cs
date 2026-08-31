using UnityEngine;

public class PlayerEntry : MonoBehaviour
{
	public UILabel lbPlayerName;

	public UIButton btAddFriend;

	public UIButton btMutePlayer;

	public UIButton btUnMutePlayer;

	public bool isAlreadyMyFriend;

	public UISprite backgroundEntry;

	private NetworkPlayer player;

	public void SetPlayer(NetworkPlayer player)
	{
		this.player = player;
		lbPlayerName.text = this.player.name;
		Color color = this.player.GetColor();
		color.a = 1f;
		lbPlayerName.color = color;
		if (!player.isLocal)
		{
			bool isLoggedIn = MySingleton<Manager>.Instance.facebookManager.isLoggedIn;
			bool flag = player.UPID != "none";
			isAlreadyMyFriend = MySingleton<Manager>.Instance.networkFriends.IsFriend(player);
			bool state = isLoggedIn && flag && !isAlreadyMyFriend;
			NGUITools.SetActive(btAddFriend.gameObject, state);
			NGUITools.SetActive(btMutePlayer.gameObject, !player.isIgnored);
			NGUITools.SetActive(btUnMutePlayer.gameObject, player.isIgnored);
		}
		else
		{
			NGUITools.SetActive(btMutePlayer.gameObject, false);
			NGUITools.SetActive(btAddFriend.gameObject, false);
			NGUITools.SetActive(btMutePlayer.gameObject, false);
			NGUITools.SetActive(btUnMutePlayer.gameObject, false);
		}
	}

	public void ShowAddFriendNotification()
	{
		MySingleton<Manager>.Instance.guiGameLogic.playerEntrySelected = this;
		MySingleton<Manager>.Instance.guiGameLogic.ShowAddFriendConfirmation();
	}

	public void AddThisPlayerAsFriend()
	{
		MySingleton<Manager>.Instance.networkFriends.AddFriend(player.UPID, player.name, false);
		if (MySingleton<Manager>.Instance.networkFriends.IsFriend(player))
		{
			NGUITools.SetActive(btAddFriend.gameObject, false);
		}
	}

	public void IgnorePlayer()
	{
		MySingleton<Manager>.Instance.networkGame.SetIgnorePlayer(player, true);
		NGUITools.SetActive(btMutePlayer.gameObject, !player.isIgnored);
		NGUITools.SetActive(btUnMutePlayer.gameObject, player.isIgnored);
	}

	public void UnignorePlayer()
	{
		MySingleton<Manager>.Instance.networkGame.SetIgnorePlayer(player, false);
		NGUITools.SetActive(btMutePlayer.gameObject, !player.isIgnored);
		NGUITools.SetActive(btUnMutePlayer.gameObject, player.isIgnored);
	}
}
