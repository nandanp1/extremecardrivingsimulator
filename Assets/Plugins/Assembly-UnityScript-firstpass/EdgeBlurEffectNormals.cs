using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Edge Blur")]
[RequireComponent(typeof(Camera))]
public class EdgeBlurEffectNormals : PostEffectsBase
{
	public Shader edgeDetectHqShader;

	private Material _edgeDetectHqMaterial;

	public Shader edgeBlurApplyShader;

	private Material _edgeBlurApplyMaterial;

	public Shader showAlphaChannelShader;

	private Material _showAlphaChannelMaterial;

	public float sensitivityDepth;

	public float sensitivityNormals;

	public float edgeDetectSpread;

	public float filterRadius;

	public bool showEdges;

	public int iterations;

	public EdgeBlurEffectNormals()
	{
		sensitivityDepth = 1f;
		sensitivityNormals = 1f;
		edgeDetectSpread = 0.9f;
		filterRadius = 0.8f;
		iterations = 1;
	}

	public virtual void CreateMaterials()
	{
		if (!_edgeDetectHqMaterial)
		{
			if (!CheckShader(edgeDetectHqShader))
			{
				enabled = false;
				return;
			}
			_edgeDetectHqMaterial = new Material(edgeDetectHqShader);
			_edgeDetectHqMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_edgeBlurApplyMaterial)
		{
			if (!CheckShader(edgeBlurApplyShader))
			{
				enabled = false;
				return;
			}
			_edgeBlurApplyMaterial = new Material(edgeBlurApplyShader);
			_edgeBlurApplyMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_showAlphaChannelMaterial)
		{
			if (!CheckShader(showAlphaChannelShader))
			{
				enabled = false;
				return;
			}
			_showAlphaChannelMaterial = new Material(showAlphaChannelShader);
			_showAlphaChannelMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			enabled = false;
		}
	}

	public override void Start()
	{
		CreateMaterials();
	}

	public override void OnEnable()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		Vector2 vector = new Vector2
		{
			x = sensitivityDepth,
			y = sensitivityNormals
		};
		_edgeDetectHqMaterial.SetVector("sensitivity", new Vector4(vector.x, vector.y, Mathf.Max(0.1f, edgeDetectSpread), vector.y));
		_edgeDetectHqMaterial.SetFloat("edgesOnly", 0f);
		_edgeDetectHqMaterial.SetVector("edgesOnlyBgColor", Vector4.zero);
		Graphics.Blit(source, source, _edgeDetectHqMaterial);
		if (showEdges)
		{
			Graphics.Blit(source, destination, _showAlphaChannelMaterial);
			return;
		}
		_edgeBlurApplyMaterial.SetTexture("_EdgeTex", source);
		_edgeBlurApplyMaterial.SetFloat("filterRadius", filterRadius);
		Graphics.Blit(source, destination, _edgeBlurApplyMaterial);
		int num = iterations - 1;
		if (num < 0)
		{
			num = 0;
		}
		if (num > 5)
		{
			num = 5;
		}
		while (num > 0)
		{
			Graphics.Blit(destination, source, _edgeBlurApplyMaterial);
			_edgeBlurApplyMaterial.SetTexture("_EdgeTex", source);
			_edgeBlurApplyMaterial.SetFloat("filterRadius", filterRadius);
			Graphics.Blit(source, destination, _edgeBlurApplyMaterial);
			num--;
		}
	}

	public override void Main()
	{
	}
}
