using UnityEngine;

public class UnparentWhenParticleDie : MonoBehaviour
{
	private ParticleSystem particles;

	private void Start()
	{
		particles = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!particles.isPlaying)
		{
			base.transform.parent = null;
		}
	}
}
