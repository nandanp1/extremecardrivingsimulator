using System;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;

namespace Unibill
{
	public class Biller : IBillingServiceCallback
	{
		private TransactionDatabase transactionDatabase;

		private ILogger logger;

		private HelpCentre help;

		public ProductIdRemapper remapper;

		private CurrencyManager currencyManager;

		private bool restoreInProgress;

		public UnibillConfiguration InventoryDatabase { get; private set; }

		public IBillingService billingSubsystem { get; private set; }

		public BillerState State { get; private set; }

		public List<UnibillError> Errors { get; private set; }

		public bool Ready
		{
			get
			{
				return State == BillerState.INITIALISED || State == BillerState.INITIALISED_WITH_ERROR;
			}
		}

		public string[] CurrencyIdentifiers
		{
			get
			{
				return currencyManager.Currencies;
			}
		}

		public event Action<bool> onBillerReady;

		public event Action<PurchaseEvent> onPurchaseComplete;

		public event Action<bool> onTransactionRestoreBegin;

		public event Action<bool> onTransactionsRestored;

		public event Action<PurchasableItem> onPurchaseCancelled;

		public event Action<PurchasableItem> onPurchaseRefunded;

		public event Action<PurchaseFailedEvent> onPurchaseFailed;

		public event Action<PurchasableItem> onPurchaseDeferred;

		public Biller(UnibillConfiguration config, TransactionDatabase tDb, IBillingService billingSubsystem, ILogger logger, HelpCentre help, ProductIdRemapper remapper, CurrencyManager currencyManager)
		{
			InventoryDatabase = config;
			transactionDatabase = tDb;
			this.billingSubsystem = billingSubsystem;
			this.logger = logger;
			logger.prefix = "UnibillBiller";
			this.help = help;
			Errors = new List<UnibillError>();
			this.remapper = remapper;
			this.currencyManager = currencyManager;
			State = BillerState.NOT_INITIALISED;
		}

		public void Initialise()
		{
			if (State == BillerState.INITIALISING)
			{
				logError(UnibillError.BILLER_NOT_READY);
			}
			else if (InventoryDatabase.AllPurchasableItems.Count == 0)
			{
				logError(UnibillError.UNIBILL_NO_PRODUCTS_DEFINED);
				onSetupComplete(false);
			}
			else
			{
				State = BillerState.INITIALISING;
				billingSubsystem.initialise(this);
			}
		}

		public int getPurchaseHistory(PurchasableItem item)
		{
			return transactionDatabase.getPurchaseHistory(item);
		}

		public int getPurchaseHistory(string purchasableId)
		{
			PurchasableItem itemById = InventoryDatabase.getItemById(purchasableId);
			if (itemById == null)
			{
				return -1;
			}
			return getPurchaseHistory(itemById);
		}

		public decimal getCurrencyBalance(string identifier)
		{
			return currencyManager.GetCurrencyBalance(identifier);
		}

		public void creditCurrencyBalance(string identifier, decimal amount)
		{
			currencyManager.CreditBalance(identifier, amount);
		}

		public bool debitCurrencyBalance(string identifier, decimal amount)
		{
			return currencyManager.DebitBalance(identifier, amount);
		}

		public void purchase(PurchasableItem item, string developerPayload = "")
		{
			if (State == BillerState.INITIALISING)
			{
				logError(UnibillError.BILLER_NOT_READY);
				onPurchaseFailedEvent(item, PurchaseFailureReason.BILLER_NOT_READY);
			}
			else if (State == BillerState.INITIALISED_WITH_CRITICAL_ERROR)
			{
				logError(UnibillError.UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR);
				onPurchaseFailedEvent(item, PurchaseFailureReason.BILLING_UNAVAILABLE);
			}
			else if (item == null)
			{
				logger.LogError("Trying to purchase null PurchasableItem");
			}
			else if (!item.AvailableToPurchase)
			{
				logError(UnibillError.UNIBILL_MISSING_PRODUCT, item.Id);
			}
			else if (item.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory(item) > 0)
			{
				logError(UnibillError.UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE);
				onPurchaseFailedEvent(item, PurchaseFailureReason.CANNOT_REPURCHASE_NON_CONSUMABLE);
			}
			else
			{
				billingSubsystem.purchase(remapper.mapItemIdToPlatformSpecificId(item), developerPayload);
				logger.Log("purchase({0})", item.Id);
			}
		}

		public void purchase(string purchasableId, string developerPayload = "")
		{
			PurchasableItem itemById = InventoryDatabase.getItemById(purchasableId);
			if (itemById == null)
			{
				logger.LogWarning("Unable to purchase unknown item with id: {0}", purchasableId);
			}
			purchase(itemById, developerPayload);
		}

		public void restoreTransactions()
		{
			logger.Log("restoreTransactions()");
			if (!Ready)
			{
				logError(UnibillError.BILLER_NOT_READY);
				return;
			}
			restoreInProgress = true;
			if (onTransactionRestoreBegin != null)
			{
				onTransactionRestoreBegin(true);
			}
			billingSubsystem.restoreTransactions();
		}

		public void onPurchaseSucceeded(string id, string receipt, string transactionId)
		{
			onPurchaseSucceeded(id, !restoreInProgress, receipt, transactionId);
		}

