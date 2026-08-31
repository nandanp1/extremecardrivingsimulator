using UnityEngine;

public class GUICanvas
{
	private Texture2D m_texture;

	private float m_pixelsWd;

	private float m_pixelsHt;

	private float m_canvasWd;

	private float m_canvasHt;

	private float m_canvasX0;

	private float m_canvasY0;

	private float m_scaleX;

	private float m_scaleY;

	private float m_moveX;

	private float m_moveY;

	private Color32[] m_pixels;

	private Color32[] m_buffer;

	private float m_alpha = -1f;

	private bool m_changed;

	private Color[] COLORS = new Color[8]
	{
		Color.green,
		Color.yellow,
		Color.cyan,
		Color.magenta,
		Color.white,
		Color.blue,
		Color.red,
		Color.gray
	};

	private float m_lastExponent;

	private float m_lastBias = -1f;

	public GUICanvas(int pixelsWd, int pixelsHt, float canvasX0, float canvasY0, float canvasWd, float canvasHt)
	{
		InitCanvas(pixelsWd, pixelsHt, canvasX0, canvasY0, canvasWd, canvasHt);
	}

	public GUICanvas(int pixelsWd, int pixelsHt, float canvasWd, float canvasHt)
	{
		InitCanvas(pixelsWd, pixelsHt, 0f, 0f, canvasWd, canvasHt);
	}

	private void InitCanvas(int pixelsWd, int pixelsHt, float canvasX0, float canvasY0, float canvasWd, float canvasHt)
	{
		m_texture = new Texture2D(pixelsWd, pixelsHt, TextureFormat.ARGB32, false);
		m_pixelsWd = pixelsWd;
		m_pixelsHt = pixelsHt;
		m_moveX = 0f;
		m_moveY = 0f;
		m_pixels = new Color32[pixelsWd * pixelsHt];
		ResizeCanvas(canvasX0, canvasY0, canvasWd, canvasHt);
	}

	private void ResizeCanvas(float canvasX0, float canvasY0, float canvasWd, float canvasHt)
	{
		m_scaleX = m_pixelsWd / canvasWd;
		m_scaleY = m_pixelsHt / canvasHt;
		m_canvasX0 = canvasX0;
		m_canvasY0 = canvasY0;
		m_canvasWd = canvasWd;
		m_canvasHt = canvasHt;
	}

	public float CanvasWidth()
	{
		return m_canvasWd;
	}

	public float CanvasHeight()
	{
		return m_canvasHt;
	}

	public float PixelsWidth()
	{
		return m_pixelsWd;
	}

	public float PixelsHeight()
	{
		return m_pixelsHt;
	}

	public float ScaleX()
	{
		return m_scaleX;
	}

	public float ScaleY()
	{
		return m_scaleY;
	}

	private void DestroyTexture()
	{
		Object.DestroyImmediate(m_texture);
	}

	public void SetAlpha(float alpha)
	{
		m_alpha = alpha;
	}

	private void Line(float x0, float y0, float x1, float y1, Color32 col)
	{
		MoveTo(x0, y0);
		LineTo(x1, y1, col);
	}

	private void MoveTo(float x0, float y0)
	{
		m_moveX = x0;
		m_moveY = y0;
	}

	private void LineTo(float xn, float yn, Color32 col)
	{
		if (m_alpha >= 0f)
		{
			col.a = (byte)(Mathf.Clamp01(m_alpha) * 255f);
		}
		float num = m_moveX - m_canvasX0;
		float num2 = m_moveY - m_canvasY0;
		float num3 = xn - m_canvasX0;
		float num4 = yn - m_canvasY0;
		m_moveX = xn;
		m_moveY = yn;
		if (num > num3)
		{
			float num5 = num;
			num = num3;
			num3 = num5;
			num5 = num2;
			num2 = num4;
			num4 = num5;
		}
		float num6 = (num4 - num2) / (num3 - num);
		if (num < 0f)
		{
			num2 -= num * num6;
			num = 0f;
		}
		if (num3 > m_canvasWd)
		{
			num4 -= (num3 - m_canvasWd) * num6;
			num3 = m_canvasWd;
		}
		if (!(num > m_canvasWd) && !(num3 < 0f) && (!(num2 < 0f) || !(num4 < 0f)) && (!(num2 > m_canvasHt) || !(num4 > m_canvasHt)))
		{
			if (num2 < 0f)
			{
				num -= num2 / num6;
				num2 = 0f;
			}
			if (num2 > m_canvasHt)
			{
				num -= (num2 - m_canvasHt) / num6;
				num2 = m_canvasHt;
			}
			if (num4 < 0f)
			{
				num3 -= num4 / num6;
				num4 = 0f;
			}
			if (num4 > m_canvasHt)
			{
				num3 -= (num4 - m_canvasHt) / num6;
				num4 = m_canvasHt;
			}
			TexLine((int)(num * m_scaleX), (int)(num2 * m_scaleY), (int)(num3 * m_scaleX), (int)(num4 * m_scaleY), col);
			m_changed = true;
		}
	}

