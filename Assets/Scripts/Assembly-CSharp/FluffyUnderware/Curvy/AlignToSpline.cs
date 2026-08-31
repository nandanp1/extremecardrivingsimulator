using System;
using System.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class AlignToSpline : CurvyComponent
	{
		public enum AlignMode
		{
			Relative = 0,
			Absolute = 1
		}

		[Label(Tooltip = "The Spline or Splinegroup to use")]
		[SerializeField]
		private CurvySplineBase m_Spline;

		[SerializeField]
		private AlignMode m_Mode;

		[Label(Tooltip = "Interpolate cached values?")]
		public bool FastInterpolation;

		[SerializeField]
		private float m_Position;

		[Label(Tooltip = "Align to Up-Vector?")]
		public bool SetOrientation = true;

		[Label(Text = "Use 2D Orientation", Tooltip = "Use 2D Orientation (along z-axis only)?")]
		public bool Use2DOrientation;

		[Label(Tooltip = "Enable to recalculate position when spline segments are added/removed during runtime")]
		[SerializeField]
		private bool m_Dynamic = true;

		public CurvyComponentEvent OnNullSegment;

		private bool mIsInitialized;

		private WeakReference mCurrentSeg;

		protected float mCurrentSegmentF;

		protected float mCurrentTF;

		private bool mNeedRefresh;

		public CurvySplineBase Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					if ((bool)m_Spline)
					{
						m_Spline.OnRefresh -= OnSplineRefresh;
					}
					m_Spline = value;
					doUpdate();
				}
			}
		}

		public AlignMode Mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				if (m_Mode != value)
				{
					m_Mode = value;
					doUpdate();
				}
			}
		}

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
					doUpdate();
				}
			}
		}

		public bool Dynamic
		{
			get
			{
				return m_Dynamic;
			}
			set
			{
				if (m_Dynamic != value)
				{
					m_Dynamic = value;
					doUpdate();
				}
			}
		}

		public CurvySplineSegment CurrentSegment
		{
			get
			{
				if (Dynamic)
				{
					return mCurrentSegment;
				}
				return Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
			}
		}

		public float CurrentSegmentF
		{
			get
			{
				return mCurrentSegmentF;
			}
		}

		public float CurrentTF
		{
			get
			{
				return mCurrentTF;
			}
		}

		protected CurvySplineSegment mCurrentSegment
		{
			get
			{
				return (mCurrentSeg != null) ? ((CurvySplineSegment)mCurrentSeg.Target) : null;
			}
			set
			{
				if (mCurrentSeg == null)
				{
					mCurrentSeg = new WeakReference(value);
				}
				else
				{
					mCurrentSeg.Target = value;
				}
			}
		}

		protected virtual bool SourceIsInitialized
		{
			get
			{
				return (bool)Spline && Spline.IsInitialized;
			}
		}

		private IEnumerator Start()
		{
			if ((bool)Spline)
			{
				while (!SourceIsInitialized)
				{
					yield return 0;
				}
				doUpdate();
			}
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				doUpdate();
			}
		}

		private void Update()
		{
			if ((UpdateIn == CurvyUpdateMethod.Update && mNeedRefresh) || !Application.isPlaying)
			{
				doUpdate();
			}
		}

		private void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate && mNeedRefresh)
			{
				doUpdate();
			}
		}

		private void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate && mNeedRefresh)
			{
				doUpdate();
			}
		}

		private void OnValidate()
		{
			doUpdate();
		}

		public void Refresh()
		{
			doUpdate();
		}

		protected virtual void Validate()
		{
			switch (Mode)
			{
			case AlignMode.Relative:
				m_Position = Mathf.Clamp01(m_Position);
				mCurrentTF = m_Position;
				break;
			case AlignMode.Absolute:
				m_Position = Mathf.Clamp(m_Position, 0f, Spline.Length);
				mCurrentTF = Spline.DistanceToTF(m_Position);
				break;
			}
			if (Dynamic || (!Dynamic && mCurrentSeg == null) || !Application.isPlaying)
			{
				mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
			}
		}

		protected virtual void DoRefresh()
		{
			if (mCurrentSegment == null)
			{
				if (OnNullSegment != null)
				{
					OnNullSegment(this);
				}
				base.gameObject.SetActive(false);
				return;
			}
			if (FastInterpolation)
			{
				base.Transform.position = mCurrentSegment.InterpolateFast(mCurrentSegmentF);
			}
			else
			{
				base.Transform.position = mCurrentSegment.Interpolate(mCurrentSegmentF);
			}
			if (SetOrientation)
			{
				if (Use2DOrientation)
				{
					base.Transform.rotation = Quaternion.LookRotation(Vector3.forward, mCurrentSegment.GetTangentFast(mCurrentSegmentF));
				}
				else
				{
					base.Transform.rotation = mCurrentSegment.GetOrientationFast(mCurrentSegmentF);
				}
			}
			mNeedRefresh = false;
		}

		private void doUpdate()
		{
			if (SourceIsInitialized)
			{
				m_Spline.OnRefresh -= OnSplineRefresh;
				m_Spline.OnRefresh += OnSplineRefresh;
				Validate();
				DoRefresh();
			}
		}

		private void OnSplineRefresh(CurvySplineBase sender)
		{
			mNeedRefresh = true;
		}
	}
}