		private void onPurchaseSucceeded(string id, bool isNewPurchase, string receipt, string transactionId)
		{
			if (!verifyPlatformId(id))
			{
				billingSubsystem.finishTransaction(null, transactionId);
				return;
			}
			if (receipt != null)
			{
				onPurchaseReceiptRetrieved(id, receipt);
			}
			PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
			if (purchasableItemFromPlatformSpecificId.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory(purchasableItemFromPlatformSpecificId) > 0)
			{
				logger.Log("Ignoring multi purchase of non consumable " + purchasableItemFromPlatformSpecificId.Id);
				billingSubsystem.finishTransaction(purchasableItemFromPlatformSpecificId, transactionId);
				return;
			}
			if (transactionDatabase.recordPurchase(purchasableItemFromPlatformSpecificId, transactionId))
			{
				currencyManager.OnPurchased(purchasableItemFromPlatformSpecificId.Id);
				if (onPurchaseComplete != null)
				{
					logger.Log("onPurchaseSucceeded {0} {1})", purchasableItemFromPlatformSpecificId.Id, transactionId);
					onPurchaseComplete(new PurchaseEvent(purchasableItemFromPlatformSpecificId, isNewPurchase, receipt, transactionId));
				}
			}
			billingSubsystem.finishTransaction(purchasableItemFromPlatformSpecificId, transactionId);
		}

		public void onSetupComplete(bool available)
		{
			logger.Log("onSetupComplete({0})", available);
			State = ((!available) ? BillerState.INITIALISED_WITH_CRITICAL_ERROR : ((Errors.Count <= 0) ? BillerState.INITIALISED : BillerState.INITIALISED_WITH_ERROR));
			if (onBillerReady != null)
			{
				onBillerReady(Ready);
			}
		}

		public void onPurchaseDeferredEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseDeferredEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (onPurchaseDeferred != null)
				{
					onPurchaseDeferred(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseRefundedEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseRefundedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				transactionDatabase.onRefunded(purchasableItemFromPlatformSpecificId);
				if (onPurchaseRefunded != null)
				{
					onPurchaseRefunded(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseFailedEvent(PurchaseFailureDescription description)
		{
			if (verifyPlatformId(description.ProductId))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(description.ProductId);
				logger.Log("onPurchaseFailedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (onPurchaseFailed != null)
				{
					onPurchaseFailedEvent(purchasableItemFromPlatformSpecificId, description.Reason);
				}
			}
		}

		public void onTransactionsRestoredSuccess()
		{
			logger.Log("onTransactionsRestoredSuccess()");
			restoreInProgress = false;
			if (onTransactionsRestored != null)
			{
				onTransactionsRestored(true);
			}
		}

		public void ClearPurchases()
		{
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				transactionDatabase.clearPurchases(allPurchasableItem);
			}
		}

		public void onTransactionsRestoredFail(string error)
		{
			logger.Log("onTransactionsRestoredFail({0})", error);
			restoreInProgress = false;
			onTransactionsRestored(false);
		}

		public bool isOwned(PurchasableItem item)
		{
			return getPurchaseHistory(item) > 0;
		}

		public void setAppReceipt(string receipt)
		{
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				if (getPurchaseHistory(allPurchasableItem) > 0)
				{
					allPurchasableItem.receipt = receipt;
				}
			}
		}

		public void onSetupComplete(List<ProductDescription> products)
		{
			foreach (ProductDescription product in products)
			{
				if (remapper.canMapProductSpecificId(product.PlatformSpecificID))
				{
					PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(product.PlatformSpecificID);
					purchasableItemFromPlatformSpecificId.AvailableToPurchase = true;
					purchasableItemFromPlatformSpecificId.localizedPriceString = product.Price;
					purchasableItemFromPlatformSpecificId.localizedTitle = product.Title;
					purchasableItemFromPlatformSpecificId.localizedDescription = product.Description;
					purchasableItemFromPlatformSpecificId.isoCurrencySymbol = product.ISOCurrencyCode;
					purchasableItemFromPlatformSpecificId.priceInLocalCurrency = product.PriceDecimal;
					purchasableItemFromPlatformSpecificId.receipt = product.Receipt;
					if (!string.IsNullOrEmpty(product.Receipt))
					{
						onPurchaseSucceeded(product.PlatformSpecificID, false, product.Receipt, product.TransactionID);
					}
				}
				else
				{
					logger.LogError("Warning: Unknown product identifier: {0}", product.PlatformSpecificID);
				}
			}
			bool available = false;
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				if (!allPurchasableItem.AvailableToPurchase)
				{
					logError(UnibillError.UNIBILL_MISSING_PRODUCT, allPurchasableItem.Id, allPurchasableItem.LocalId);
				}
				else
				{
					available = true;
				}
			}
			onSetupComplete(available);
		}

		public IAppleExtensions getAppleExtensions()
		{
			return billingSubsystem as IAppleExtensions;
		}

		public void logError(UnibillError error)
		{
			logError(error, new object[0]);
		}

		public void logError(UnibillError error, params object[] args)
		{
			Errors.Add(error);
			logger.LogError(help.getMessage(error), args);
		}

		public void onPurchaseReceiptRetrieved(string platformSpecificItemId, string receipt)
		{
			if (remapper.canMapProductSpecificId(platformSpecificItemId))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(platformSpecificItemId);
				purchasableItemFromPlatformSpecificId.receipt = receipt;
			}
		}

		private void onPurchaseFailedEvent(PurchasableItem item, PurchaseFailureReason reason)
		{
			onPurchaseFailed(new PurchaseFailedEvent(item, reason, null));
		}

		private bool verifyPlatformId(string platformId)
		{
			if (!remapper.canMapProductSpecificId(platformId))
			{
				logError(UnibillError.UNIBILL_UNKNOWN_PRODUCTID, platformId);
				return false;
			}
			return true;
		}
	}
}
