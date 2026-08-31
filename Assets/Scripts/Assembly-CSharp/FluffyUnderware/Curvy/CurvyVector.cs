using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[Serializable]
	public class CurvyVector : CurvyVectorRelative
	{
		public float MaxDistance;

		public CurvyVector()
		{
			MaxDistance = -1f;
		}

		public CurvyVector(CurvyVector org)
		{
			MaxDistance = org.MaxDistance;
			m_Position = org.m_Position;
			m_Direction = org.m_Direction;
		}

		public CurvyVector(float position, int direction)
			: this(position, direction, -1f)
		{
		}

		public CurvyVector(float position, int direction, float maxDistance)
		{
			MaxDistance = maxDistance;
			base.Direction = direction;
			base.Position = position;
		}

		public void Absolute(float maxDistance)
		{
			MaxDistance = maxDistance;
			Validate();
		}

		public void Relative()
		{
			MaxDistance = -1f;
			Validate();
		}

		public void Validate(float maxDistance)
		{
			MaxDistance = maxDistance;
			Validate();
		}

		public override void Validate()
		{
			if (MaxDistance == -1f)
			{
				base.Validate();
			}
			else
			{
				m_Position = Mathf.Clamp(m_Position, 0f, MaxDistance);
			}
		}
	}
}
