using System.Collections.Generic;
using System.IO;

namespace Unibill.Impl
{
	public class Util
	{
		public static string ReadAllText(string path)
		{
			using (StreamReader streamReader = new StreamReader(path))
			{
				return streamReader.ReadToEnd();
			}
		}

		public static void WriteAllText(string path, string text)
		{
			using (StreamWriter streamWriter = new StreamWriter(path))
			{
				streamWriter.Write(text);
			}
		}

		public static List<ProductDescription> DeserialiseProductList(string json)
		{
			Dictionary<string, object> productHash = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			return DeserialiseProductList(productHash);
		}

		public static List<ProductDescription> DeserialiseProductList(Dictionary<string, object> productHash)
		{
			List<ProductDescription> list = new List<ProductDescription>();
			foreach (string key in productHash.Keys)
			{
				Dictionary<string, object> dic = (Dictionary<string, object>)productHash[key];
				ProductDescription item = new ProductDescription(key, dic.getString("price", string.Empty), dic.getString("localizedTitle", string.Empty), dic.getString("localizedDescription", string.Empty), dic.getString("isoCurrencyCode", string.Empty), decimal.Parse(dic.getString("priceDecimal", string.Empty)), dic.getString("receipt", string.Empty), dic.getString("transactionId", string.Empty));
				list.Add(item);
			}
			return list;
		}
	}
}
