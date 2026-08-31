using System;
using System.Collections.Generic;

namespace Unibill.Impl
{
	public class ProductIdRemapper
	{
		private Dictionary<string, string> genericToPlatformSpecificIds;

		private Dictionary<string, string> platformSpecificToGenericIds;

		public UnibillConfiguration db;

		public ProductIdRemapper(UnibillConfiguration config)
		{
			db = config;
			initialiseForPlatform(config.CurrentPlatform);
		}

		public void AddDirectMapping(string id)
		{
			genericToPlatformSpecificIds[id] = id;
			platformSpecificToGenericIds[id] = id;
		}

		public void initialiseForPlatform(BillingPlatform platform)
		{
			genericToPlatformSpecificIds = new Dictionary<string, string>();
			platformSpecificToGenericIds = new Dictionary<string, string>();
			foreach (PurchasableItem item in db.inventory)
			{
				genericToPlatformSpecificIds[item.Id] = item.LocalId;
				platformSpecificToGenericIds[item.LocalId] = item.Id;
			}
		}

		public string[] getAllPlatformSpecificProductIds()
		{
			List<string> list = new List<string>();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				list.Add(mapItemIdToPlatformSpecificId(allPurchasableItem));
			}
			return list.ToArray();
		}

		public string mapItemIdToPlatformSpecificId(PurchasableItem item)
		{
			if (!genericToPlatformSpecificIds.ContainsKey(item.Id))
			{
				throw new ArgumentException("Unknown product id: " + item.Id);
			}
			return genericToPlatformSpecificIds[item.Id];
		}

		public PurchasableItem getPurchasableItemFromPlatformSpecificId(string platformSpecificId)
		{
			string id = platformSpecificToGenericIds[platformSpecificId];
			return db.getItemById(id);
		}

		public bool canMapProductSpecificId(string id)
		{
			if (platformSpecificToGenericIds.ContainsKey(id))
			{
				return true;
			}
			return false;
		}
	}
}
