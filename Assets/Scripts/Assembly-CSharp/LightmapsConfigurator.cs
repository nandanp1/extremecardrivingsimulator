using System.Collections.Generic;
using UnityEngine;

public class LightmapsConfigurator : MonoBehaviour
{
	public bool dontNeedLightmapConfiguration;

	public Texture OriginDayLightmap;

	public Texture OriginNighLightmap;

	public Texture tempDayLightmap;

	public Texture tempNightLightmap;

	public string directory;

	public string sceneName;

	public List<Material> sceneMaterials;

	public List<Material> commonMaterials;

	public List<Renderer> modifiedRenderers;

	public new RealTimeLight light;

	public List<Material> allMaterials;

	private void Start()
	{
		light = Object.FindFirstObjectByType<RealTimeLight>();
		ReplaceMaterials();
		AddDNMaterials();
		light.SetDayNight(light.time);
	}

	private void OnDestroy()
	{
		RemoveDNMaterials();
	}

	public void ReplaceMaterials()
	{
		foreach (Renderer modifiedRenderer in modifiedRenderers)
		{
			int num = modifiedRenderer.sharedMaterials.Length;
			Material[] array = new Material[num];
			for (int i = 0; i < num; i++)
			{
				Material originalMaterial = modifiedRenderer.sharedMaterials[i];
				array[i] = originalMaterial;
				if (originalMaterial == null)
				{
					continue;
				}

				string path = string.Empty;
				if (originalMaterial.HasProperty("_Lightmap1") && originalMaterial.HasProperty("_Lightmap2"))
				{
					string empty = originalMaterial.name;
					path = sceneName + "_Materials/" + empty + "_" + sceneName;
				}
				else if (originalMaterial.HasProperty("_DayNight"))
				{
					string empty = originalMaterial.name;
					path = "Common_Materials/" + empty + "_Common";
				}

				if (!string.IsNullOrEmpty(path))
				{
					Material replacement = Resources.Load<Material>(path);
					if (replacement != null)
					{
						array[i] = replacement;
					}
				}
			}
			modifiedRenderer.sharedMaterials = array;
		}
	}

	public void AddDNMaterials()
	{
		light.AddDNMaterials(sceneMaterials, commonMaterials);
	}

	public void RemoveDNMaterials()
	{
		light.RemoveDNMaterials(sceneMaterials);
	}
}
