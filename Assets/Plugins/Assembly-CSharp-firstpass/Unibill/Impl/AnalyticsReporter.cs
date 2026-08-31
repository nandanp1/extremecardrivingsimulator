using System;
using System.Collections.Generic;
using System.Globalization;
using Uniject;
using UnityEngine;

namespace Unibill.Impl
{
	public class AnalyticsReporter
	{
		private enum EventType
		{
			purchase_succeeded = 0,
			purchase_cancelled = 1,
			purchase_failed = 2,
			purchase_refunded = 3,
			new_installation = 4,
			new_session = 5,
			level_change = 6
		}

		private const string ANALYTICS_URL = "http://stats.unibiller.com/stats";

		private const string USER_ID_KEY = "com.outlinegames.unilytics.analytics.userId";

		public const string UNIBILL_VERSION = "1.9.0";

		private UnibillConfiguration config;

		private IHTTPClient client;

		private IUtil util;

		private string userId;

		private bool restoreInProgress;

		private string levelName;

		private DateTime levelLoadTime;

		public AnalyticsReporter(Biller biller, UnibillConfiguration config, IHTTPClient client, IStorage storage, IUtil util, ILevelLoadListener listener)
		{
			this.config = config;
			this.client = client;
			this.util = util;
			userId = getUserId(storage);
			biller.onPurchaseComplete += onSucceeded;
			biller.onPurchaseCancelled += (PurchasableItem obj) =>
			{
				onEvent(EventType.purchase_cancelled, obj, null);
			};
			biller.onPurchaseRefunded += (PurchasableItem obj) =>
			{
				onEvent(EventType.purchase_refunded, obj, null);
			};
			biller.onTransactionRestoreBegin += (bool obj) =>
			{
				restoreInProgress = true;
			};
			biller.onTransactionsRestored += (bool obj) =>
			{
				restoreInProgress = false;
			};
			listener.registerListener(() =>
			{
				onLevelLoad();
			});
			onEvent(EventType.new_session, null, null);
			levelName = util.loadedLevelName();
			levelLoadTime = DateTime.UtcNow;
		}

		private void onLevelLoad()
		{
			Dictionary<string, object> baseRequest = getBaseRequest(EventType.level_change);
			baseRequest.Add("levelChange", encodeLevelChange());
			levelLoadTime = DateTime.UtcNow;
			levelName = LegacySceneManager.LoadedLevelName;
			onEvent(baseRequest);
		}

		private string getUserId(IStorage storage)
		{
			string text = storage.GetString("com.outlinegames.unilytics.analytics.userId", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = Guid.NewGuid().ToString();
				storage.SetString("com.outlinegames.unilytics.analytics.userId", text);
				onEvent(EventType.new_installation, null, null);
			}
			return text;
		}

		private void onSucceeded(PurchaseEvent e)
		{
			if (!restoreInProgress)
			{
				onEvent(EventType.purchase_succeeded, e.PurchasedItem, e.Receipt);
			}
		}

		private void onCancelled(PurchaseEvent e)
		{
			onEvent(EventType.purchase_cancelled, e.PurchasedItem, null);
		}

		private void onEvent(EventType e, PurchasableItem item, string receipt)
		{
			Dictionary<string, object> baseRequest = getBaseRequest(e);
			if (item != null)
			{
				baseRequest.Add("item", encodeItem(item, receipt));
			}
			onEvent(baseRequest);
		}

		private void onEvent(Dictionary<string, object> e)
		{
			if (!string.IsNullOrEmpty(config.UnibillAnalyticsAppId))
			{
				string value = MiniJSON.jsonEncode(e);
				client.doPost("http://stats.unibiller.com/stats", new PostParameter("payload", value));
			}
		}

		private Dictionary<string, object> encodeLevelChange()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("fromLevel", levelName);
			dictionary.Add("fromTime", formatTimestamp(levelLoadTime));
			dictionary.Add("toLevel", util.loadedLevelName());
			dictionary.Add("toTime", formatTimestamp(DateTime.UtcNow));
			return dictionary;
		}

		private static string formatTimestamp(DateTime timestamp)
		{
			return timestamp.ToString("s", CultureInfo.InvariantCulture);
		}

		private Dictionary<string, object> getBaseRequest(EventType eventType)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("appId", config.UnibillAnalyticsAppId);
			dictionary.Add("userId", userId);
			dictionary.Add("appSecret", config.UnibillAnalyticsAppSecret);
			dictionary.Add("eventType", eventType.ToString());
			dictionary.Add("platform", config.CurrentPlatform.ToString());
			dictionary.Add("unibillVersion", "1.9.0");
			dictionary.Add("nonce", Guid.NewGuid().ToString());
			dictionary.Add("deviceInfo", encodeDeviceInfo());
			dictionary.Add("config", encodeConfig());
			return dictionary;
		}

		private Dictionary<string, object> encodeConfig()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("useAmazonSandbox", config.AmazonSandboxEnabled);
			dictionary.Add("samsungAppsMode", config.SamsungAppsMode.ToString());
			dictionary.Add("useHostedConfig", config.UseHostedConfig);
			dictionary.Add("useWin81Sandbox", config.UseWin8_1Sandbox);
			dictionary.Add("useWP8Sandbox", config.WP8SandboxEnabled);
			return dictionary;
		}

		private Dictionary<string, object> encodeItem(PurchasableItem item, string receipt)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("id", item.Id);
			dictionary.Add("currency", item.isoCurrencySymbol);
			dictionary.Add("price", item.priceInLocalCurrency.ToString());
			dictionary.Add("priceString", item.localizedPriceString);
			if (receipt != null)
			{
				dictionary.Add("receipt", receipt);
			}
			return dictionary;
		}

		private Dictionary<string, object> encodeDeviceInfo()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("deviceModel", util.DeviceModel);
			dictionary.Add("deviceName", util.DeviceName);
			dictionary.Add("deviceType", util.DeviceType.ToString());
			dictionary.Add("os", util.OperatingSystem);
			return dictionary;
		}
	}
}
