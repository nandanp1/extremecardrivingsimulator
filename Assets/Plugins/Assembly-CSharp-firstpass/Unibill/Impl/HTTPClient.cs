using System.Collections;
using System.Collections.Generic;
using Uniject;
using UnityEngine;

namespace Unibill.Impl
{
	public class HTTPClient : IHTTPClient
	{
		private class PostRequest
		{
			public string url;

			public PostParameter[] parameters;

			public PostRequest(string url, params PostParameter[] parameters)
			{
				this.url = url;
				this.parameters = parameters;
			}
		}

		private Queue<PostRequest> events = new Queue<PostRequest>();

		private WaitForSeconds wait = new WaitForSeconds(5f);

		public HTTPClient(IUtil util)
		{
			util.InitiateCoroutine(pump());
		}

		public void doPost(string url, params PostParameter[] parameters)
		{
			events.Enqueue(new PostRequest(url, parameters));
		}

		private IEnumerator pump()
		{
			while (true)
			{
				if (events.Count > 0)
				{
					PostRequest e = events.Dequeue();
					WWWForm form = new WWWForm();
					for (int t = 0; t < e.parameters.Length; t++)
					{
						form.AddField(e.parameters[0].name, e.parameters[t].value);
					}
					WWW w = new WWW(e.url, form);
					yield return w;
					if (string.IsNullOrEmpty(w.error))
					{
						continue;
					}
					events.Enqueue(e);
					yield return new WaitForSeconds(60f);
				}
				yield return wait;
			}
		}
	}
}
