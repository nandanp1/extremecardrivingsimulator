using System.Collections;
using AxesInMotion.Network;
using Crosstales.BadWord;
using UnityEngine;

public class Chat : NetworkBehaviour
{
	public ChatPanel _chatPanel;

	public NetworkCarTimeout carTimeout;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		StartCoroutine(WaitAndSearchChatPanel());
	}

	public void SendChatMessage(string message)
	{
		carTimeout.lastTimeUpdated = Time.unscaledTime;
		string text = BadWordManager.ReplaceAll(message, "spanish", "english", "portuguese", "french", "japanese", "korean", "dutch", "polish", "russian", "chinese");
		networkView.RPC("RPC_SendMessageToChat", PhotonTargets.All, text);
	}

	[RPC]
	private void RPC_SendMessageToChat(string message, PhotonMessageInfo info)
	{
		string message2 = BadWordManager.ReplaceAll(message, "spanish", "english", "portuguese", "french", "japanese", "korean", "dutch", "polish", "russian", "chinese");
		_chatPanel.AddMessage(new NetworkPlayer(info.sender), message2);
	}

	private IEnumerator WaitAndSearchChatPanel()
	{
		yield return new WaitForSeconds(1f);
		_chatPanel = MySingleton<Manager>.Instance.guiGameLogic.pnChat.GetComponent<ChatPanel>();
		if (base.isMine)
		{
			_chatPanel.chatPlayer = this;
		}
	}
}
