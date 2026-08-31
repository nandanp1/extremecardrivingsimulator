using Unibill;
using Unibill.Demo;
using UnityEngine;

[AddComponentMenu("Unibill/UnibillDemo")]
public class UnibillDemo : MonoBehaviour
{
	private const string DLC_ID = "episode2";

	private ComboBox box;

	private GUIContent[] comboBoxList;

	private GUIStyle listStyle;

	private int selectedItemIndex;

	private PurchasableItem[] items;

	public Font font;

	private void Start()
	{
		if (Resources.Load("unibillInventory.json") == null)
		{
			Debug.LogError("You must define your purchasable inventory within the inventory editor!");
			base.gameObject.SetActive(false);
			return;
		}
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onTransactionsRestored += onTransactionsRestored;
		Unibiller.onPurchaseFailed += onFailed;
		Unibiller.onPurchaseCompleteEvent += onPurchased;
		Unibiller.onPurchaseDeferred += onDeferred;
		Unibiller.Initialise();
		initCombobox();
	}

	private void onBillerReady(UnibillState state)
	{
		Debug.Log("onBillerReady:" + state);
		if (state == UnibillState.CRITICAL_ERROR)
		{
			return;
		}
		Debug.Log("Available items:");
		PurchasableItem[] allPurchasableItems = Unibiller.AllPurchasableItems;
		foreach (PurchasableItem purchasableItem in allPurchasableItems)
		{
			if (purchasableItem.AvailableToPurchase)
			{
				Debug.Log(string.Join(" - ", new string[5]
				{
					purchasableItem.localizedTitle,
					purchasableItem.localizedDescription,
					purchasableItem.isoCurrencySymbol,
					purchasableItem.priceInLocalCurrency.ToString(),
					purchasableItem.localizedPriceString
				}));
			}
		}
	}

	private void onTransactionsRestored(bool success)
	{
		Debug.Log("Transactions restored.");
	}

	private void onPurchased(PurchaseEvent e)
	{
		Debug.Log("Purchase OK: " + e.PurchasedItem.Id);
		Debug.Log("Receipt: " + e.Receipt);
		Debug.Log(string.Format("{0} has now been purchased {1} times.", e.PurchasedItem.name, Unibiller.GetPurchaseCount(e.PurchasedItem)));
	}

	private void onDeferred(PurchasableItem item)
	{
		Debug.Log("Purchase deferred blud: " + item.Id);
	}

	private void onFailed(PurchaseFailedEvent e)
	{
		Debug.Log("Purchase failed: " + e.PurchasedItem.Id);
		Debug.Log(e.Reason);
	}

	private void initCombobox()
	{
		box = new ComboBox();
		items = Unibiller.AllPurchasableItems;
		comboBoxList = new GUIContent[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			comboBoxList[i] = new GUIContent(string.Format("{0} - {1}", items[i].localizedTitle, items[i].localizedPriceString));
		}
		listStyle = new GUIStyle();
		listStyle.font = font;
		listStyle.normal.textColor = Color.white;
		GUIStyleState onHover = listStyle.onHover;
		Texture2D background = new Texture2D(2, 2);
		listStyle.hover.background = background;
		onHover.background = background;
		RectOffset padding = listStyle.padding;
		int num = 4;
		listStyle.padding.bottom = num;
		num = num;
		listStyle.padding.top = num;
		num = num;
		listStyle.padding.right = num;
		padding.left = num;
	}

	public void Update()
	{
		for (int i = 0; i < items.Length; i++)
		{
			comboBoxList[i] = new GUIContent(string.Format("{0} - {1} - {2}", items[i].name, items[i].localizedTitle, items[i].localizedPriceString));
		}
	}

	private void OnGUI()
	{
		selectedItemIndex = box.GetSelectedItemIndex();
		selectedItemIndex = box.List(new Rect(0f, 0f, Screen.width, (float)Screen.width / 20f), comboBoxList[selectedItemIndex].text, comboBoxList, listStyle);
		if (GUI.Button(new Rect(0f, (float)Screen.height - (float)Screen.width / 6f, (float)Screen.width / 2f, (float)Screen.width / 6f), "Buy"))
		{
			Unibiller.initiatePurchase(items[selectedItemIndex], string.Empty);
		}
		if (GUI.Button(new Rect((float)Screen.width / 2f, (float)Screen.height - (float)Screen.width / 6f, (float)Screen.width / 2f, (float)Screen.width / 6f), "Restore transactions"))
		{
			Unibiller.restoreTransactions();
		}
		int num = (int)((float)Screen.height - (float)(2 * Screen.width) / 6f) - 50;
		PurchasableItem[] allNonConsumablePurchasableItems = Unibiller.AllNonConsumablePurchasableItems;
		foreach (PurchasableItem purchasableItem in allNonConsumablePurchasableItems)
		{
			GUI.Label(new Rect(0f, num, 500f, 50f), purchasableItem.Id, listStyle);
			GUI.Label(new Rect((float)Screen.width - (float)Screen.width * 0.1f, num, 500f, 50f), Unibiller.GetPurchaseCount(purchasableItem).ToString(), listStyle);
			num -= 30;
		}
		string[] allCurrencies = Unibiller.AllCurrencies;
		foreach (string text in allCurrencies)
		{
			GUI.Label(new Rect(0f, num, 500f, 50f), text, listStyle);
			GUI.Label(new Rect((float)Screen.width - (float)Screen.width * 0.1f, num, 500f, 50f), Unibiller.GetCurrencyBalance(text).ToString(), listStyle);
			num -= 30;
		}
		PurchasableItem[] allSubscriptions = Unibiller.AllSubscriptions;
		foreach (PurchasableItem purchasableItem2 in allSubscriptions)
		{
			GUI.Label(new Rect(0f, num, 500f, 50f), purchasableItem2.localizedTitle, listStyle);
			GUI.Label(new Rect((float)Screen.width - (float)Screen.width * 0.1f, num, 500f, 50f), Unibiller.GetPurchaseCount(purchasableItem2).ToString(), listStyle);
			num -= 30;
		}
		GUI.Label(new Rect(0f, num - 10, 500f, 50f), "Item", listStyle);
		GUI.Label(new Rect((float)Screen.width - (float)Screen.width * 0.2f, num - 10, 500f, 50f), "Count", listStyle);
	}
}
