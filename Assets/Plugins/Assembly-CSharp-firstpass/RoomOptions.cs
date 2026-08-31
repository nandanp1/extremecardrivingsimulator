using ExitGames.Client.Photon;

public class RoomOptions
{
	public int maxPlayers;

	private bool isVisibleField = true;

	private bool isOpenField = true;

	private bool cleanupCacheOnLeaveField = PhotonNetwork.autoCleanUpPlayerObjects;

	public Hashtable customRoomProperties;

	public string[] customRoomPropertiesForLobby = new string[0];

	public bool isVisible
	{
		get
		{
			return isVisibleField;
		}
		set
		{
			isVisibleField = value;
		}
	}

	public bool isOpen
	{
		get
		{
			return isOpenField;
		}
		set
		{
			isOpenField = value;
		}
	}

	public bool cleanupCacheOnLeave
	{
		get
		{
			return cleanupCacheOnLeaveField;
		}
		set
		{
			cleanupCacheOnLeaveField = value;
		}
	}
}
