using UnityEngine;

public class NotificationOKRequest : NotificationRequest
{
	private string message;

	private string okMessage;

	public NotificationOKRequest(string message, string okMessage, GameObject prefab)
		: base(prefab)
	{
		this.message = message;
		this.okMessage = okMessage;
	}

	public override bool Create(out GameObject gameObject)
	{
		notificationObject = Object.Instantiate(notificationPrefab) as GameObject;
		NotificationOKObject component = notificationObject.GetComponent<NotificationOKObject>();
		component.lbMessage.text = message;
		component.lbOK.text = okMessage;
		gameObject = notificationObject;
		return true;
	}
}
