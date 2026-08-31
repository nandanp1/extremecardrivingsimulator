namespace Unibill.Impl
{
	public interface IHTTPClient
	{
		void doPost(string url, params PostParameter[] parameters);
	}
}
