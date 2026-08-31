namespace AxesInMotion.SocialPlugin
{
	public interface ISocial
	{
		bool Requesting { get; set; }

		string Id();

		bool IsInitialized();

		bool IsLoggedIn();

		void Init(SocialInitializedCallbackDelegate initializedCallback);

		void Login(SocialLoginCallbackDelegate callback = null, SocialScope[] scopes = null);

		void Logout();
	}
}
