using UnityEngine;

public class UISlideButton : UIButton
{
	protected override void OnDragOver()
	{
		RedirectDragOver();
	}

	protected override void OnDragOut()
	{
	}

	private void RedirectDragOver()
	{
		GameObject gameObject = UICamera.currentTouch.current;
		if ((bool)gameObject && gameObject != UICamera.currentTouch.pressed)
		{
			UIButton component = UICamera.currentTouch.pressed.GetComponent<UIButton>();
			if ((bool)component)
			{
				component.SetState(State.Normal, false);
			}
			UICamera.currentTouch.pressed.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
			UICamera.currentTouch.pressed = UICamera.currentTouch.current;
			component = UICamera.currentTouch.pressed.GetComponent<UIButton>();
			if ((bool)component)
			{
				component.SetState(State.Pressed, false);
			}
			UICamera.currentTouch.pressed.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
		}
	}
}
