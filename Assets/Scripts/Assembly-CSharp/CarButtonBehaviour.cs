using UnityEngine;

public class CarButtonBehaviour : MonoBehaviour
{
	public UISprite ButtonSprite;

	public void Init(string spritename, bool enabled = false)
	{
		ButtonSprite.spriteName = spritename;
		if (enabled)
		{
			EnableButton();
		}
		else
		{
			DisableButton();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		ButtonSprite.MakePixelPerfect();
	}

	public void ShowButton()
	{
		GetComponent<TweenScale>().PlayForward();
	}

	public void HideButton()
	{
		GetComponent<TweenScale>().PlayReverse();
	}

	public void EnableButton()
	{
		GetComponent<BoxCollider>().enabled = false;
	}

	public void DisableButton()
	{
		GetComponent<BoxCollider>().enabled = true;
	}
}
