using UnityEngine;

public class UIDailyRewardSpriteLogic : MonoBehaviour
{
	public UISprite spToday;

	public UISprite spDone;

	public UISprite spTomorrow;

	public Color doneColor = Color.green;

	public void ShowToday()
	{
		HideAll();
		NGUITools.SetActive(spToday.gameObject, true);
		GetComponent<TweenColor>().enabled = true;
	}

	public void ShowDone()
	{
		HideAll();
		NGUITools.SetActive(spDone.gameObject, true);
		GetComponent<UISprite>().color = doneColor;
	}

	public void ShowTomorrow()
	{
		HideAll();
		NGUITools.SetActive(spTomorrow.gameObject, true);
	}

	public void HideAll()
	{
		NGUITools.SetActive(spToday.gameObject, false);
		NGUITools.SetActive(spDone.gameObject, false);
		NGUITools.SetActive(spTomorrow.gameObject, false);
		GetComponent<UISprite>().color = Color.white;
		GetComponent<TweenColor>().enabled = false;
	}
}
