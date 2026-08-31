using AxesInMotion.Network.Minigames;
using ExitGames.Client.Photon;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class Friend
	{
		public string lastKnownUsername;

		public string upid;

		public bool isOnline;

		public bool isInRoom;

		public string roomName;

		public MiniGameLogic.MiniGameType minigameType;

		public Hashtable roomProperties;

		public bool isRoomFull;

		public bool isFBFriend;

		public Vector2 playersInRoom;

		public Friend(string upid, string lastKnownUsername, bool fbFriend)
		{
			this.upid = upid;
			this.lastKnownUsername = lastKnownUsername;
			isFBFriend = fbFriend;
			isOnline = false;
			isInRoom = false;
			roomName = null;
			minigameType = MiniGameLogic.MiniGameType.None;
			roomProperties = null;
			isRoomFull = false;
			playersInRoom = new Vector2(0f, MySingleton<Manager>.Instance.connection.maxPlayersPerRoom);
		}

		public static string FriendsToString(Friend[] friends)
		{
			string text = string.Empty;
			for (int i = 0; i < friends.Length; i++)
			{
				string text2 = text;
				text = text2 + friends[i].upid + "-" + friends[i].lastKnownUsername + "-" + (friends[i].isFBFriend ? 1 : 0);
				if (i < friends.Length - 1)
				{
					text += "_";
				}
			}
			return text;
		}

		public static Friend[] StringToFriends(string rawData)
		{
			if (rawData == "none")
			{
				return new Friend[0];
			}
			string[] array = rawData.Split('_');
			Friend[] array2 = new Friend[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string[] array3 = array[i].Split('-');
				array2[i] = new Friend(array3[0], array3[1], array3[2] == "1");
			}
			return array2;
		}

		public void UpdateData(FriendInfo info)
		{
			isOnline = info.IsOnline;
			isInRoom = info.IsInRoom;
			roomName = info.Room;
			RoomInfo room = MySingleton<Manager>.Instance.networkRooms.GetRoom(roomName);
			playersInRoom = MySingleton<Manager>.Instance.networkRooms.GetPlayersInRoom(roomName);
			if (room != null)
			{
				roomProperties = room.customProperties;
				minigameType = (MiniGameLogic.MiniGameType)(int)roomProperties["gameMode"];
				if (minigameType == MiniGameLogic.MiniGameType.Racing)
				{
					playersInRoom.y = 4f;
					byte b = (byte)roomProperties["customFull"];
					isRoomFull = b == 1;
				}
			}
			else
			{
				isRoomFull = playersInRoom.x == playersInRoom.y;
			}
		}

		public override string ToString()
		{
			return lastKnownUsername + " " + upid + ((!isFBFriend) ? string.Empty : " FB") + ((!isOnline) ? string.Empty : " ONLINE") + ((!isInRoom) ? string.Empty : " PLAYING") + ((!isOnline) ? string.Empty : ((!isRoomFull) ? "ROOM_AVAILABLE" : " ROOM_FULL"));
		}
	}
}
