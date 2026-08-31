using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePullDownBehaviour : MonoBehaviour
{
	public const string pullDownButtonSpriteName = "icon_dropup";

	public const string pullUpButtonSpriteName = "icon_dropdown";

	public float heightoffset = 10f;

	public UIGrid collectableGrid;

	public UIGrid categoryGrid;

	public UISprite categoryButton;

	public UISprite categoryBackground;

	public TweenScale categoryBackgroundTween;

	public float tweenscaleDelay;

	public UISprite pullDownButtonSprite;

	private bool toogle;

	private bool LastButton;

	private float worldToPixels;

	private void Start()
	{
		collectableGrid.Reposition();
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
		List<Transform> childList2 = collectableGrid.GetChildList();
		int count = childList2.Count;
		float cellHeight = collectableGrid.cellHeight;
		float num = categoryButton.height;
		pullDownButtonSprite.spriteName = "icon_dropdown";
		categoryBackground.height = categoryButton.height + Convert.ToInt32(heightoffset * 2f + (float)count * cellHeight);
		collectableGrid.transform.GetComponent<TweenScale>().delay = tweenscaleDelay;
		categoryBackgroundTween.delay = 0f;
		DisableButtons();
		collectableGrid.transform.GetComponent<TweenScale>().PlayForward();
		categoryBackgroundTween.PlayForward();
		collectableGrid.repositionNow = true;
		int index = categoryGrid.GetIndex(base.transform);
		for (int i = 0; i < childList.Count; i++)
		{
			if (i != index)
			{
				childList[i].GetComponent<CollectablePullDownBehaviour>().HidePullDown(true);
			}
		}
		float num2 = heightoffset * 2f + (float)count * cellHeight;
		DisableButtons();
		for (int j = index + 1; j < childList.Count; j++)
		{
			Vector3 localPosition = childList[j].localPosition;
			TweenPosition tweenPosition = TweenPosition.Begin(pos: new Vector3(localPosition.x, localPosition.y - num2, localPosition.z), go: childList[j].gameObject, duration: 0.5f);
			tweenPosition.AddOnFinished(EnableButtons);
		}
		toogle = true;
	}

	public void HidePullDown(bool inmediate = false)
	{
		if (toogle)
		{
			List<Transform> childList = categoryGrid.GetChildList();
			List<Transform> childList2 = collectableGrid.GetChildList();
			int count = childList2.Count;
			float cellHeight = collectableGrid.cellHeight;
			float num = categoryButton.height;
			pullDownButtonSprite.spriteName = "icon_dropup";
			collectableGrid.transform.GetComponent<TweenScale>().delay = 0f;
			categoryBackgroundTween.delay = tweenscaleDelay;
			DisableButtons();
			collectableGrid.transform.GetComponent<TweenScale>().PlayReverse();
			categoryBackgroundTween.PlayReverse();
			int index = categoryGrid.GetIndex(base.transform);
			float num2 = heightoffset * 2f + (float)count * cellHeight;
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
