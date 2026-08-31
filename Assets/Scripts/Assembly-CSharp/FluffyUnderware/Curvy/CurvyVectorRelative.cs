using System;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[Serializable]
	public class CurvyVectorRelative
	{
		[SerializeField]
		internal float m_Position;

		[SerializeField]
		internal int m_Direction = 1;

		public float Position
		{
			get
			{
				return m_Position;
			}
			set
			{
				if (m_Position != value)
				{
					m_Position = value;
					Validate();
				}
			}
		}

		public int Direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if (value != m_Direction)
				{
					if (value > 0)
					{
						m_Direction = 1;
					}
					else
					{
						m_Direction = -1;
					}
				}
			}
		}

		public CurvyVectorRelative()
		{
		}

		public CurvyVectorRelative(CurvyVectorRelative org)
		{
			m_Position = org.m_Position;
			m_Direction = org.m_Direction;
		}

		public CurvyVectorRelative(float position, int direction)
		{
			Direction = direction;
			Position = position;
		}

		public virtual void Validate()
		{
			m_Position = Mathf.Clamp01(m_Position);
		}
	}
}
