using System.Collections.Generic;
using Uniject;
using UnityEngine;
using ILogger = Uniject.ILogger;

namespace Unibill.Impl
{
	public class UnibillConfiguration
	{
		private ILogger logger;

		public List<PurchasableItem> inventory = new List<PurchasableItem>();

		public List<VirtualCurrency> currencies = new List<VirtualCurrency>();

		public BillingPlatform CurrentPlatform { get; set; }

		public string iOSSKU { get; set; }

		public string macAppStoreSKU { get; set; }

		public BillingPlatform AndroidBillingPlatform { get; set; }

		public string GooglePlayPublicKey { get; set; }

		public bool AmazonSandboxEnabled { get; set; }

		public bool WP8SandboxEnabled { get; set; }

		public bool UseHostedConfig { get; set; }

		public string HostedConfigUrl { get; set; }

		public string UnibillAnalyticsAppId { get; set; }

		public string UnibillAnalyticsAppSecret { get; set; }

		public bool UseWin8_1Sandbox { get; set; }

		public SamsungAppsMode SamsungAppsMode { get; set; }

		public string SamsungItemGroupId { get; set; }

		public List<PurchasableItem> AllPurchasableItems
		{
			get
			{
				return new List<PurchasableItem>(inventory);
			}
		}

		public List<PurchasableItem> AllNonConsumablePurchasableItems
		{
			get
			{
				return inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.NonConsumable);
			}
		}

