using System.Linq;
using Tests;
using Uniject;

namespace Unibill.Impl
{
	public class BillerFactory
	{
		private IResourceLoader loader;

		private ILogger logger;

		private IStorage storage;

		private IRawBillingPlatformProvider platformProvider;

		private IUtil util;

		private UnibillConfiguration config;

		private IUnityAnalytics analytics;

		private CurrencyManager _currencyManager;

		private TransactionDatabase _tDb;

		private HelpCentre _helpCentre;

		private ProductIdRemapper _remapper;

		public BillerFactory(IResourceLoader resourceLoader, ILogger logger, IStorage storage, IRawBillingPlatformProvider platformProvider, UnibillConfiguration config, IUtil util, IUnityAnalytics analytics)
		{
			loader = resourceLoader;
			this.logger = logger;
			this.storage = storage;
			this.platformProvider = platformProvider;
			this.config = config;
			this.util = util;
			this.analytics = analytics;
		}

		public Biller instantiate()
		{
			IBillingService billingSubsystem = instantiateBillingSubsystem();
			Biller biller = new Biller(config, getTransactionDatabase(), billingSubsystem, getLogger(), getHelp(), getMapper(), getCurrencyManager());
			instantiateAnalytics(biller);
			return biller;
		}

		public void instantiateAnalytics(Biller biller)
		{
			if (!string.IsNullOrEmpty(config.UnibillAnalyticsAppId))
			{
				new AnalyticsReporter(biller, config, platformProvider.getHTTPClient(util), getStorage(), util, platformProvider.getLevelLoadListener());
			}
		}

		private IBillingService instantiateBillingSubsystem()
		{
			switch (config.CurrentPlatform)
			{
			case BillingPlatform.AppleAppStore:
				return new AppleAppStoreBillingService(getMapper(), platformProvider.getStorekit(), util, getLogger());
			case BillingPlatform.AmazonAppstore:
				return new AmazonAppStoreBillingService(platformProvider.getAmazon(), getMapper(), getTransactionDatabase(), getLogger());
			case BillingPlatform.GooglePlay:
				return new GooglePlayBillingService(platformProvider.getGooglePlay(), config, getMapper(), getLogger());
			case BillingPlatform.MacAppStore:
				return new AppleAppStoreBillingService(getMapper(), platformProvider.getStorekit(), util, getLogger());
			case BillingPlatform.SamsungApps:
				return new SamsungAppsBillingService(config, platformProvider.getSamsung());
			default:
				return new FakeBillingService(getMapper());
			}
		}

		private CurrencyManager getCurrencyManager()
		{
			if (_currencyManager == null)
			{
				_currencyManager = new CurrencyManager(config, getStorage());
			}
			return _currencyManager;
		}

		private ProductDescription[] GetDummyProducts()
		{
			return (from x in config.AllPurchasableItems
				where x.PurchaseType != PurchaseType.Subscription
				select new ProductDescription(x.LocalId, "$123.45", x.name, x.description, null, 123.45m)
				{
					Consumable = (x.PurchaseType == PurchaseType.Consumable)
				}).ToArray();
		}

		private TransactionDatabase getTransactionDatabase()
		{
			if (_tDb == null)
			{
				_tDb = new TransactionDatabase(getStorage(), getLogger());
			}
			return _tDb;
		}

		private IStorage getStorage()
		{
			return storage;
		}

		private HelpCentre getHelp()
		{
			if (_helpCentre == null)
			{
				_helpCentre = new HelpCentre(loader.openTextFile("unibillStrings.json").ReadToEnd());
			}
			return _helpCentre;
		}

		private ProductIdRemapper getMapper()
		{
			if (_remapper == null)
			{
				_remapper = new ProductIdRemapper(config);
			}
			return _remapper;
		}

		private ILogger getLogger()
		{
			return logger;
		}

		private IResourceLoader getResourceLoader()
		{
			return loader;
		}
	}
}
