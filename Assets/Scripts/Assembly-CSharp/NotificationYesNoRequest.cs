using UnityEngine;

public class NotificationYesNoRequest : NotificationRequest
{
	private string title;

	private string message;

	private string yesText;

	private string noText;

	private Color textColor = Color.black;

	private Color backgroundColor = Color.white;

	private bool invertColorsOfNo;

	private bool invertColorsOfYes;

	private EventDelegate yesFunction;

	private EventDelegate noFunction;

	public NotificationYesNoRequest(string message, string yesText, string noText, EventDelegate yesFunction, EventDelegate noFunction, GameObject prefab)
		: base(prefab)
	{
		this.message = message;
		this.yesText = yesText;
		this.noText = noText;
		this.yesFunction = yesFunction;
		this.noFunction = noFunction;
	}

	public NotificationYesNoRequest(string title, string message, string yesText, string noText, Color textColor, Color backgroundColor, bool invertColorsOfYes, bool invertColorsOfNo, EventDelegate yesFunction, EventDelegate noFunction, GameObject prefab)
		: base(prefab)
	{
		this.title = title;
		this.message = message;
		this.yesText = yesText;
		this.noText = noText;
		this.textColor = textColor;
		this.backgroundColor = backgroundColor;
		this.invertColorsOfYes = invertColorsOfYes;
		this.invertColorsOfNo = invertColorsOfNo;
		this.yesFunction = yesFunction;
		this.noFunction = noFunction;
	}

	public override bool Create(out GameObject gameObject)
	{
		notificationObject = Object.Instantiate(notificationPrefab) as GameObject;
		NotificationYesNoObject component = notificationObject.GetComponent<NotificationYesNoObject>();
		component.lbTitle.text = title;
		component.lbMessage.text = message;
		component.lbYes.text = yesText;
		component.lbNo.text = noText;
		component.lbTitle.color = textColor;
		component.lbMessage.color = textColor;
		component.background.color = backgroundColor;
		if (invertColorsOfYes)
		{
			component.backgroundYes.color = textColor;
			component.lbYes.color = backgroundColor;
		}
		else
		{
			component.backgroundYes.color = Color.clear;
			component.lbYes.color = textColor;
		}
		if (invertColorsOfNo)
		{
			component.backgroundNo.color = textColor;
			component.lbNo.color = backgroundColor;
		}
		else
		{
			component.backgroundNo.color = Color.clear;
			component.lbNo.color = textColor;
		}
		component.yesFunction = yesFunction;
		component.noFunction = noFunction;
		gameObject = notificationObject;
		return true;
	}
}
