using UnityEngine;

public class UIRewardRowInApp : MonoBehaviour
{
	public enum Item
	{
		miniPack = 0,
		smallPack = 1,
		mediumPack = 2,
		bigPack = 3,
		megaPack = 4,
		exclusivePack = 5
	}

	[SerializeField]
	private UILabel taskTitle;

	[SerializeField]
	private UILabel reward;

	public Item item;

	private void Start()
	{
		RefreshGUI();
	}

	private void RefreshGUI()
	{
		taskTitle.text = Localization.Get(item.ToString());
		switch (item)
		{
		case Item.miniPack:
			reward.text = 1000m.ToString();
			break;
		case Item.smallPack:
			reward.text = 5500m.ToString();
			break;
		case Item.mediumPack:
			reward.text = 11500m.ToString();
			break;
		case Item.bigPack:
			reward.text = 24000m.ToString();
			break;
		case Item.megaPack:
			reward.text = 62500m.ToString();
			break;
		case Item.exclusivePack:
			reward.text = 200000m.ToString();
			break;
		}
		UIAutoResizeLabel component = reward.GetComponent<UIAutoResizeLabel>();
		if ((bool)component)
		{
			component.RecenterInParent();
		}
	}
}
