using UnityEngine;

public class ChatPanel : MonoBehaviour
{
	public UITextList textList;

	public UIInput inputNewMessage;

	public Chat chatPlayer;

	public Color colorSystem = Color.yellow;

	private void Awake()
	{
		textList = GetComponentInChildren<UITextList>();
		inputNewMessage = GetComponentInChildren<UIInput>();
		inputNewMessage.label.maxLineCount = 1;
	}

	private void Start()
	{
		AddSystemMessage(Localization.Get("chat_welcome"));
		if (SettingsManager.GetIsObsolete())
		{
			ObsoleteMessage();
		}
	}

	public void AddSystemMessage(string message)
	{
		string text = "[c][" + Utils.ColorRBGAToHex(colorSystem) + "]" + message + "[-][/c]";
		if (!string.IsNullOrEmpty(text))
		{
			textList.Add(text);
			textList.scrollValue = 1f;
		}
	}

	public void ObsoleteMessage()
	{
		string text = "[c][" + Utils.ColorRBGAToHex(colorSystem) + "]" + Localization.Get("chat_new_version_available") + "[-][/c]";
		if (!string.IsNullOrEmpty(text))
		{
			textList.Add(text);
			textList.scrollValue = 1f;
		}
	}

	public void PlayerAddMessage(NetworkPlayer player)
	{
		string text = "[c][" + Utils.ColorRBGAToHex(colorSystem) + "]" + Localization.Get("chat_player_add") + "[-][/c]";
		Color color = player.GetColor();
		string text2 = "[c][" + Utils.ColorRBGAToHex(color) + "]" + player.name + "[-][/c]";
		string text3 = text2 + " " + text;
		if (!string.IsNullOrEmpty(text3))
		{
			textList.Add(text3);
			textList.scrollValue = 1f;
		}
	}

	public void PlayerLeftMessage(NetworkPlayer player)
	{
		string text = "[c][" + Utils.ColorRBGAToHex(colorSystem) + "]" + Localization.Get("chat_player_left") + "[-][/c]";
		Color color = player.GetColor();
		string text2 = "[c][" + Utils.ColorRBGAToHex(color) + "]" + player.name + "[-][/c]";
		string text3 = text2 + " " + text;
		if (!string.IsNullOrEmpty(text3))
		{
			textList.Add(text3);
			textList.scrollValue = 1f;
		}
	}

	public void AddMessage(NetworkPlayer player, string message)
	{
		if ((player.isLocal || !player.isIgnored) && textList != null)
		{
			string text = NGUIText.StripSymbols(message);
			Color color = player.GetColor();
			string text2 = "[c][" + Utils.ColorRBGAToHex(color) + "]" + player.name + "[-][/c]";
			text = text2 + ": " + text;
			if (!string.IsNullOrEmpty(text))
			{
				textList.Add(text);
				textList.scrollValue = 1f;
			}
		}
	}

	public void OnSubmit()
	{
		if (inputNewMessage.value.Length > 0)
		{
			chatPlayer.SendChatMessage(inputNewMessage.value);
		}
		inputNewMessage.value = string.Empty;
		inputNewMessage.isSelected = false;
	}
}
