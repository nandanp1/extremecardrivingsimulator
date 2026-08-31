using System;
using System.Collections;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class FollowSpline : CurvyComponent
	{
		public enum FollowMode
		{
			Relative = 0,
			AbsoluteExtrapolate = 1,
			AbsolutePrecise = 2
		}

		protected delegate void Action();

		[Label(Tooltip = "The Spline or Splinegroup to use")]
		[SerializeField]
		private CurvySplineBase m_Spline;

		[Label(Tooltip = "Interpolate cached values?")]
		public bool FastInterpolation;

		[SerializeField]
		[Label(Tooltip = "Movement Mode")]
		private FollowMode m_Mode;

		[SerializeField]
		private CurvyVector m_Initial = new CurvyVector();

		[Positive(Tooltip = "Speed in F or World Units (depending on Mode)")]
		public float Speed;

		[Label(Tooltip = "End of Spline Behaviour")]
		public CurvyClamping Clamping;

		[Label(Tooltip = "Align to Up-Vector?")]
		public bool SetOrientation = true;

		[Label(Text = "Use 2D Orientation", Tooltip = "Use 2D Orientation (along z-axis only)?")]
		public bool Use2DOrientation;

		[Label(Tooltip = "Enable if you plan to add/remove segments during movement")]
		[SerializeField]
		private bool m_Dynamic;

		private bool mIsInitialized;

		private CurvyVector Current;

		private WeakReference mCurrentSeg;

		protected float mCurrentSegmentF = -1f;

		protected float mCurrentTF;

		protected Action UpdateAction;

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
					m_Spline = value;
					Initialize();
				}
			}
		}

		public FollowMode Mode
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
					Initialize();
				}
			}
		}

		public CurvyVector Initial
		{
			get
			{
				return m_Initial;
			}
			set
			{
				if (m_Initial != value)
				{
					m_Initial = value;
					if (Mode != FollowMode.Relative && SourceIsInitialized)
					{
						m_Initial.Validate(Spline.Length);
					}
					else
					{
						m_Initial.Validate();
					}
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
					if (SourceIsInitialized)
					{
						mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
					}
					m_Dynamic = value;
				}
			}
		}

		public CurvySplineSegment CurrentSegment
		{
			get
			{
				if (m_Dynamic)
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
				if (mCurrentSegmentF == -1f)
				{
					mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
				}
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

		public virtual float Position
		{
			get
			{
				FollowMode mode = Mode;
				if (mode == FollowMode.AbsoluteExtrapolate || mode == FollowMode.AbsolutePrecise)
				{
					return Spline.TFToDistance(mCurrentTF);
				}
				return mCurrentTF;
			}
			set
			{
				Current.Position = value;
				FollowMode mode = Mode;
				if (mode == FollowMode.AbsoluteExtrapolate || mode == FollowMode.AbsolutePrecise)
				{
					mCurrentTF = Spline.DistanceToTF(value);
				}
				else
				{
					mCurrentTF = value;
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
				Initialize();
			}
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				Initialize();
			}
		}

		private void Update()
		{
			if (UpdateIn == CurvyUpdateMethod.Update && Application.isPlaying)
			{
				doUpdate();
			}
		}

		private void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate)
			{
				doUpdate();
			}
		}

		private void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
			{
				doUpdate();
			}
		}

		private void OnValidate()
		{
			Initialize();
		}

		public virtual void Refresh()
		{
			UpdateAction();
		}

		public override void EditorUpdate()
		{
			base.EditorUpdate();
			doUpdate();
		}

		public override bool Initialize()
		{
			if (!SourceIsInitialized)
			{
				return false;
			}
			switch (Mode)
			{
			case FollowMode.AbsoluteExtrapolate:
				UpdateAction = UpdateAbsoluteExtrapolate;
				Initial.Absolute(Spline.Length);
				Current = new CurvyVector(Initial);
				mCurrentTF = Spline.DistanceToTF(Current.m_Position);
				break;
			case FollowMode.AbsolutePrecise:
				UpdateAction = UpdateAbsolutePrecise;
				Initial.Absolute(Spline.Length);
				Current = new CurvyVector(Initial);
				mCurrentTF = Spline.DistanceToTF(Current.m_Position);
				break;
			default:
				UpdateAction = UpdateRelative;
				Initial.Relative();
				Current = new CurvyVector(Initial);
				mCurrentTF = Current.Position;
				break;
			}
			mCurrentSegment = null;
			mCurrentSegmentF = 0f;
			base.Transform.position = Spline.Interpolate(mCurrentTF);
			if (SetOrientation)
			{
				orientate();
			}
			return true;
		}

		protected void UpdateRelative()
		{
			if (FastInterpolation)
			{
				base.Transform.position = Spline.MoveFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
			}
			else
			{
				base.Transform.position = Spline.Move(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
			}
			if (SetOrientation)
			{
				orientate();
			}
		}

		protected void UpdateAbsoluteExtrapolate()
		{
			if (FastInterpolation)
			{
				base.Transform.position = Spline.MoveByFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
			}
			else
			{
				base.Transform.position = Spline.MoveBy(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
			}
			if (SetOrientation)
			{
				orientate();
			}
		}

		protected void UpdateAbsolutePrecise()
		{
			base.Transform.position = Spline.MoveByLengthFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
			if (SetOrientation)
			{
				orientate();
			}
		}

		private void orientate()
		{
			if (Use2DOrientation)
			{
				base.Transform.rotation = Quaternion.LookRotation(Vector3.forward, Spline.GetTangentFast(mCurrentTF));
			}
			else
			{
				base.Transform.rotation = Spline.GetOrientationFast(mCurrentTF);
			}
		}

		private void doUpdate()
		{
			if (!SourceIsInitialized)
			{
				return;
			}
			if (!mIsInitialized)
			{
				mIsInitialized = Initialize();
				if (!mIsInitialized)
				{
					return;
				}
			}
			if (Dynamic)
			{
				if ((bool)mCurrentSegment)
				{
					mCurrentTF = Spline.SegmentToTF(mCurrentSegment, mCurrentSegmentF);
				}
				Refresh();
				mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
			}
			else
			{
				Refresh();
			}
		}
	}
}
