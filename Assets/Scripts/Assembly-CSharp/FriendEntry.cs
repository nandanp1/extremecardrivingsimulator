using AxesInMotion.Network;
using UnityEngine;

public class FriendEntry : MonoBehaviour
{
	public Color greenColor;

	public Color redColor;

	public Color greyColor;

	public new UILabel name;

	public UILabel moreInfo;

	public UILabel onlineStatus;

	public UILabel offlineStatus;

	public UISprite isFBFriend;

	public UISprite isNotFBFriend;

	public UIButton btRemoveFriend;

	public UIButton btJoin;

	private Friend friend;

	private void Start()
	{
		GetComponent<UISprite>().SetAnchor(GetComponentInParent<UIPanel>().transform);
	}

	public void SetFriend(Friend f)
	{
		NGUITools.SetActive(name.gameObject, false);
		NGUITools.SetActive(moreInfo.gameObject, false);
		NGUITools.SetActive(onlineStatus.gameObject, false);
		NGUITools.SetActive(offlineStatus.gameObject, false);
		NGUITools.SetActive(isFBFriend.gameObject, false);
		NGUITools.SetActive(isNotFBFriend.gameObject, false);
		NGUITools.SetActive(btRemoveFriend.gameObject, true);
		NGUITools.SetActive(btJoin.gameObject, true);
		btJoin.isEnabled = false;
		friend = f;
		name.text = friend.lastKnownUsername;
		NGUITools.SetActive(name.gameObject, true);
		NGUITools.SetActive(isFBFriend.gameObject, friend.isFBFriend);
		btRemoveFriend.isEnabled = !friend.isFBFriend;
		NGUITools.SetActive(isNotFBFriend.gameObject, !friend.isFBFriend);
		if (friend.isOnline)
		{
			NGUITools.SetActive(offlineStatus.gameObject, false);
			if (friend.isInRoom)
			{
				NGUITools.SetActive(moreInfo.gameObject, true);
				moreInfo.text = string.Format(Localization.Get("room_multiplayer"), Mathf.FloorToInt(friend.playersInRoom.x), Mathf.FloorToInt(friend.playersInRoom.y));
				if (friend.isRoomFull)
				{
					btJoin.isEnabled = false;
					moreInfo.color = redColor;
				}
				else
				{
					btJoin.isEnabled = true;
					moreInfo.color = greenColor;
				}
			}
			else
			{
				NGUITools.SetActive(onlineStatus.gameObject, true);
			}
		}
		else
		{
			NGUITools.SetActive(offlineStatus.gameObject, true);
		}
	}

	public void OnClickJoin()
	{
		Vector2 playersInRoom = MySingleton<Manager>.Instance.networkRooms.GetPlayersInRoom(friend.roomName);
		if (playersInRoom.x < playersInRoom.y)
		{
			MySingleton<Manager>.Instance.networkRooms.targetRoom = friend.roomName;
			MySingleton<Manager>.Instance.networkGame.miniGameSelected = friend.minigameType;
			MySingleton<Manager>.Instance.guiMenuLogic.GoToConnectingPanel();
		}
		else
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowMultiplayerNotification(MultiplayerNotificationPanel.NotificationType.RoomFull);
		}
	}

	public void OnClickRemoveFriend()
	{
		string message = string.Format(Localization.Get("multiplayer_friend_remove_confirmation"), friend.lastKnownUsername);
		MySingleton<Manager>.Instance.notificationManager.RequestNotificationYesNo(message, Localization.Get("yes"), Localization.Get("no"), new EventDelegate(this, "RemoveFriend"), null);
	}

	private void RemoveFriend()
	{
		MySingleton<Manager>.Instance.networkFriends.RemoveFriend(friend.upid);
		if (LegacySceneManager.LoadedLevel == 1)
		{
			((GUIMenuLogic)MySingleton<Manager>.Instance.guiCurrentLogic).RefreshFriendList();
		}
	}
}
