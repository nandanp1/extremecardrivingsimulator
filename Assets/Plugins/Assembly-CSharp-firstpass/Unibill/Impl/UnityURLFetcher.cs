using System.Collections.Generic;
using Uniject;
using UnityEngine;

namespace Unibill.Impl
{
	public class UnityURLFetcher : IURLFetcher
	{
		private UnityHTTPRequest request;

		public object doGet(string url, Dictionary<string, string> headers)
		{
			WWW wWW = new WWW(url, null, headers);
			request = new UnityHTTPRequest(wWW);
			return wWW;
		}

		public object doPost(string url, Dictionary<string, string> parameters)
		{
			WWWForm wWWForm = new WWWForm();
			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				wWWForm.AddField(parameter.Key, parameter.Value);
			}
			WWW wWW = new WWW(url, wWWForm);
			request = new UnityHTTPRequest(wWW);
			return wWW;
		}

		public IHTTPRequest getResponse()
		{
			return request;
		}
	}
}
