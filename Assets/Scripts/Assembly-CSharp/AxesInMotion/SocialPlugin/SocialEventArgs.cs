namespace AxesInMotion.SocialPlugin
{
	public class SocialEventArgs : ISocialEventArgs
	{
		public bool Error { get; set; }

		public string Message { get; set; }

		public SocialEventType Type { get; set; }

		public SocialEventArgs(SocialEventType type)
		{
			Type = type;
		}
	}
}