	public void Line(Vector2 P0, Vector2 P1, Color32 col)
	{
		MoveTo(P0.x, P0.y);
		LineTo(P1.x, P1.y, col);
	}

	private void MoveTo(Vector2 P0)
	{
		MoveTo(P0.x, P0.y);
	}

	private void LineTo(Vector2 P1, Color32 col)
	{
		LineTo(P1.x, P1.y, col);
	}

	public void LineH(float y, Color32 col)
	{
		Line(m_canvasX0, y, m_canvasX0 + m_canvasWd, y, col);
	}

	public void LineV(float x, Color32 col)
	{
		Line(x, m_canvasY0, x, m_canvasY0 + m_canvasHt, col);
	}

	private void Circle(float x, float y, float radius, Color32 col)
	{
		if (m_alpha >= 0f)
		{
			col.a = (byte)(Mathf.Clamp01(m_alpha) * 255f);
		}
		TexCircle((int)((x - m_canvasX0) * m_scaleX), (int)((y - m_canvasY0) * m_scaleY), (int)(radius * m_scaleX), col);
		m_changed = true;
	}

	public void Clear(Color32 col)
	{
		float num = m_pixelsWd * m_pixelsHt;
		if (m_alpha >= 0f)
		{
			col.a = (byte)(Mathf.Clamp01(m_alpha) * 255f);
		}
		for (int i = 0; (float)i < num; i++)
		{
			m_pixels[i] = col;
		}
		m_changed = true;
	}

