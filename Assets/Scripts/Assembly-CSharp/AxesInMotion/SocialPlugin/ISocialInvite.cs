namespace AxesInMotion.SocialPlugin
{
	public interface ISocialInvite
	{
		void Invite(string title, string message, int maxRecipients, SocialInviteCallbackDelegate callback = null);
	}
}
