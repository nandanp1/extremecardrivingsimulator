using System;
using System.Collections.Generic;

namespace Unibill.Impl
{
	public static class MiniJsonExtensions
	{
		public static Dictionary<string, object> getHash(this Dictionary<string, object> dic, string key)
		{
			return (Dictionary<string, object>)dic[key];
		}

		public static T getEnum<T>(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return (T)Enum.Parse(typeof(T), dic[key].ToString(), true);
			}
			return default(T);
		}

		public static string getString(this Dictionary<string, object> dic, string key, string defaultValue = "")
		{
			if (dic.ContainsKey(key))
			{
				return dic[key].ToString();
			}
			return defaultValue;
		}

		public static long getLong(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return long.Parse(dic[key].ToString());
			}
			return 0L;
		}

		public static List<string> getStringList(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				List<string> list = new List<string>();
				List<object> list2 = (List<object>)dic[key];
				{
					foreach (object item in list2)
					{
						list.Add(item.ToString());
					}
					return list;
				}
			}
			return new List<string>();
		}

		public static bool getBool(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return bool.Parse(dic[key].ToString());
			}
			return false;
		}

		public static T get<T>(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return (T)dic[key];
			}
			return default(T);
		}

		public static string toJson(this Dictionary<string, object> obj)
		{
			return MiniJSON.jsonEncode(obj);
		}

		public static string toJson(this Dictionary<string, string> obj)
		{
			return MiniJSON.jsonEncode(obj);
		}

		public static string toJson(this string[] array)
		{
			List<object> list = new List<object>();
			foreach (string item in array)
			{
				list.Add(item);
			}
			return MiniJSON.jsonEncode(list);
		}

		public static List<object> arrayListFromJson(this string json)
		{
			return MiniJSON.jsonDecode(json) as List<object>;
		}

		public static Dictionary<string, object> hashtableFromJson(this string json)
		{
			return MiniJSON.jsonDecode(json) as Dictionary<string, object>;
		}
	}
}
