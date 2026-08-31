using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Image Effects/Depth of Field")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DepthOfField : PostEffectsBase
{
	public Shader weightedBlurShader;

	private Material _weightedBlurMaterial;

	public Shader preDofShader;

	private Material _preDofMaterial;

	public Shader preDofZReadShader;

	private Material _preDofZReadMaterial;

	public Shader dofShader;

	private Material _dofMaterial;

	public Shader blurShader;

	private Material _blurMaterial;

	public Shader foregroundShader;

	private Material _foregroundBlurMaterial;

	public Shader depthConvertShader;

	private Material _depthConvertMaterial;

	public Shader depthBlurShader;

	private Material _depthBlurMaterial;

	public Shader recordCenterDepthShader;

	private Material _recordCenterDepthMaterial;

	private RenderTexture _onePixel;

	public DofResolutionSetting resolution;

	public DofQualitySetting quality;

	public float focalZDistance;

	public float focalZStart;

	public float focalZEnd;

	private float _focalDistance01;

	private float _focalStart01;

	private float _focalEnd01;

	public float focalFalloff;

	public Transform focusOnThis;

	public bool focusOnScreenCenterDepth;

	public float focalSize;

	public float focalChangeSpeed;

	public int blurIterations;

	public int foregroundBlurIterations;

	public float blurSpread;

	public float foregroundBlurSpread;

	public float foregroundBlurStrength;

	public float foregroundBlurThreshhold;

	public DepthOfField()
	{
		resolution = DofResolutionSetting.Normal;
		quality = DofQualitySetting.High;
		focalZEnd = 10000f;
		_focalEnd01 = 1f;
		focalFalloff = 1f;
		focalSize = 0.075f;
		focalChangeSpeed = 2.275f;
		blurIterations = 2;
		foregroundBlurIterations = 2;
		blurSpread = 1.25f;
		foregroundBlurSpread = 1.75f;
		foregroundBlurStrength = 1.15f;
		foregroundBlurThreshhold = 0.002f;
	}

	public virtual void CreateMaterials()
	{
		if (!_onePixel)
		{
			_onePixel = new RenderTexture(1, 1, 0);
		}
		_onePixel.hideFlags = HideFlags.HideAndDontSave;
		if (!_recordCenterDepthMaterial)
		{
			if (!CheckShader(recordCenterDepthShader))
			{
				enabled = false;
				return;
			}
			_recordCenterDepthMaterial = new Material(recordCenterDepthShader);
			_recordCenterDepthMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_weightedBlurMaterial)
		{
			if (!CheckShader(blurShader))
			{
				enabled = false;
				return;
			}
			_weightedBlurMaterial = new Material(weightedBlurShader);
			_weightedBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_blurMaterial)
		{
			if (!CheckShader(blurShader))
			{
				enabled = false;
				return;
			}
			_blurMaterial = new Material(blurShader);
			_blurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_dofMaterial)
		{
			if (!CheckShader(dofShader))
			{
				enabled = false;
				return;
			}
			_dofMaterial = new Material(dofShader);
			_dofMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_preDofMaterial)
		{
			if (!CheckShader(preDofShader))
			{
				enabled = false;
				return;
			}
			_preDofMaterial = new Material(preDofShader);
			_preDofMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_preDofZReadMaterial)
		{
			if (!CheckShader(preDofZReadShader))
			{
				enabled = false;
				return;
			}
			_preDofZReadMaterial = new Material(preDofZReadShader);
			_preDofZReadMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_foregroundBlurMaterial)
		{
			if (!CheckShader(foregroundShader))
			{
				enabled = false;
				return;
			}
			_foregroundBlurMaterial = new Material(foregroundShader);
			_foregroundBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_depthConvertMaterial)
		{
			if (!CheckShader(depthConvertShader))
			{
				enabled = false;
				return;
			}
			_depthConvertMaterial = new Material(depthConvertShader);
			_depthConvertMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		if (!_depthBlurMaterial)
		{
			if (!CheckShader(depthBlurShader))
			{
				enabled = false;
				return;
			}
			_depthBlurMaterial = new Material(depthBlurShader);
			_depthBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
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
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateMaterials();
		if (foregroundBlurIterations < 1)
		{
			foregroundBlurIterations = 1;
		}
		if (blurIterations < 1)
		{
			blurIterations = 1;
		}
		float num = 4f;
		if (resolution == DofResolutionSetting.Normal)
		{
			num = 2f;
		}
		else if (resolution == DofResolutionSetting.High)
		{
			num = 1f;
		}
		Material material = _preDofMaterial;
		if ((bool)focusOnThis)
		{
			Vector3 vector = GetComponent<Camera>().WorldToViewportPoint(focusOnThis.position);
			vector.z /= GetComponent<Camera>().farClipPlane;
			_focalDistance01 = vector.z;
		}
		else if (focusOnScreenCenterDepth)
		{
			material = _preDofZReadMaterial;
			_recordCenterDepthMaterial.SetFloat("deltaTime", Time.deltaTime * focalChangeSpeed);
			Graphics.Blit(_onePixel, _onePixel, _recordCenterDepthMaterial);
		}
		else
		{
			_focalDistance01 = GetComponent<Camera>().WorldToViewportPoint(focalZDistance * GetComponent<Camera>().transform.forward + GetComponent<Camera>().transform.position).z / GetComponent<Camera>().farClipPlane;
		}
		if (!(focalZEnd <= GetComponent<Camera>().farClipPlane))
		{
			focalZEnd = GetComponent<Camera>().farClipPlane;
		}
		_focalStart01 = GetComponent<Camera>().WorldToViewportPoint(focalZStart * GetComponent<Camera>().transform.forward + GetComponent<Camera>().transform.position).z / GetComponent<Camera>().farClipPlane;
		_focalEnd01 = GetComponent<Camera>().WorldToViewportPoint(focalZEnd * GetComponent<Camera>().transform.forward + GetComponent<Camera>().transform.position).z / GetComponent<Camera>().farClipPlane;
		if (!(_focalEnd01 >= _focalStart01))
		{
			_focalEnd01 = _focalStart01 + float.Epsilon;
		}
		material.SetFloat("focalDistance01", _focalDistance01);
		material.SetFloat("focalFalloff", focalFalloff);
		material.SetFloat("focalStart01", _focalStart01);
		material.SetFloat("focalEnd01", _focalEnd01);
		material.SetFloat("focalSize", focalSize * 0.5f);
		material.SetTexture("_onePixelTex", _onePixel);
		Graphics.Blit(source, source, material);
		RenderTexture temporary = RenderTexture.GetTemporary((int)((float)source.width / num), (int)((float)source.height / num), 0);
		RenderTexture temporary2 = RenderTexture.GetTemporary((int)((float)source.width / num), (int)((float)source.height / num), 0);
		RenderTexture temporary3 = RenderTexture.GetTemporary((int)((float)source.width / num), (int)((float)source.height / num), 0);
		if (quality >= DofQualitySetting.High)
		{
			temporary.filterMode = FilterMode.Point;
			temporary2.filterMode = FilterMode.Point;
			temporary3.filterMode = FilterMode.Point;
			source.filterMode = FilterMode.Point;
			Graphics.Blit(source, temporary2, _depthConvertMaterial);
			source.filterMode = FilterMode.Bilinear;
			for (int i = 0; i < foregroundBlurIterations; i++)
			{
				_depthBlurMaterial.SetVector("offsets", new Vector4(0f, foregroundBlurSpread / (float)temporary2.height, 0f, _focalDistance01));
				Graphics.Blit(temporary2, temporary, _depthBlurMaterial);
				_depthBlurMaterial.SetVector("offsets", new Vector4(foregroundBlurSpread / (float)temporary2.width, 0f, 0f, _focalDistance01));
				Graphics.Blit(temporary, temporary2, _depthBlurMaterial);
			}
			temporary.filterMode = FilterMode.Bilinear;
			temporary2.filterMode = FilterMode.Bilinear;
			temporary3.filterMode = FilterMode.Bilinear;
			int num2 = blurIterations;
			if (resolution != DofResolutionSetting.High)
			{
				Graphics.Blit(source, temporary);
			}
			else
			{
				_blurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, 0f));
				Graphics.Blit(source, temporary3, _blurMaterial);
				_blurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, 0f));
				Graphics.Blit(temporary3, temporary, _blurMaterial);
				num2--;
			}
			for (int i = 0; i < num2; i++)
			{
				_blurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, 0f));
				Graphics.Blit(temporary, temporary3, _blurMaterial);
				_blurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, 0f));
				Graphics.Blit(temporary3, temporary, _blurMaterial);
			}
			temporary2.filterMode = FilterMode.Point;
			_foregroundBlurMaterial.SetFloat("focalFalloff", focalFalloff);
			_foregroundBlurMaterial.SetFloat("focalDistance01", _focalDistance01);
			_foregroundBlurMaterial.SetFloat("focalStart01", _focalStart01);
			_foregroundBlurMaterial.SetFloat("focalEnd01", _focalEnd01);
			_foregroundBlurMaterial.SetFloat("foregroundBlurStrength", foregroundBlurStrength);
			_foregroundBlurMaterial.SetFloat("foregroundBlurThreshhold", foregroundBlurThreshhold);
			_foregroundBlurMaterial.SetTexture("_SourceTex", source);
			_foregroundBlurMaterial.SetTexture("_BlurredCoc", temporary);
			Graphics.Blit(temporary2, source, _foregroundBlurMaterial);
			temporary.filterMode = FilterMode.Bilinear;
			temporary2.filterMode = FilterMode.Bilinear;
			temporary3.filterMode = FilterMode.Bilinear;
		}
		int num3 = blurIterations;
		if (resolution != DofResolutionSetting.High)
		{
			Graphics.Blit(source, temporary2);
		}
		else
		{
			if (quality >= DofQualitySetting.Medium)
			{
				_weightedBlurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, _focalDistance01));
				Graphics.Blit(source, temporary3, _weightedBlurMaterial);
				_weightedBlurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, _focalDistance01));
				Graphics.Blit(temporary3, temporary2, _weightedBlurMaterial);
			}
			else
			{
				_blurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, 0f));
				Graphics.Blit(source, temporary3, _blurMaterial);
				_blurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, 0f));
				Graphics.Blit(temporary3, temporary2, _blurMaterial);
			}
			num3--;
		}
		for (int i = 0; i < num3; i++)
		{
			if (quality >= DofQualitySetting.Medium)
			{
				_weightedBlurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, _focalDistance01));
				Graphics.Blit(temporary2, temporary3, _weightedBlurMaterial);
				_weightedBlurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, _focalDistance01));
				Graphics.Blit(temporary3, temporary2, _weightedBlurMaterial);
			}
			else
			{
				_blurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / (float)temporary2.height, 0f, 0f));
				Graphics.Blit(temporary2, temporary3, _blurMaterial);
				_blurMaterial.SetVector("offsets", new Vector4(blurSpread / (float)temporary2.width, 0f, 0f, 0f));
				Graphics.Blit(temporary3, temporary2, _blurMaterial);
			}
		}
		_dofMaterial.SetFloat("focalDistance01", _focalDistance01);
		_dofMaterial.SetTexture("_LowRez", temporary2);
		Graphics.Blit(source, destination, _dofMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	public override void Main()
	{
	}
}