		public List<PurchasableItem> AllConsumablePurchasableItems
		{
			get
			{
				return inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Consumable);
			}
		}

		public List<PurchasableItem> AllSubscriptions
		{
			get
			{
				return inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Subscription);
			}
		}

		public List<PurchasableItem> AllNonSubscriptionPurchasableItems
		{
			get
			{
				return inventory.FindAll((PurchasableItem x) => x.PurchaseType != PurchaseType.Subscription);
			}
		}

		public UnibillConfiguration(string json, RuntimePlatform runtimePlatform, ILogger logger, List<ProductDefinition> runtimeProducts = null)
		{
			this.logger = logger;
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			iOSSKU = dictionary.getString("iOSSKU", string.Empty);
			macAppStoreSKU = dictionary.getString("macAppStoreSKU", string.Empty);
			AndroidBillingPlatform = dictionary.getEnum<BillingPlatform>("androidBillingPlatform");
			GooglePlayPublicKey = dictionary.get<string>("GooglePlayPublicKey");
			AmazonSandboxEnabled = dictionary.getBool("useAmazonSandbox");
			WP8SandboxEnabled = dictionary.getBool("UseWP8MockingFramework");
			UseHostedConfig = dictionary.getBool("useHostedConfig");
			HostedConfigUrl = dictionary.get<string>("hostedConfigUrl");
			UseWin8_1Sandbox = dictionary.getBool("UseWin8_1Sandbox");
			UnibillAnalyticsAppId = dictionary.getString("unibillAnalyticsAppId", string.Empty);
			UnibillAnalyticsAppSecret = dictionary.getString("unibillAnalyticsAppSecret", string.Empty);
			SamsungAppsMode = dictionary.getEnum<SamsungAppsMode>("samsungAppsMode");
			SamsungItemGroupId = dictionary.getString("samsungAppsItemGroupId", string.Empty);
			switch (runtimePlatform)
			{
			case RuntimePlatform.Android:
				CurrentPlatform = AndroidBillingPlatform;
				break;
			case RuntimePlatform.IPhonePlayer:
				CurrentPlatform = BillingPlatform.AppleAppStore;
				break;
			case RuntimePlatform.OSXPlayer:
				CurrentPlatform = BillingPlatform.MacAppStore;
				break;
			case RuntimePlatform.WSAPlayerX86:
			case RuntimePlatform.WSAPlayerX64:
			case RuntimePlatform.WSAPlayerARM:
				CurrentPlatform = BillingPlatform.Windows8_1;
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				CurrentPlatform = BillingPlatform.UnityEditor;
				break;
			default:
				CurrentPlatform = BillingPlatform.UnityEditor;
				break;
			}
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["purchasableItems"];
			foreach (KeyValuePair<string, object> item3 in dictionary2)
			{
				PurchasableItem item = new PurchasableItem(item3.Key, (Dictionary<string, object>)item3.Value, CurrentPlatform);
				inventory.Add(item);
			}
			if (runtimeProducts != null)
			{
				foreach (ProductDefinition runtimeProduct in runtimeProducts)
				{
					Dictionary<string, object> hash = new Dictionary<string, object> { 
					{
						"@purchaseType",
						runtimeProduct.Type.ToString()
					} };
					PurchasableItem item2 = new PurchasableItem(runtimeProduct.PlatformSpecificId, hash, CurrentPlatform);
					if (!inventory.Exists((PurchasableItem x) => x.Id == item2.Id))
					{
						inventory.Add(item2);
					}
				}
			}
			loadCurrencies(dictionary);
		}

		private void loadCurrencies(Dictionary<string, object> root)
		{
			currencies = new List<VirtualCurrency>();
			Dictionary<string, object> hash = root.getHash("currencies");
			if (hash == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> item in hash)
			{
				Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
				foreach (KeyValuePair<string, object> item2 in (Dictionary<string, object>)item.Value)
				{
					dictionary.Add(item2.Key, decimal.Parse(item2.Value.ToString()));
				}
				currencies.Add(new VirtualCurrency(item.Key, dictionary));
			}
		}

		public PurchasableItem AddItem()
		{
			PurchasableItem purchasableItem = new PurchasableItem();
			inventory.Add(purchasableItem);
			return purchasableItem;
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("iOSSKU", iOSSKU);
			dictionary.Add("macAppStoreSKU", macAppStoreSKU);
			dictionary.Add("androidBillingPlatform", AndroidBillingPlatform.ToString());
			dictionary.Add("GooglePlayPublicKey", GooglePlayPublicKey);
			dictionary.Add("useAmazonSandbox", AmazonSandboxEnabled);
			dictionary.Add("UseWP8MockingFramework", WP8SandboxEnabled);
			dictionary.Add("useHostedConfig", UseHostedConfig);
			dictionary.Add("hostedConfigUrl", HostedConfigUrl);
			dictionary.Add("UseWin8_1Sandbox", UseWin8_1Sandbox);
			dictionary.Add("unibillAnalyticsAppId", UnibillAnalyticsAppId);
			dictionary.Add("unibillAnalyticsAppSecret", UnibillAnalyticsAppSecret);
			dictionary.Add("samsungAppsMode", SamsungAppsMode.ToString());
			dictionary.Add("samsungAppsItemGroupId", SamsungItemGroupId);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			foreach (PurchasableItem item in inventory)
			{
				dictionary2.Add(item.Id, item.Serialize());
			}
			dictionary.Add("purchasableItems", dictionary2);
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			foreach (VirtualCurrency currency in currencies)
			{
				Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
				foreach (KeyValuePair<string, decimal> mapping in currency.mappings)
				{
					dictionary4.Add(mapping.Key, mapping.Value);
				}
				dictionary3.Add(currency.currencyId, dictionary4);
			}
			dictionary.Add("currencies", dictionary3);
			return dictionary;
		}

		public PurchasableItem getItemById(string id)
		{
			PurchasableItem purchasableItem = inventory.Find((PurchasableItem x) => x.Id == id);
			if (purchasableItem == null)
			{
				logger.LogWarning("Unknown purchasable item:{0}. Check your Unibill inventory configuration.", id);
			}
			return purchasableItem;
		}

		public VirtualCurrency getCurrency(string id)
		{
			return currencies.Find((VirtualCurrency x) => x.currencyId == id);
		}

		private bool tryGetBoolean(string name, Dictionary<string, object> root)
		{
			if (root.ContainsKey(name))
			{
				return bool.Parse(root[name].ToString());
			}
			return false;
		}
	}
}
