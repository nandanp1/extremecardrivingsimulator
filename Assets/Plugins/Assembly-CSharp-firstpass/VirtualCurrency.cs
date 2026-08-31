using System.Collections.Generic;

public class VirtualCurrency
{
	public string currencyId { get; set; }

	public Dictionary<string, decimal> mappings { get; private set; }

	public VirtualCurrency(string id, Dictionary<string, decimal> mappings)
	{
		currencyId = id;
		this.mappings = mappings;
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("currencyId", currencyId);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (KeyValuePair<string, decimal> mapping in mappings)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("id", mapping.Key);
			dictionary2.Add("amount", mapping.Value);
			list.Add(dictionary2);
		}
		dictionary.Add("mappings", list);
		return dictionary;
	}
}
