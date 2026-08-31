using System.Linq;
using Uniject;

namespace Unibill.Impl
{
	public class CurrencyManager
	{
		private IStorage storage;

		private UnibillConfiguration config;

		public string[] Currencies { get; private set; }

		public CurrencyManager(UnibillConfiguration config, IStorage storage)
		{
			this.storage = storage;
			this.config = config;
			Currencies = config.currencies.Select((VirtualCurrency x) => x.currencyId).ToArray();
		}

		public void OnPurchased(string id)
		{
			foreach (VirtualCurrency currency in config.currencies)
			{
				if (currency.mappings.ContainsKey(id))
				{
					CreditBalance(currency.currencyId, currency.mappings[id]);
				}
			}
		}

		public decimal GetCurrencyBalance(string id)
		{
			return storage.GetInt(getKey(id), 0);
		}

		public void CreditBalance(string id, decimal amount)
		{
			storage.SetInt(getKey(id), (int)(GetCurrencyBalance(id) + amount));
		}

		public void SetBalance(string id, decimal amount)
		{
			storage.SetInt(getKey(id), (int)amount);
		}

		public bool DebitBalance(string id, decimal amount)
		{
			decimal currencyBalance = GetCurrencyBalance(id);
			if (currencyBalance - amount >= 0m)
			{
				storage.SetInt(getKey(id), (int)(currencyBalance - amount));
				return true;
			}
			return false;
		}

		private string getKey(string id)
		{
			return string.Format("com.outlinegames.unibill.currencies.{0}.balance", id);
		}
	}
}
