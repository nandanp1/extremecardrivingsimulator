using UnityEngine;

public class RubbishParticles : MonoBehaviour
{
	private ParticleSystem particles;

	public CarVisuals carVisual;

	private void Awake()
	{
		particles = GetComponent<ParticleSystem>();
	}

	public void ChangeColor()
	{
		if (carVisual == null)
		{
			carVisual = Object.FindFirstObjectByType<CarVisuals>();
		}
		Color color = carVisual.Body.GetComponent<Renderer>().material.color;
		particles.startColor = color;
	}
}
