using System;
using System.Collections.Generic;
using Uniject;
using UnityEngine;
using ILogger = Uniject.ILogger;

namespace Unibill.Impl
{
	public class RemoteConfigManager
	{
		private const string CACHED_CONFIG_PATH = "com.outlinegames.unibill.cached.config";

		private IStorage storage;

		public string XML;

		public UnibillConfiguration Config { get; private set; }

		public RemoteConfigManager(IResourceLoader loader, IStorage storage, ILogger logger, RuntimePlatform platform, List<ProductDefinition> runtimeProducts = null)
		{
			this.storage = storage;
			logger.prefix = "Unibill.RemoteConfigManager";
			XML = loader.openTextFile("unibillInventory.json").ReadToEnd();
			Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
			if (Config.UseHostedConfig)
			{
				string text = storage.GetString("com.outlinegames.unibill.cached.config", string.Empty);
				if (string.IsNullOrEmpty(text))
				{
					logger.Log("No cached config available. Using bundled");
				}
				else
				{
					logger.Log("Cached config found, attempting to parse");
					try
					{
						Config = new UnibillConfiguration(text, platform, logger, runtimeProducts);
						if (Config.inventory.Count == 0)
						{
							logger.LogError("No purchasable items in cached config, ignoring.");
							Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
						}
						else
						{
							logger.Log(string.Format("Using cached config with {0} purchasable items", Config.inventory.Count));
							XML = text;
						}
					}
					catch (Exception ex)
					{
						logger.LogError("Error parsing inventory: {0}", ex.Message);
						Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
					}
				}
				refreshCachedConfig(Config.HostedConfigUrl, logger);
			}
			else
			{
				logger.Log("Not using cached inventory, using bundled.");
				Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
			}
		}

		private void refreshCachedConfig(string url, ILogger logger)
		{
			logger.Log("Trying to fetch remote config...");
			new GameObject().AddComponent<RemoteConfigFetcher>().Fetch(storage, Config.HostedConfigUrl, "com.outlinegames.unibill.cached.config");
		}
	}
}
