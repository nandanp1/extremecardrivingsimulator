using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleParticlesController : MonoBehaviour
{
	private ParticleSystem rubbishParticleSystem;

	private ParticleSystem dustParticleSystem;

	private ParticleSystem sparksParticleSystem;

	private Vector3 impactVelocityVector;

	private GameLogic gameLogic;

	private float thresholdParticlesCarVelocity = 14f;

	private float impactVelocity;

	private bool particlesAvailable;

	private bool particlesUnavailableLogged;

	private float nextParticleLookupTime;

	private const float ParticleLookupRetryInterval = 1f;

	private void OnEnable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		TryResolveParticleSystems(true, false);
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start()
	{
		TryResolveParticleSystems(true, false);
		gameLogic = MySingleton<Manager>.Instance.gameLogic;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		TryResolveParticleSystems(true, false);
	}

	private bool TryResolveParticleSystems(bool forceLookup, bool logIfUnavailable)
	{
		particlesAvailable = rubbishParticleSystem != null && dustParticleSystem != null && sparksParticleSystem != null;
		if (particlesAvailable)
		{
			return true;
		}
		if (!forceLookup && Time.unscaledTime < nextParticleLookupTime)
		{
			return false;
		}

		nextParticleLookupTime = Time.unscaledTime + ParticleLookupRetryInterval;
		rubbishParticleSystem = FindSceneParticleSystem("RubbishParticles");
		dustParticleSystem = FindSceneParticleSystem("DustParticles");
		sparksParticleSystem = FindSceneParticleSystem("SparksParticles");
		particlesAvailable = rubbishParticleSystem != null && dustParticleSystem != null && sparksParticleSystem != null;
		if (particlesAvailable)
		{
			particlesUnavailableLogged = false;
		}
		if (!particlesAvailable && logIfUnavailable && !particlesUnavailableLogged)
		{
			ParticleSystem[] loadedSystems = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			Debug.Log(string.Format(
				"No se encuentran en la escena los sistemas de particulas!! loaded={0}, rubbish={1}, dust={2}, sparks={3}",
				loadedSystems.Length,
				rubbishParticleSystem != null,
				dustParticleSystem != null,
				sparksParticleSystem != null));
			particlesUnavailableLogged = true;
		}
		return particlesAvailable;
	}

	private static ParticleSystem FindSceneParticleSystem(string expectedName)
	{
		// The recovered Unity 4 scene retained legacy numeric tag IDs, while Unity 6
		// compacted the empty tag slots in TagManager.  Tag lookup therefore cannot
		// identify these three objects reliably.  They are unique roots of the shared
		// gameplay scene, so resolve that narrow, deterministic scope by exact name.
		Scene sharedGameplayScene = SceneManager.GetSceneByName("2Game");
		if (!sharedGameplayScene.IsValid() || !sharedGameplayScene.isLoaded)
		{
			return null;
		}

		GameObject[] roots = sharedGameplayScene.GetRootGameObjects();
		for (int i = 0; i < roots.Length; i++)
		{
			GameObject root = roots[i];
			if (root != null && root.name == expectedName)
			{
				return root.GetComponent<ParticleSystem>();
			}
		}
		return null;
	}

	private static void EnsureParticleSystemIsActive(ParticleSystem particleSystem)
	{
		if (particleSystem != null && !particleSystem.gameObject.activeSelf)
		{
			particleSystem.gameObject.SetActive(true);
		}
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		if (!TryResolveParticleSystems(true, true) || gameLogic == null)
		{
			return;
		}
		EnsureParticleSystemIsActive(rubbishParticleSystem);
		EnsureParticleSystemIsActive(dustParticleSystem);
		EnsureParticleSystemIsActive(sparksParticleSystem);

		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (!contactPoint.thisCollider.CompareTag("Car"))
			{
				continue;
			}
			rubbishParticleSystem.GetComponent<RubbishParticles>().ChangeColor();
			impactVelocityVector = GetComponent<Rigidbody>().linearVelocity;
			impactVelocity = Mathf.Abs(gameLogic.GetSpeedMS(collisionInfo.relativeVelocity));
			if (impactVelocity > thresholdParticlesCarVelocity)
			{
				dustParticleSystem.transform.position = contactPoint.point;
				dustParticleSystem.transform.LookAt(contactPoint.point + -impactVelocityVector.normalized * 4f);
				dustParticleSystem.Stop();
				dustParticleSystem.Clear();
				if (!dustParticleSystem.isPlaying)
				{
					dustParticleSystem.Play();
					dustParticleSystem.transform.parent = base.transform;
				}
				break;
			}
		}
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (!TryResolveParticleSystems(false, false) || gameLogic == null)
		{
			return;
		}

		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (contactPoint.thisCollider.CompareTag("Car") && impactVelocity > thresholdParticlesCarVelocity)
			{
				rubbishParticleSystem.transform.position = contactPoint.point;
				rubbishParticleSystem.transform.LookAt(contactPoint.point + contactPoint.normal * 4f);
				if (!rubbishParticleSystem.isPlaying)
				{
					rubbishParticleSystem.Play();
				}
				sparksParticleSystem.transform.position = contactPoint.point;
				sparksParticleSystem.transform.LookAt(contactPoint.point + -impactVelocityVector.normalized * 4f);
				sparksParticleSystem.Stop();
				sparksParticleSystem.Clear();
				if (!sparksParticleSystem.isPlaying)
				{
					sparksParticleSystem.Play();
					sparksParticleSystem.GetComponent<SparksParticlesFlare>().ActiveFlare();
					impactVelocity = Mathf.Abs(gameLogic.GetSpeedMS(GetComponent<Rigidbody>().linearVelocity));
					sparksParticleSystem.transform.parent = base.transform;
				}
			}
		}
	}

	private void OnCollisionExit(Collision collisionInfo)
	{
		impactVelocityVector = Vector3.zero;
		impactVelocity = 0f;
	}
}
