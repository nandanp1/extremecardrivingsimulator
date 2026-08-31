using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CrossPromoNEW : MonoBehaviour
{
	[Serializable]
	public class CrossPromoNEWData
	{
		public string icon;

		public string link;

		public string title;

		public bool promoted;

		public CrossPromoNEWData(Dictionary<string, object> data)
		{
			icon = data["icon"].ToString();
			link = data["link"].ToString();
			title = data["title"].ToString();
			if (data.ContainsKey("promoted"))
			{
				promoted = data["promoted"].ToString() == "True";
			}
		}

		public CrossPromoNEWData(string icon, string link, string title, bool promoted)
		{
			this.icon = icon;
			this.link = link;
			this.title = title;
			this.promoted = promoted;
		}
	}

	public CrossPromoNEWData crossPromoData;

	public bool isMenuCrosspromo;

	public bool isPausePromotedCrosspromo;

	public bool isMenuPopup;

	private string link;

	[Header("GUI References")]
	public UILabel lbTitle;

	public UISprite spIcon;

	public UISprite spPromoted;

	public void Init(Dictionary<string, object> data)
	{
		crossPromoData = new CrossPromoNEWData(data);
		UpdateGUI();
	}

	public void Init(CrossPromoNEWData crossPromoData)
	{
		this.crossPromoData = crossPromoData;
		UpdateGUI();
	}

	private void UpdateGUI()
	{
		spIcon.spriteName = crossPromoData.icon;
		lbTitle.text = crossPromoData.title;
		link = crossPromoData.link;
		NGUITools.SetActive(spPromoted.gameObject, crossPromoData.promoted);
	}

	public void OnClick()
	{
		if (isMenuCrosspromo)
		{
			if (MySingleton<Manager>.Instance.remoteConfiguration.menuClickedTimes >= MySingleton<Manager>.Instance.remoteConfiguration.menuMaxClickedTimes - 1)
			{
				if (MySingleton<Manager>.Instance.remoteConfiguration.menuMaxClickedTimes >= 0)
				{
					MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuClicked = true;
					Close();
				}
			}
			else
			{
				MySingleton<Manager>.Instance.remoteConfiguration.menuClickedTimes++;
			}
		}
		else if (isPausePromotedCrosspromo)
		{
			if (MySingleton<Manager>.Instance.remoteConfiguration.pauseClickedTimes >= MySingleton<Manager>.Instance.remoteConfiguration.pauseMaxClickedTimes - 1)
			{
				if (MySingleton<Manager>.Instance.remoteConfiguration.pauseMaxClickedTimes >= 0)
				{
					MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPauseClicked = true;
					Close();
				}
			}
			else
			{
				MySingleton<Manager>.Instance.remoteConfiguration.pauseClickedTimes++;
			}
		}
		else if (isMenuPopup)
		{
			if (MySingleton<Manager>.Instance.remoteConfiguration.menuPopupClickedTimes >= MySingleton<Manager>.Instance.remoteConfiguration.menuPopupMaxClickedTimes - 1)
			{
				if (MySingleton<Manager>.Instance.remoteConfiguration.menuPopupMaxClickedTimes >= 0)
				{
					MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuPopupClicked = true;
				}
			}
			else
			{
				MySingleton<Manager>.Instance.remoteConfiguration.menuPopupClickedTimes++;
			}
			Invoke("Close", 0.25f);
		}
		Application.OpenURL(link);
	}

	private void Close()
	{
		base.gameObject.SetActive(false);
	}
}
