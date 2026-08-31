using UnityEngine;

public class CurvyShaperDefinition : ScriptableObject
{
	public string Name;

	public int Resolution;

	public float Range = 1f;

	public float Radius;

	public SplineShaper.ModifierMode RadiusModifier;

	public AnimationCurve RadiusModifierCurve;

	public float Z;

	public SplineShaper.ModifierMode ZModifier;

	public AnimationCurve ZModifierCurve;

	public float m;

	public float n1;

	public float n2;

	public float n3;

	public float a;

	public float b;

	public static CurvyShaperDefinition Create()
	{
		return ScriptableObject.CreateInstance<CurvyShaperDefinition>();
	}

	public static CurvyShaperDefinition Create(SplineShaper shape)
	{
		CurvyShaperDefinition curvyShaperDefinition = Create();
		curvyShaperDefinition.Name = shape.Name;
		curvyShaperDefinition.Resolution = shape.Resolution;
		curvyShaperDefinition.Range = shape.Range;
		curvyShaperDefinition.Radius = shape.Radius;
		curvyShaperDefinition.RadiusModifier = shape.RadiusModifier;
		curvyShaperDefinition.RadiusModifierCurve = shape.RadiusModifierCurve;
		curvyShaperDefinition.Z = shape.Z;
		curvyShaperDefinition.ZModifier = shape.ZModifier;
		curvyShaperDefinition.ZModifierCurve = shape.ZModifierCurve;
		curvyShaperDefinition.m = shape.m;
		curvyShaperDefinition.n1 = shape.n1;
		curvyShaperDefinition.n2 = shape.n2;
		curvyShaperDefinition.n3 = shape.n3;
		curvyShaperDefinition.a = shape.a;
		curvyShaperDefinition.b = shape.b;
		return curvyShaperDefinition;
	}

	public void LoadInto(SplineShaper shape, bool loadGeneral)
	{
		if (loadGeneral)
		{
			shape.Name = Name;
			shape.Resolution = Resolution;
			shape.Range = Range;
			shape.Radius = Radius;
			shape.RadiusModifier = RadiusModifier;
			shape.RadiusModifierCurve = RadiusModifierCurve;
			shape.Z = Z;
			shape.ZModifier = ZModifier;
			shape.ZModifierCurve = ZModifierCurve;
		}
		shape.m = m;
		shape.n1 = n1;
		shape.n2 = n2;
		shape.n3 = n3;
		shape.a = a;
		shape.b = b;
	}
}
