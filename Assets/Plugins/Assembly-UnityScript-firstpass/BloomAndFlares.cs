using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Bloom and Flares")]
public class BloomAndFlares : PostEffectsBase
{
	public Shader addAlphaHackShader;

	private Material _alphaAddMaterial;

	public Shader lensFlareShader;

	private Material _lensFlareMaterial;

	public Shader vignetteShader;

	private Material _vignetteMaterial;

	public Shader separableBlurShader;

	private Material _separableBlurMaterial;

	public Shader addBrightStuffOneOneShader;

	private Material _addBrightStuffBlendOneOneMaterial;

	public Shader hollywoodFlareBlurShader;

	private Material _hollywoodFlareBlurMaterial;

	public Shader hollywoodFlareStretchShader;

	private Material _hollywoodFlareStretchMaterial;

	public Shader brightPassFilterShader;

	private Material _brightPassFilterMaterial;

	public string bloomThisTag;

	public float sepBlurSpread;

	public float useSrcAlphaAsMask;

	public float bloomIntensity;

	public float bloomThreshhold;

	public int bloomBlurIterations;

	public TweakMode tweakMode;

	public bool lensflares;

	public int hollywoodFlareBlurIterations;

	public LensflareStyle lensflareMode;

	public float hollyStretchWidth;

	public float lensflareIntensity;

	public float lensflareThreshhold;

	public Color flareColorA;

	public Color flareColorB;

	public Color flareColorC;

	public Color flareColorD;

	public float blurWidth;

	public BloomAndFlares()
	{
		sepBlurSpread = 1.5f;
		useSrcAlphaAsMask = 0.5f;
		bloomIntensity = 1f;
		bloomThreshhold = 0.4f;
		bloomBlurIterations = 3;
		tweakMode = TweakMode.Advanced;
		lensflares = true;
		hollywoodFlareBlurIterations = 4;
		hollyStretchWidth = 2.5f;
		lensflareIntensity = 0.75f;
		lensflareThreshhold = 0.5f;
		flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
		flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
		flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
		flareColorD = new Color(0.8f, 0.4f, 0f, 0.75f);
		blurWidth = 1f;
	}

	public override void Start()
	{
		CreateMaterials();
	}

