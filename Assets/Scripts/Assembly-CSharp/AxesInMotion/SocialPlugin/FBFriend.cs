namespace AxesInMotion.SocialPlugin
{
	public struct FBFriend
	{
		public string id;

		public string firstName;

		public FBFriend(string id, string fullName)
		{
			this.id = id;
			firstName = fullName.Split(' ')[0];
		}

		public override string ToString()
		{
			return firstName + "_" + id;
		}
	}
}
