namespace Unibill.Impl
{
	public class PostParameter
	{
		public string name { get; private set; }

		public string value { get; private set; }

		public PostParameter(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}
}