	public virtual void CreateMaterials()
	{
		if (!_lensFlareMaterial)
		{
			if (!CheckShader(lensFlareShader))
			{
				enabled = false;
				return;
			}
			_lensFlareMaterial = new Material(lensFlareShader);
			_lensFlareMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_vignetteMaterial)
		{
			if (!CheckShader(vignetteShader))
			{
				enabled = false;
				return;
			}
			_vignetteMaterial = new Material(vignetteShader);
			_vignetteMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_separableBlurMaterial)
		{
			if (!CheckShader(separableBlurShader))
			{
				enabled = false;
				return;
			}
			_separableBlurMaterial = new Material(separableBlurShader);
			_separableBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_addBrightStuffBlendOneOneMaterial)
		{
			if (!CheckShader(addBrightStuffOneOneShader))
			{
				enabled = false;
				return;
			}
			_addBrightStuffBlendOneOneMaterial = new Material(addBrightStuffOneOneShader);
			_addBrightStuffBlendOneOneMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_hollywoodFlareBlurMaterial)
		{
			if (!CheckShader(hollywoodFlareBlurShader))
			{
				enabled = false;
				return;
			}
			_hollywoodFlareBlurMaterial = new Material(hollywoodFlareBlurShader);
			_hollywoodFlareBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_hollywoodFlareStretchMaterial)
		{
			if (!CheckShader(hollywoodFlareStretchShader))
			{
				enabled = false;
				return;
			}
			_hollywoodFlareStretchMaterial = new Material(hollywoodFlareStretchShader);
			_hollywoodFlareStretchMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_brightPassFilterMaterial)
		{
			if (!CheckShader(brightPassFilterShader))
			{
				enabled = false;
				return;
			}
			_brightPassFilterMaterial = new Material(brightPassFilterShader);
			_brightPassFilterMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_alphaAddMaterial)
		{
			if (!CheckShader(addAlphaHackShader))
			{
				enabled = false;
				return;
			}
			_alphaAddMaterial = new Material(addAlphaHackShader);
			_alphaAddMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public virtual void OnPreCull()
	{
		if (string.IsNullOrEmpty(bloomThisTag) || !(bloomThisTag != "Untagged"))
		{
			return;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag(bloomThisTag);
		int i = 0;
		GameObject[] array2 = array;
		for (int length = array2.Length; i < length; i++)
		{
			if ((bool)(MeshFilter)array2[i].GetComponent(typeof(MeshFilter)))
			{
				Mesh sharedMesh = (((MeshFilter)array2[i].GetComponent(typeof(MeshFilter))) as MeshFilter).sharedMesh;
				Graphics.DrawMesh(sharedMesh, array2[i].transform.localToWorldMatrix, _alphaAddMaterial, 0, GetComponent<Camera>());
			}
		}
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		RenderTexture temporary = RenderTexture.GetTemporary((int)((float)source.width / 2f), (int)((float)source.height / 2f), 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary((int)((float)source.width / 4f), (int)((float)source.height / 4f), 0);
		RenderTexture temporary3 = RenderTexture.GetTemporary((int)((float)source.width / 4f), (int)((float)source.height / 4f), 0);
		RenderTexture temporary4 = RenderTexture.GetTemporary((int)((float)source.width / 4f), (int)((float)source.height / 4f), 0);
		Graphics.Blit(source, temporary);
		Graphics.Blit(temporary, temporary2);
		_brightPassFilterMaterial.SetVector("threshhold", new Vector4(bloomThreshhold, 1f / (1f - bloomThreshhold), 0f, 0f));
		_brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", useSrcAlphaAsMask);
		Graphics.Blit(temporary2, temporary3, _brightPassFilterMaterial);
		if (bloomBlurIterations < 1)
		{
			bloomBlurIterations = 1;
		}
		Graphics.Blit(temporary3, temporary2);
		for (int i = 0; i < bloomBlurIterations; i++)
		{
			_separableBlurMaterial.SetVector("offsets", new Vector4(0f, sepBlurSpread * 1f / (float)temporary2.height, 0f, 0f));
			Graphics.Blit(temporary2, temporary4, _separableBlurMaterial);
			_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
			Graphics.Blit(temporary4, temporary2, _separableBlurMaterial);
		}
		Graphics.Blit(source, destination);
		if (lensflares)
		{
			if (lensflareMode == LensflareStyle.Ghosting)
			{
				_brightPassFilterMaterial.SetVector("threshhold", new Vector4(lensflareThreshhold, 1f / (1f - lensflareThreshhold), 0f, 0f));
				_brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", 0f);
				Graphics.Blit(temporary2, temporary4, _brightPassFilterMaterial);
				_separableBlurMaterial.SetVector("offsets", new Vector4(0f, sepBlurSpread * 1f / (float)temporary2.height, 0f, 0f));
				Graphics.Blit(temporary4, temporary3, _separableBlurMaterial);
				_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
				Graphics.Blit(temporary3, temporary4, _separableBlurMaterial);
				_vignetteMaterial.SetFloat("vignetteIntensity", 1f);
				Graphics.Blit(temporary4, temporary3, _vignetteMaterial);
				_lensFlareMaterial.SetVector("color0", new Vector4(0f, 0f, 0f, 0f) * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorA", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorB", new Vector4(flareColorB.r, flareColorB.g, flareColorB.b, flareColorB.a) * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorC", new Vector4(flareColorC.r, flareColorC.g, flareColorC.b, flareColorC.a) * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorD", new Vector4(flareColorD.r, flareColorD.g, flareColorD.b, flareColorD.a) * lensflareIntensity);
				Graphics.Blit(temporary3, temporary4, _lensFlareMaterial);
				_addBrightStuffBlendOneOneMaterial.SetFloat("intensity", 1f);
				Graphics.Blit(temporary4, temporary2, _addBrightStuffBlendOneOneMaterial);
			}
			else if (lensflareMode == LensflareStyle.Hollywood)
			{
				_brightPassFilterMaterial.SetVector("threshhold", new Vector4(lensflareThreshhold, 1f / (1f - lensflareThreshhold), 0f, 0f));
				_brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", 0f);
				Graphics.Blit(temporary3, temporary4, _brightPassFilterMaterial);
				_hollywoodFlareBlurMaterial.SetVector("offsets", new Vector4(0f, sepBlurSpread * 1f / (float)temporary2.height, 0f, 0f));
				_hollywoodFlareBlurMaterial.SetTexture("_NonBlurredTex", temporary2);
				_hollywoodFlareBlurMaterial.SetVector("tintColor", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * flareColorA.a * lensflareIntensity);
				Graphics.Blit(temporary4, temporary3, _hollywoodFlareBlurMaterial);
				_hollywoodFlareStretchMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
				_hollywoodFlareStretchMaterial.SetFloat("stretchWidth", hollyStretchWidth);
				Graphics.Blit(temporary3, temporary4, _hollywoodFlareStretchMaterial);
				for (int j = 0; j < hollywoodFlareBlurIterations; j++)
				{
					_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
					Graphics.Blit(temporary4, temporary3, _separableBlurMaterial);
					_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
					Graphics.Blit(temporary3, temporary4, _separableBlurMaterial);
				}
				_addBrightStuffBlendOneOneMaterial.SetFloat("intensity", 1f);
				Graphics.Blit(temporary4, temporary2, _addBrightStuffBlendOneOneMaterial);
			}
			else
			{
				_brightPassFilterMaterial.SetVector("threshhold", new Vector4(lensflareThreshhold, 1f / (1f - lensflareThreshhold), 0f, 0f));
				_brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", 0f);
				Graphics.Blit(temporary3, temporary4, _brightPassFilterMaterial);
				_hollywoodFlareBlurMaterial.SetVector("offsets", new Vector4(0f, sepBlurSpread * 1f / (float)temporary2.height, 0f, 0f));
				_hollywoodFlareBlurMaterial.SetTexture("_NonBlurredTex", temporary2);
				_hollywoodFlareBlurMaterial.SetVector("tintColor", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * flareColorA.a * lensflareIntensity);
				Graphics.Blit(temporary4, temporary3, _hollywoodFlareBlurMaterial);
				_hollywoodFlareStretchMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
				_hollywoodFlareStretchMaterial.SetFloat("stretchWidth", hollyStretchWidth);
				Graphics.Blit(temporary3, temporary4, _hollywoodFlareStretchMaterial);
				for (int k = 0; k < hollywoodFlareBlurIterations; k++)
				{
					_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
					Graphics.Blit(temporary4, temporary3, _separableBlurMaterial);
					_separableBlurMaterial.SetVector("offsets", new Vector4(sepBlurSpread * 1f / (float)temporary2.width, 0f, 0f, 0f));
					Graphics.Blit(temporary3, temporary4, _separableBlurMaterial);
				}
				_vignetteMaterial.SetFloat("vignetteIntensity", 1f);
				Graphics.Blit(temporary4, temporary3, _vignetteMaterial);
				_lensFlareMaterial.SetVector("color0", new Vector4(0f, 0f, 0f, 0f) * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorA", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * flareColorA.a * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorB", new Vector4(flareColorB.r, flareColorB.g, flareColorB.b, flareColorB.a) * flareColorB.a * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorC", new Vector4(flareColorC.r, flareColorC.g, flareColorC.b, flareColorC.a) * flareColorC.a * lensflareIntensity);
				_lensFlareMaterial.SetVector("colorD", new Vector4(flareColorD.r, flareColorD.g, flareColorD.b, flareColorD.a) * flareColorD.a * lensflareIntensity);
				Graphics.Blit(temporary3, temporary4, _lensFlareMaterial);
				_addBrightStuffBlendOneOneMaterial.SetFloat("intensity", 1f);
				Graphics.Blit(temporary4, temporary2, _addBrightStuffBlendOneOneMaterial);
			}
		}
		_addBrightStuffBlendOneOneMaterial.SetFloat("intensity", bloomIntensity);
		Graphics.Blit(temporary2, destination, _addBrightStuffBlendOneOneMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
		RenderTexture.ReleaseTemporary(temporary4);
	}

	public override void Main()
	{
	}
}
