using System;
using System.Collections.Generic;
using UnityEngine;

public class ChallengePullDownBehaviour : MonoBehaviour
{
	public const string pullDownButtonSpriteName = "icon_dropup";

	public const string pullUpButtonSpriteName = "icon_dropdown";

	public float carButtonMaxHeight = 130f;

	public float heightoffset = 10f;

	public UIGrid challengeGrid;

	public UIGrid categoryGrid;

	public UISprite categoryButton;

	public UISprite categoryBackground;

	public TweenScale categoryBackgroundTween;

	public float tweenscaleDelay;

	public GameObject unlockCarSection;

	public UISprite pullDownButtonSprite;

	public UISprite buttonCarReward;

	private bool toogle;

	private bool LastButton;

	private float worldToPixels;

	private int buttonCarOffset;

	private void Start()
	{
		challengeGrid.Reposition();
		worldToPixels = (float)Screen.height / 2f / Camera.main.orthographicSize;
	}

	private void Update()
	{
	}

	public void OnButtonPullDownClicked()
	{
		if (!toogle)
		{
			ShowPullDown();
		}
		else
		{
			HidePullDown();
		}
	}

	public void ShowPullDown()
	{
		List<Transform> childList = categoryGrid.GetChildList();
		List<Transform> childList2 = challengeGrid.GetChildList();
		int count = childList2.Count;
		float cellHeight = challengeGrid.cellHeight;
		float num = categoryButton.height;
		bool flag = false;
		buttonCarReward.MakePixelPerfect();
		pullDownButtonSprite.spriteName = "icon_dropdown";
		if (base.transform.GetComponent<ChallengeCategoryBehaviour>() != null && base.transform.GetComponent<ChallengeCategoryBehaviour>().carButtonEnable)
		{
			unlockCarSection.SetActive(true);
			buttonCarOffset = Convert.ToInt32(carButtonMaxHeight);
		}
		else
		{
			unlockCarSection.SetActive(false);
			buttonCarOffset = 0;
		}
		categoryBackground.height = categoryButton.height + Convert.ToInt32(heightoffset * 2f + (float)count * cellHeight) + buttonCarOffset;
		challengeGrid.transform.GetComponent<TweenScale>().delay = tweenscaleDelay;
		categoryBackgroundTween.delay = 0f;
		DisableButtons();
		challengeGrid.transform.GetComponent<TweenScale>().PlayForward();
		categoryBackgroundTween.PlayForward();
		challengeGrid.repositionNow = true;
		int index = categoryGrid.GetIndex(base.transform);
		for (int i = 0; i < childList.Count; i++)
		{
			if (i != index)
			{
				childList[i].GetComponent<ChallengePullDownBehaviour>().HidePullDown(true);
			}
		}
		float num2 = heightoffset * 2f + (float)count * cellHeight + (float)buttonCarOffset;
		DisableButtons();
		for (int j = index + 1; j < childList.Count; j++)
		{
			Vector3 localPosition = childList[j].localPosition;
			TweenPosition tweenPosition = TweenPosition.Begin(pos: new Vector3(localPosition.x, localPosition.y - num2, localPosition.z), go: childList[j].gameObject, duration: 0.5f);
			tweenPosition.AddOnFinished(EnableButtons);
		}
		toogle = true;
	}

	public void ActiveUnlockedCarSection(bool activate)
	{
		unlockCarSection.SetActive(activate);
	}

	public void HidePullDown(bool inmediate = false)
	{
		if (toogle)
		{
			List<Transform> childList = categoryGrid.GetChildList();
			List<Transform> childList2 = challengeGrid.GetChildList();
			int count = childList2.Count;
			float cellHeight = challengeGrid.cellHeight;
			float num = categoryButton.height;
			buttonCarReward.MakePixelPerfect();
			pullDownButtonSprite.spriteName = "icon_dropup";
			if (base.transform.GetComponent<ChallengeCategoryBehaviour>() != null && base.transform.GetComponent<ChallengeCategoryBehaviour>().carButtonEnable)
			{
				unlockCarSection.SetActive(true);
				buttonCarOffset = Convert.ToInt32(carButtonMaxHeight);
			}
			else
			{
				unlockCarSection.SetActive(false);
				buttonCarOffset = 0;
			}
			challengeGrid.transform.GetComponent<TweenScale>().delay = 0f;
			categoryBackgroundTween.delay = tweenscaleDelay;
			DisableButtons();
			challengeGrid.transform.GetComponent<TweenScale>().PlayReverse();
			categoryBackgroundTween.PlayReverse();
			int index = categoryGrid.GetIndex(base.transform);
			float num2 = heightoffset * 2f + (float)count * cellHeight + (float)buttonCarOffset;
			if (!inmediate)
			{
				DisableButtons();
			}
			for (int i = index + 1; i < childList.Count; i++)
			{
				Vector3 localPosition = childList[i].localPosition;
				Vector3 vector = new Vector3(localPosition.x, localPosition.y + num2, localPosition.z);
				if (!inmediate)
				{
					TweenPosition tweenPosition = TweenPosition.Begin(childList[i].gameObject, 0.5f, vector);
					tweenPosition.AddOnFinished(EnableButtons);
				}
				else
				{
					childList[i].transform.localPosition = vector;
				}
			}
		}
		toogle = false;
	}

	public void DisableButtons()
	{
		List<Transform> childList = categoryGrid.GetChildList();
		foreach (Transform item in childList)
		{
			item.GetComponent<BoxCollider2D>().enabled = false;
		}
	}

	public void EnableButtons()
	{
		List<Transform> childList = categoryGrid.GetChildList();
		foreach (Transform item in childList)
		{
			item.GetComponent<BoxCollider2D>().enabled = true;
		}
	}
}
