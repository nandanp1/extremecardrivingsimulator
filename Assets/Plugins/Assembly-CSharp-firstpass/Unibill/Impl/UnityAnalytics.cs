using System;
using System.Collections.Generic;
using System.Reflection;
using unibill.Dummy;

namespace Unibill.Impl
{
	public class UnityAnalytics : IUnityAnalytics
	{
		private MethodInfo[] analyticsMethods;

		private readonly string[] UnityAnalyticsTypes = new string[2] { "UnityEngine.Cloud.Analytics.UnityAnalytics", "UnityEngine.Analytics.Analytics, UnityEngine.Analytics" };

		public UnityAnalytics()
		{
			analyticsMethods = GetUnityAnalyticsMethods(UnityAnalyticsTypes);
		}

		public void Transaction(string productId, decimal price, string currency, string receipt, string signature)
		{
			object[] parameters = new object[5] { productId, price, currency, receipt, signature };
			MethodInfo[] array = analyticsMethods;
			foreach (MethodInfo methodInfo in array)
			{
				methodInfo.Invoke(null, parameters);
			}
		}

		private static MethodInfo[] GetUnityAnalyticsMethods(string[] typeNamesToSearch)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (string typeName in typeNamesToSearch)
			{
				Type type = Type.GetType(typeName);
				if (type != null)
				{
					Type[] paramTypes = new Type[5]
					{
						typeof(string),
						typeof(decimal),
						typeof(string),
						typeof(string),
						typeof(string)
					};
					MethodInfo method = WinRTUtils.GetMethod(type, "Transaction", paramTypes);
					if (method != null)
					{
						list.Add(method);
					}
				}
			}
			return list.ToArray();
		}
	}
}
