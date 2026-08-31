using System.Collections.Generic;

namespace Uniject
{
	public interface IURLFetcher
	{
		object doGet(string url, Dictionary<string, string> headers);

		object doPost(string url, Dictionary<string, string> parameters);

		IHTTPRequest getResponse();
	}
}
