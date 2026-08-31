using UnityEngine;

public class RecommendedOffer : MonoBehaviour
{
	public BillingManager.InAppProduct recommendedOffer;

	[SerializeField]
	private UILabel lbRecommendedOfferCredits;

	[SerializeField]
	private UILabel lbRecommendedOfferPrice;

	[SerializeField]
	private BillingButton billingButton;

	private void OnEnable()
	{
		UpdateOffer();
	}

	private void UpdateOffer()
	{
		string productId = MySingleton<Manager>.Instance.billingManager.GetProductId(recommendedOffer);
		billingButton.product = recommendedOffer;
		lbRecommendedOfferCredits.text = MySingleton<Manager>.Instance.billingManager.GetCreditsAmount(productId).ToString();
		lbRecommendedOfferPrice.text = Unibiller.GetPurchasableItemById(productId).localizedPriceString;
		UIAutoResizeLabel component = lbRecommendedOfferCredits.GetComponent<UIAutoResizeLabel>();
		if ((bool)component)
		{
			component.RecenterInParent();
		}
	}
}
