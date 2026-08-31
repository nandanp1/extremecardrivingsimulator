using System.Collections.Generic;

public class AndroidInventory
{
	private Dictionary<string, GooglePurchaseTemplate> _purchases;

	private Dictionary<string, GoogleProductTemplate> _products;

	public List<GooglePurchaseTemplate> purchases
	{
		get
		{
			return new List<GooglePurchaseTemplate>(_purchases.Values);
		}
	}

	public List<GoogleProductTemplate> products
	{
		get
		{
			return new List<GoogleProductTemplate>(_products.Values);
		}
	}

	public AndroidInventory()
	{
		_purchases = new Dictionary<string, GooglePurchaseTemplate>();
		_products = new Dictionary<string, GoogleProductTemplate>();
	}

	public void addProduct(GoogleProductTemplate product)
	{
		if (_products.ContainsKey(product.SKU))
		{
			_products[product.SKU] = product;
		}
		else
		{
			_products.Add(product.SKU, product);
		}
	}

	public void addPurchase(GooglePurchaseTemplate purchase)
	{
		if (_purchases.ContainsKey(purchase.SKU))
		{
			_purchases[purchase.SKU] = purchase;
		}
		else
		{
			_purchases.Add(purchase.SKU, purchase);
		}
	}

	public void removePurchase(GooglePurchaseTemplate purchase)
	{
		if (_purchases.ContainsKey(purchase.SKU))
		{
			_purchases.Remove(purchase.SKU);
		}
	}

	public bool IsProductPurchased(string SKU)
	{
		if (_purchases.ContainsKey(SKU))
		{
			return true;
		}
		return false;
	}

	public GoogleProductTemplate GetProductDetails(string SKU)
	{
		if (_products.ContainsKey(SKU))
		{
			return _products[SKU];
		}
		return null;
	}

	public GooglePurchaseTemplate GetPurchaseDetails(string SKU)
	{
		if (_purchases.ContainsKey(SKU))
		{
			return _purchases[SKU];
		}
		return null;
	}
}
