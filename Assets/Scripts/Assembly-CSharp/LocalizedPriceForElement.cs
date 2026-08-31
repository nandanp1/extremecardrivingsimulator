using UnityEngine;

public class LocalizedPriceForElement : MonoBehaviour
{
	public GameObject container;

	public BillingManager.InAppProduct product;

	private void Start()
	{
		LocalizeLabel();
	}

	public void LocalizeLabel()
	{
		UILabel component = GetComponent<UILabel>();
		if (MySingleton<Manager>.Instance != null && MySingleton<Manager>.Instance.billingManager != null)
		{
			PurchasableItem unibillItem = MySingleton<Manager>.Instance.billingManager.GetUnibillItem(product);
			if (container == null)
			{
				container = base.gameObject;
			}
			if (unibillItem != null && component != null && container != null)
			{
				component.text = unibillItem.localizedPriceString.ToString();
				container.SetActive(true);
			}
			else
			{
				container.SetActive(false);
			}
		}
	}
}
