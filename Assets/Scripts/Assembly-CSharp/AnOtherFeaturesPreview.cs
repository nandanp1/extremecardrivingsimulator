using UnityEngine;

public class AnOtherFeaturesPreview : MonoBehaviour
{
	private void EnableImmersiveMode()
	{
		Singleton<ImmersiveMode>.instance.EnableImmersiveMode();
	}
}
