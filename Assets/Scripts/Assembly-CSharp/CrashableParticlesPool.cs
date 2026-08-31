using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashableParticlesPool : MonoBehaviour
{
	public static CrashableParticlesPool instance;

	public int maxPopulation = 5;

	public GameObject sparksParticlesPrefab;

	private Dictionary<ParticleSystemCrasherType, Stack<ParticleSystem>> pools;

	private Dictionary<ParticleSystemCrasherType, GameObject> particlesPrefabByType;

	private Dictionary<ParticleSystemCrasherType, HashSet<ParticleSystem>> activeParticles;

	private Vector3 inactivePosition;

	private Transform myTransform;

	private void Awake()
	{
		if ((bool)instance)
		{
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		instance = this;
		myTransform = base.transform;
		inactivePosition = new Vector3(9000f, 9000f, 9000f);
		particlesPrefabByType = new Dictionary<ParticleSystemCrasherType, GameObject>();
		particlesPrefabByType.Add(ParticleSystemCrasherType.Sparks, sparksParticlesPrefab);
		pools = new Dictionary<ParticleSystemCrasherType, Stack<ParticleSystem>>(maxPopulation);
		activeParticles = new Dictionary<ParticleSystemCrasherType, HashSet<ParticleSystem>>();
		foreach (int value in Enum.GetValues(typeof(ParticleSystemCrasherType)))
		{
			pools.Add((ParticleSystemCrasherType)value, new Stack<ParticleSystem>(maxPopulation));
			activeParticles.Add((ParticleSystemCrasherType)value, new HashSet<ParticleSystem>());
		}
		PopulatePools();
	}

	private void PopulatePools()
	{
		foreach (ParticleSystemCrasherType key in pools.Keys)
		{
			for (int i = 0; i < maxPopulation; i++)
			{
				CreateParticles(key);
			}
		}
	}

	private void CreateParticles(ParticleSystemCrasherType type)
	{
		if (type != ParticleSystemCrasherType.None)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(particlesPrefabByType[type], inactivePosition, Quaternion.identity) as GameObject;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(myTransform);
			pools[type].Push(gameObject.GetComponent<ParticleSystem>());
		}
	}

	public void PlayParticles(Transform t, ParticleSystemCrasherType type)
	{
		Debug.Log("Play particles of type " + type);
		if (type != ParticleSystemCrasherType.None)
		{
			Stack<ParticleSystem> stack = pools[type];
			if (stack.Count == 0)
			{
				CreateParticles(type);
			}
			ParticleSystem particleSystem = stack.Pop();
			particleSystem.transform.position = t.position;
			particleSystem.transform.rotation = t.rotation;
			particleSystem.transform.SetParent(t);
			activeParticles[type].Add(particleSystem);
			particleSystem.gameObject.SetActive(true);
			StartCoroutine("WaitAndStopParticles", particleSystem);
		}
	}

	private IEnumerator WaitAndStopParticles(ParticleSystem particles)
	{
		particles.Play();
		yield return new WaitForSeconds(particles.duration);
		if ((bool)particles && particles.gameObject.activeSelf)
		{
			StopParticles(particles);
		}
	}

	public void StopParticles(ParticleSystem particles)
	{
		Debug.Log("Stop particles");
		ParticleSystemCrasherType particleSystemCrasherType = ParticleSystemCrasherType.None;
		HashSet<ParticleSystem> hashSet = null;
		foreach (ParticleSystemCrasherType key in activeParticles.Keys)
		{
			if (activeParticles[key].Contains(particles))
			{
				Debug.Log("Particula identificada de tipo " + key);
				particleSystemCrasherType = key;
				hashSet = activeParticles[key];
				break;
			}
		}
		if (particleSystemCrasherType == ParticleSystemCrasherType.None)
		{
			Debug.LogWarning("Una particula que intentaba desactivarse no existe en el sistema de pools. Si existe en la escena se borrara");
			if ((bool)particles)
			{
				UnityEngine.Object.DestroyImmediate(particles);
			}
			return;
		}
		particles.gameObject.SetActive(false);
		hashSet.Remove(particles);
		Stack<ParticleSystem> stack = pools[particleSystemCrasherType];
		if (stack.Count >= maxPopulation)
		{
			UnityEngine.Object.DestroyImmediate(particles.gameObject);
			return;
		}
		particles.transform.position = inactivePosition;
		particles.transform.rotation = Quaternion.identity;
		particles.transform.SetParent(myTransform);
		stack.Push(particles);
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		foreach (ParticleSystemCrasherType key in pools.Keys)
		{
			foreach (ParticleSystem item in pools[key])
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		foreach (ParticleSystemCrasherType key2 in activeParticles.Keys)
		{
			foreach (ParticleSystem item2 in activeParticles[key2])
			{
				UnityEngine.Object.Destroy(item2);
			}
		}
	}
}