	private void Fill(float x, float y, float width, float height, Color32 col)
	{
		int num = (int)((x - m_canvasX0) * m_scaleX);
		int num2 = (int)((y - m_canvasY0) * m_scaleY);
		int num3 = (int)((float)num + width * m_scaleX);
		int num4 = (int)((float)num2 + height * m_scaleY);
		if (num3 <= num || num4 <= num2)
		{
			return;
		}
		if (m_alpha >= 0f)
		{
			col.a = (byte)(Mathf.Clamp01(m_alpha) * 255f);
		}
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				TexSetPixel(j, i, col);
			}
		}
		m_changed = true;
	}

	private void SpLine(Vector2 P0, Vector2 T0, Vector2 P1, Vector2 T1, Color32 col)
	{
		SpLine(P0, T0, P1, T1, col, 20);
	}

	private void SpLine(Vector2 P0, Vector2 T0, Vector2 P1, Vector2 T1, Color32 col, int steps)
	{
		SpLine(P0, T0, P1, T1, col, steps, 1f);
	}

	private void SpLine(Vector2 P0, Vector2 T0, Vector2 P1, Vector2 T1, Color32 col, int steps, float scaleY)
	{
		MoveTo(P0.x, P0.y * scaleY);
		for (int i = 0; i <= steps; i++)
		{
			float num = i;
			num /= (float)steps;
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = 2f * num3 - 3f * num2 + 1f;
			float num5 = -2f * num3 + 3f * num2;
			float num6 = num3 - 2f * num2 + num;
			float num7 = num3 - num2;
			Vector2 vector = num4 * P0 + num5 * P1 + num6 * T0 + num7 * T1;
			LineTo(vector.x, vector.y * scaleY, col);
		}
	}

	public void Grid(float stepX, float stepY, Color32 col)
	{
		float num = (float)Mathf.FloorToInt(m_canvasX0 / stepX) * stepX;
		float num2 = (float)Mathf.FloorToInt(m_canvasY0 / stepY) * stepY;
		for (float num3 = num; num3 <= m_canvasX0 + m_canvasWd; num3 += stepX)
		{
			LineV(num3, col);
		}
		for (float num3 = num2; num3 <= m_canvasY0 + m_canvasHt; num3 += stepY)
		{
			LineH(num3, col);
		}
	}

	public void FrictionCurve(float Slip0, float Value0, float Slip1, float Value1, float Slope, Color col)
	{
		Vector2 vector = new Vector2(Slip0, Value0);
		Vector2 p = new Vector2(Slip1, Value1);
		float num = Value0 / Slip0;
		SpLine(Vector2.zero, new Vector2(Slip0, 0f), vector, vector, col);
		SpLine(vector, new Vector2(Slip1 - Slip0, num * Slip1 - Value0) * (1f + (Slip1 - Slip0) / (Slip1 - Slip0 + 1f)) * CarWheelFriction.MCt0, p, new Vector2(Slip1 - Slip0, 0f) * CarWheelFriction.MCt1, col);
		Line(Slip1, Value1, m_canvasWd, Value1 + (m_canvasWd - Slip1) * Slope, col);
	}

	public void BiasCurve(Vector2 P0, Vector2 P1, float bias, Color32 col)
	{
		int num = 20;
		float num2 = P1.x - P0.x;
		float num3 = P1.y - P0.y;
		MoveTo(P0);
		for (int i = 0; i <= num; i++)
		{
			float num4 = i;
			num4 /= (float)num;
			float num5 = Bias(num4, bias);
			LineTo(P0.x + num2 * num4, P0.y + num3 * num5, col);
		}
	}

	public void LineGraph(float[] Values, int ValueSize)
	{
		if (ValueSize < 2 || Values.Length < 2 * ValueSize)
		{
			return;
		}
		for (int i = 1; i < ValueSize; i++)
		{
			MoveTo(Values[0], Values[i]);
			for (int j = 1; j < Values.Length / ValueSize; j++)
			{
				LineTo(Values[j * ValueSize], Values[j * ValueSize + i], COLORS[(i - 1) % 8]);
			}
		}
	}

	public void Save()
	{
		if (m_buffer == null)
		{
			m_buffer = m_pixels.Clone() as Color32[];
		}
		else
		{
			m_pixels.CopyTo(m_buffer, 0);
		}
	}

	public void Restore()
	{
		if (m_buffer != null)
		{
			m_buffer.CopyTo(m_pixels, 0);
			m_changed = true;
		}
	}

	private void ApplyChanges()
	{
		if (m_changed)
		{
			m_texture.SetPixels32(m_pixels);
			m_texture.Apply(false);
			m_changed = false;
		}
	}

	public void GUIDraw(int posX, int posY)
	{
		ApplyChanges();
		GUI.DrawTexture(new Rect(posX, posY, m_pixelsWd, m_pixelsHt), m_texture);
	}

	public void GUIStretchDraw(int posX, int posY, int width, int height)
	{
		ApplyChanges();
		GUI.DrawTexture(new Rect(posX, posY, width, height), m_texture);
	}

	private void GUIStretchDrawWidth(int posX, int posY, int width)
	{
		ApplyChanges();
		int num = (int)(m_pixelsHt / m_pixelsWd);
		GUI.DrawTexture(new Rect(posX, posY, width, width * num), m_texture);
	}

	private Texture GetTexture()
	{
		ApplyChanges();
		return m_texture;
	}

	private float BiasRaw(float x, float fBias)
	{
		if (x <= 0f)
		{
			return 0f;
		}
		if (x >= 1f)
		{
			return 1f;
		}
		if (fBias != m_lastBias)
		{
			if (fBias <= 0f)
			{
				return (!(x >= 1f)) ? 0f : 1f;
			}
			if (fBias >= 1f)
			{
				return (!(x > 0f)) ? 0f : 1f;
			}
			if (fBias == 0.5f)
			{
				return x;
			}
			m_lastExponent = Mathf.Log(fBias) * -1.4427f;
			m_lastBias = fBias;
		}
		return Mathf.Pow(x, m_lastExponent);
	}

	private float Bias(float x, float fBias)
	{
		float num = ((!(fBias <= 0.5f)) ? (1f - BiasRaw(1f - Mathf.Abs(x), 1f - fBias)) : BiasRaw(Mathf.Abs(x), fBias));
		return (!(x < 0f)) ? num : (0f - num);
	}

	private void TexSetPixel(int x, int y, Color32 col)
	{
		if (x >= 0 && (float)x < m_pixelsWd && y >= 0 && (float)y < m_pixelsHt)
		{
			m_pixels[y * (int)m_pixelsWd + x] = col;
		}
	}

	private void TexLine(int x0, int y0, int x1, int y1, Color32 col)
	{
		int num = y1 - y0;
		int num2 = x1 - x0;
		int num3;
		if (num < 0)
		{
			num = -num;
			num3 = -1;
		}
		else
		{
			num3 = 1;
		}
		int num4;
		if (num2 < 0)
		{
			num2 = -num2;
			num4 = -1;
		}
		else
		{
			num4 = 1;
		}
		num <<= 1;
		num2 <<= 1;
		TexSetPixel(x0, y0, col);
		int num5;
		if (num2 > num)
		{
			num5 = num - (num2 >> 1);
			while (x0 != x1)
			{
				if (num5 >= 0)
				{
					y0 += num3;
					num5 -= num2;
				}
				x0 += num4;
				num5 += num;
				TexSetPixel(x0, y0, col);
			}
			return;
		}
		num5 = num2 - (num >> 1);
		while (y0 != y1)
		{
			if (num5 >= 0)
			{
				x0 += num4;
				num5 -= num;
			}
			y0 += num3;
			num5 += num2;
			TexSetPixel(x0, y0, col);
		}
	}

	private void TexCircle(int cx, int cy, int r, Color32 col)
	{
		int num = r;
		int num2 = -r;
		int num3 = (int)Mathf.Ceil((float)r / Mathf.Sqrt(2f));
		for (int i = 0; i <= num3; i++)
		{
			TexSetPixel(cx + i, cy + num, col);
			TexSetPixel(cx + i, cy - num, col);
			TexSetPixel(cx - i, cy + num, col);
			TexSetPixel(cx - i, cy - num, col);
			TexSetPixel(cx + num, cy + i, col);
			TexSetPixel(cx - num, cy + i, col);
			TexSetPixel(cx + num, cy - i, col);
			TexSetPixel(cx - num, cy - i, col);
			num2 += 2 * i + 1;
			if (num2 > 0)
			{
				num2 += 2 - 2 * num--;
			}
		}
	}
}
