using System.Collections.Generic;
using AxesInMotion.SocialPlugin;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class NetworkFriends : MonoBehaviour
	{
		public List<Friend> friends;

		public event OnRefreshFriendListDelegate OnRefreshFriendList;

		private void Start()
		{
			if ((bool)MySingleton<Manager>.Instance.networkManager)
			{
				MySingleton<Manager>.Instance.networkManager.OnAIMUpdateFriendsList += OnUpdateFriendsListCallback;
			}
			LoadPlayerPrefsFriends();
		}

		public void LoadPlayerPrefsFriends()
		{
			friends = new List<Friend>(SettingsManager.GetFriends());
		}

		public void UpdatePlayerPrefs()
		{
			SettingsManager.SetFriends(friends.ToArray());
		}

		public void AddFriend(string upid, string name, bool isFBFriend)
		{
			friends.Add(new Friend(upid, name, isFBFriend));
			UpdatePlayerPrefs();
			LoadPlayerPrefsFriends();
		}

		public void AddFriend(FBFriend fbFriend)
		{
			AddFriend(fbFriend.id, fbFriend.firstName, true);
		}

		public void RemoveFriend(string upid)
		{
			bool flag = false;
			Friend item = null;
			foreach (Friend friend in friends)
			{
				if (friend.upid.Equals(upid))
				{
					item = friend;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				friends.Remove(item);
				UpdatePlayerPrefs();
				LoadPlayerPrefsFriends();
			}
		}

		public bool IsFriend(NetworkPlayer player)
		{
			foreach (Friend friend in friends)
			{
				if (friend.upid.Equals(player.UPID))
				{
					return true;
				}
			}
			return false;
		}

		public void UpdateNameFriend(string friendUpid, string name)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (Friend friend in friends)
			{
				if (friend.upid.Equals(friendUpid))
				{
					flag = true;
					if (friend.lastKnownUsername != name)
					{
						Debug.Log("El jugador amigo " + friend.lastKnownUsername + " ha actualizado su nombre a " + name);
						friend.lastKnownUsername = name;
						flag2 = true;
					}
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("El jugador con nombre " + name + " y UPID " + friendUpid + " no está en la lista de amigos. ¿Seguro que es tu amigo?");
			}
			else if (flag && flag2)
			{
				UpdatePlayerPrefs();
			}
		}

		public Friend GetFriendByUPID(string upid)
		{
			foreach (Friend friend in friends)
			{
				if (friend.upid.Equals(upid))
				{
					return friend;
				}
			}
			return null;
		}

		public bool ContainsFBFriend(FBFriend fbFriend)
		{
			foreach (Friend friend in friends)
			{
				if (friend.upid.Equals(fbFriend.id))
				{
					return true;
				}
			}
			return false;
		}

		public void RefreshOnlineFriends()
		{
			if (friends.Count > 0)
			{
				string[] array = new string[friends.Count];
				for (int i = 0; i < friends.Count; i++)
				{
					array[i] = friends[i].upid;
				}
				MySingleton<Manager>.Instance.networkManager.FindFriends(array);
			}
		}

		private void OnUpdateFriendsListCallback()
		{
			foreach (FriendInfo friend in MySingleton<Manager>.Instance.networkManager.friends)
			{
				foreach (Friend friend2 in friends)
				{
					if (friend2.upid.Equals(friend.Name))
					{
						friend2.UpdateData(friend);
						break;
					}
				}
			}
			if (OnRefreshFriendList != null)
			{
				OnRefreshFriendList();
			}
		}
	}
}
