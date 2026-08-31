using System.Collections;
using UnityEngine;

public class SparksParticlesFlare : MonoBehaviour
{
	private ParticleSystem particles;

	[SerializeField]
	private GameObject flare;

	[SerializeField]
	private float timeToDisableFlare = 0.05f;

	private void Start()
	{
		flare.SetActive(false);
		particles = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!particles.isPlaying)
		{
			flare.SetActive(false);
		}
	}

	public void ActiveFlare()
	{
		StartCoroutine(DisableFlare());
	}

	private IEnumerator DisableFlare()
	{
		if (flare != null)
		{
			flare.SetActive(true);
			yield return new WaitForSeconds(timeToDisableFlare);
			flare.SetActive(false);
		}
	}
}
