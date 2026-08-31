using UnityEngine;

public class GLCurvyRenderer : MonoBehaviour
{
	public CurvySplineBase[] Splines;

	public Color[] Colors;

	private Vector3[] Points;

	private Material lineMaterial;

	private void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void OnPostRender()
	{
		if (Splines.Length == 0)
		{
			return;
		}
		for (int i = 0; i < Splines.Length; i++)
		{
			Color lineColor = ((i >= Colors.Length) ? Color.green : Colors[i]);
			if (Splines[i] is CurvySpline)
			{
				RenderSpline(Splines[i], lineColor);
			}
			else
			{
				if (!(Splines[i] is CurvySplineGroup))
				{
					continue;
				}
				CurvySplineGroup curvySplineGroup = Splines[i] as CurvySplineGroup;
				foreach (CurvySpline spline in curvySplineGroup.Splines)
				{
					RenderSpline(spline, lineColor);
				}
			}
		}
	}

	private void RenderSpline(CurvySplineBase spl, Color lineColor)
	{
		if ((bool)spl && spl.IsInitialized)
		{
			Points = spl.GetApproximation();
			CreateLineMaterial();
			lineMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(lineColor);
			for (int i = 1; i < Points.Length; i++)
			{
				GL.Vertex(Points[i - 1]);
				GL.Vertex(Points[i]);
			}
			GL.End();
		}
	}
}
