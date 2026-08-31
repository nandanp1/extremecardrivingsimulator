using System.Collections.Generic;
using Uniject;
using UnityEngine;

namespace Unibill.Impl
{
	public class UnityHTTPRequest : IHTTPRequest
	{
		private WWW w;

		public Dictionary<string, string> responseHeaders
		{
			get
			{
				return w.responseHeaders;
			}
		}

		public byte[] bytes
		{
			get
			{
				return w.bytes;
			}
		}

		public string contentString
		{
			get
			{
				return w.text;
			}
		}

		public string error
		{
			get
			{
				return w.error;
			}
		}

		public UnityHTTPRequest(WWW w)
		{
			this.w = w;
		}
	}
}
