namespace AxesInMotion.SocialPlugin
{
	public interface ISocialEventArgs
	{
		bool Error { get; set; }

		string Message { get; set; }

		SocialEventType Type { get; set; }
	}
}
