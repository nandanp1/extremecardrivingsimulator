using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	internal struct CurvyMeshSegmentInfo
	{
		public CurvySplineBase Target;

		public float TF;

		private float mDistance;

		public Matrix4x4 Matrix;

		public float Distance
		{
			get
			{
				return (mDistance != -1f) ? mDistance : Target.TFToDistance(TF);
			}
		}

		public CurvyMeshSegmentInfo(SplinePathMeshBuilder mb, float tf, Vector3 scale)
		{
			Target = mb.Spline;
			TF = tf;
			mDistance = -1f;
			Vector3 position = ((!mb.FastInterpolation) ? Target.Interpolate(TF) : Target.InterpolateFast(TF));
			if (mb.UseWorldPosition)
			{
				Matrix = Matrix4x4.TRS(mb.Transform.InverseTransformPoint(position), Target.GetOrientationFast(TF), scale);
			}
			else
			{
				Matrix = Matrix4x4.TRS(Target.Transform.InverseTransformPoint(position), Target.GetOrientationFast(TF), scale);
			}
		}

		public CurvyMeshSegmentInfo(SplinePathMeshBuilder mb, float tf, float distance, Vector3 scale)
		{
			Target = mb.Spline;
			TF = tf;
			mDistance = distance;
			Vector3 position = ((!mb.FastInterpolation) ? Target.Interpolate(TF) : Target.InterpolateFast(TF));
			if (mb.UseWorldPosition)
			{
				Matrix = Matrix4x4.TRS(mb.Transform.InverseTransformPoint(position), Target.GetOrientationFast(TF), scale);
			}
			else
			{
				Matrix = Matrix4x4.TRS(Target.Transform.InverseTransformPoint(position), Target.GetOrientationFast(TF), scale);
			}
		}
	}
}
