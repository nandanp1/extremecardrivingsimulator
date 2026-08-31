public class ChallangePlayerPrefIdentifier
{
	private ChallangeKey _key;

	private ChallangeSceneIndex _sceneIndex;

	public ChallangePlayerPrefIdentifier(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		_key = key;
		_sceneIndex = sceneIndex;
	}

	public ChallangeKey GetKey()
	{
		return _key;
	}

	public ChallangeSceneIndex GetSceneIndex()
	{
		return _sceneIndex;
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		return _key == (other as ChallangePlayerPrefIdentifier).GetKey() && _sceneIndex == (other as ChallangePlayerPrefIdentifier).GetSceneIndex();
	}

	public override int GetHashCode()
	{
		return _key.GetHashCode() ^ _sceneIndex.GetHashCode();
	}

	public static ChallangePlayerPrefIdentifier Identifier(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		return new ChallangePlayerPrefIdentifier(key, sceneIndex);
	}
}
