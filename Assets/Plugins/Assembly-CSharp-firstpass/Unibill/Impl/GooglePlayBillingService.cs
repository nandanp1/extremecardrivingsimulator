using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Uniject;

namespace Unibill.Impl
{
	public class GooglePlayBillingService : IBillingService
	{
		private string publicKey;

		private IRawGooglePlayInterface rawInterface;

		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private UnibillConfiguration db;

		private ILogger logger;

		private RSACryptoServiceProvider cryptoProvider;

		public GooglePlayBillingService(IRawGooglePlayInterface rawInterface, UnibillConfiguration config, ProductIdRemapper remapper, ILogger logger)
		{
			this.rawInterface = rawInterface;
			publicKey = config.GooglePlayPublicKey;
			this.remapper = remapper;
			db = config;
			this.logger = logger;
			cryptoProvider = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(publicKey);
		}

		public void initialise(IBillingServiceCallback callback)
		{
			this.callback = callback;
			if (publicKey == null || publicKey.Equals("[Your key]"))
			{
				callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED, publicKey);
				callback.onSetupComplete(false);
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("publicKey", publicKey);
			List<string> list = new List<string>();
			List<object> list2 = new List<object>();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				string text = remapper.mapItemIdToPlatformSpecificId(allPurchasableItem);
				list.Add(text);
				dictionary2.Add("productId", text);
				dictionary2.Add("consumable", allPurchasableItem.PurchaseType == PurchaseType.Consumable);
				list2.Add(dictionary2);
			}
			dictionary.Add("products", list2);
			string text2 = dictionary.toJson();
			rawInterface.initialise(this, text2, list.ToArray());
		}

		public void restoreTransactions()
		{
			rawInterface.restoreTransactions();
		}

		public void purchase(string item, string developerPayload)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["productId"] = item;
			dictionary["developerPayload"] = developerPayload;
			rawInterface.purchase(MiniJSON.jsonEncode(dictionary));
		}

		public void onBillingNotSupported()
		{
			callback.logError(UnibillError.GOOGLEPLAY_BILLING_UNAVAILABLE);
			callback.onSetupComplete(false);
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			string receipt = (string)dictionary["signature"];
			string text = (string)dictionary["productId"];
			string transactionIdentifier = (string)dictionary["transactionId"];
			if (!verifyReceipt(receipt))
			{
				logger.Log("Signature is invalid!");
				callback.onPurchaseFailedEvent(new PurchaseFailureDescription(text, PurchaseFailureReason.SIGNATURE_INVALID, "mono"));
			}
			else
			{
				callback.onPurchaseSucceeded(text, receipt, transactionIdentifier);
			}
		}

		public void onPurchaseRefunded(string item)
		{
			callback.onPurchaseRefundedEvent(item);
		}

		public void onPurchaseFailed(string json)
		{
			callback.onPurchaseFailedEvent(new PurchaseFailureDescription(json));
		}

		public void onTransactionsRestored(string success)
		{
			if (bool.Parse(success))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onInvalidPublicKey(string key)
		{
			callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_INVALID, key);
			callback.onSetupComplete(false);
		}

		public void onProductListReceived(string json)
		{
			logger.Log("Received product list, completing setup...");
			callback.onSetupComplete(Util.DeserialiseProductList(json));
		}

		public void finishTransaction(PurchasableItem item, string transactionId)
		{
			if (item.PurchaseType == PurchaseType.Consumable)
			{
				rawInterface.finishTransaction(transactionId);
			}
		}

		private bool verifyReceipt(string receipt)
		{
			try
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(receipt);
				if (dictionary == null)
				{
					return false;
				}
				string text = dictionary.getString("signature", string.Empty);
				string text2 = dictionary.getString("json", string.Empty);
				if (text == null || text2 == null)
				{
					return false;
				}
				byte[] signature = Convert.FromBase64String(text);
				SHA1Managed halg = new SHA1Managed();
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				return cryptoProvider.VerifyData(bytes, halg, signature);
			}
			catch (Exception ex)
			{
				logger.Log("Validation exception");
				logger.Log(ex.Message);
				logger.Log(ex.StackTrace.ToString());
				return false;
			}
		}
	}
}
