using System.Collections.Generic;

namespace Uniject
{
	public interface IHTTPRequest
	{
		Dictionary<string, string> responseHeaders { get; }

		byte[] bytes { get; }

		string contentString { get; }

		string error { get; }
	}
}
