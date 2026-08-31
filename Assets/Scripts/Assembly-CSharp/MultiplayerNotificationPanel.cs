using UnityEngine;

public class MultiplayerNotificationPanel : MonoBehaviour
{
	public enum NotificationType
	{
		RoomFull = 0
	}

	public UILabel lbMessage;

	public GUIMenuButton btButton;

	private void Start()
	{
		SetNotification(NotificationType.RoomFull);
	}

	public void SetNotification(NotificationType t)
	{
		if (t == NotificationType.RoomFull)
		{
			Localization.Get("multiplayernotification_roomfull");
			if (MySingleton<Manager>.Instance.networkManager.connected)
			{
				btButton.function = GUIMenuButton.MenuButtonFunction.GoToMultiplayerPanel;
			}
			else
			{
				btButton.function = GUIMenuButton.MenuButtonFunction.GoToConnectingToMultiplayerModePanel;
			}
		}
	}
}
