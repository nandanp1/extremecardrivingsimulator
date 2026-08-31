using UnityEngine;

public class CrashableParticles : MonoBehaviour
{
	public Transform particlesSpawner;

	public ParticleSystemCrasherType particlesType;

	private Crashable crashable;

	private void Reset()
	{
		crashable = GetComponent<Crashable>();
		if (!crashable)
		{
			Debug.LogError("Este script no encuentra un componente crashable en el objeto", base.gameObject);
			return;
		}
		EventDelegate eventDelegate = new EventDelegate(this, "OnCrash");
		EventDelegate eventDelegate2 = new EventDelegate(this, "OnReset");
		if (!crashable.onCrash.Contains(eventDelegate))
		{
			EventDelegate.Add(crashable.onCrash, eventDelegate);
		}
		if (!crashable.onReset.Contains(eventDelegate2))
		{
			EventDelegate.Add(crashable.onReset, eventDelegate2);
		}
	}

	private void Awake()
	{
		if (!particlesSpawner)
		{
			Debug.LogWarning("Este CrashableParticles no tiene un transform asignado. Se usará el suyo propio");
			particlesSpawner = base.transform;
		}
	}

	public void OnCrash()
	{
		CrashableParticlesPool.instance.PlayParticles(particlesSpawner, particlesType);
	}

	public void OnReset()
	{
		ParticleSystem componentInChildren = particlesSpawner.GetComponentInChildren<ParticleSystem>();
		if ((bool)componentInChildren)
		{
			CrashableParticlesPool.instance.StopParticles(componentInChildren);
		}
	}
}
