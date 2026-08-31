using System.Collections.Generic;
using UnityEngine;

public class UIColorPicker : UIWidgetContainer
{
	public Transform zeroReference;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	[SerializeField]
	private bool circular;

	[SerializeField]
	private UITexture colorSpectrum;

	[SerializeField]
	private UISprite thumb;

	public static UIColorPicker current;

	[SerializeField]
	[HideInInspector]
	protected Color mValue = Color.white;

	public Color value
	{
		get
		{
			return mValue;
		}
		set
		{
			if (mValue != value)
			{
				Color color = this.value;
				mValue = value;
				if (color != this.value && NGUITools.GetActive(this) && EventDelegate.IsValid(onChange))
				{
					current = this;
					EventDelegate.Execute(onChange);
					current = null;
				}
			}
		}
	}

	private void Start()
	{
		value = GetColor();
	}

	private void OnPress(bool pressed)
	{
		value = GetColor();
		UpdateThumbPosition();
	}

	private void OnDrag(Vector2 delta)
	{
		value = GetColor();
		UpdateThumbPosition();
	}

	private Color GetColor()
	{
		Vector2 vector = UICamera.mainCamera.WorldToScreenPoint(colorSpectrum.transform.position);
		Vector2 vector2 = UICamera.mainCamera.WorldToScreenPoint(thumb.transform.position);
		Texture2D texture2D = colorSpectrum.mainTexture as Texture2D;
		Vector2 vector3 = UICamera.mainCamera.WorldToScreenPoint(zeroReference.position);
		Vector2 vector4 = vector;
		float x = vector3.x;
		float y = vector3.y;
		float num = (x - vector4.x) * 2f;
		float num2 = (y - vector4.y) * 2f;
		float num3 = (float)texture2D.width / num;
		float num4 = (float)texture2D.height / num2;
		Vector2 vector5 = new Vector2(vector2.x, vector2.y) - vector;
		vector5.x *= 0f - num3;
		vector5.y *= num4;
		Vector2 vector6 = vector5 + new Vector2(texture2D.width, texture2D.height) / 2f;
		vector6.x /= texture2D.width;
		vector6.y /= texture2D.height;
		return texture2D.GetPixelBilinear(vector6.x, vector6.y);
	}

	private void UpdateThumbPosition()
	{
		Vector2 lastTouchPosition = UICamera.lastTouchPosition;
		lastTouchPosition.x = Mathf.Clamp01(lastTouchPosition.x / (float)Screen.width);
		lastTouchPosition.y = Mathf.Clamp01(lastTouchPosition.y / (float)Screen.height);
		thumb.transform.position = UICamera.currentCamera.ViewportToWorldPoint(lastTouchPosition);
		if (circular)
		{
			float magnitude = thumb.transform.localPosition.magnitude;
			if (magnitude > colorSpectrum.localSize.x * 0.5f - thumb.localSize.x * 0.5f)
			{
				thumb.transform.localPosition = Vector3.ClampMagnitude(thumb.transform.localPosition, colorSpectrum.localSize.x * 0.5f - thumb.localSize.x * 0.5f);
			}
		}
		else
		{
			thumb.transform.localPosition = new Vector2(Mathf.Clamp(thumb.transform.localPosition.x, (0f - colorSpectrum.localSize.x) * 0.5f + thumb.localSize.x * 0.5f, colorSpectrum.localSize.x * 0.5f - thumb.localSize.x * 0.5f), Mathf.Clamp(thumb.transform.localPosition.y, (0f - colorSpectrum.localSize.y) * 0.5f + thumb.localSize.y * 0.5f, colorSpectrum.localSize.y * 0.5f - thumb.localSize.y * 0.5f));
		}
	}
}
