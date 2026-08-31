using UnityEngine;

public class Locomotion
{
	private Animator m_Animator;

	private int m_SpeedId;

	private int m_AgularSpeedId;

	private int m_DirectionId;

	public float m_SpeedDampTime = 0.1f;

	public float m_AnguarSpeedDampTime = 0.25f;

	public float m_DirectionResponseTime = 0.2f;

	public Locomotion(Animator animator)
	{
		m_Animator = animator;
		m_SpeedId = Animator.StringToHash("Speed");
		m_AgularSpeedId = Animator.StringToHash("AngularSpeed");
		m_DirectionId = Animator.StringToHash("Direction");
	}

	public void Do(float speed, float direction)
	{
		AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		bool flag = m_Animator.IsInTransition(0);
		bool flag2 = currentAnimatorStateInfo.IsName("Locomotion.Idle");
		bool flag3 = currentAnimatorStateInfo.IsName("Locomotion.TurnOnSpot") || currentAnimatorStateInfo.IsName("Locomotion.PlantNTurnLeft") || currentAnimatorStateInfo.IsName("Locomotion.PlantNTurnRight");
		bool flag4 = currentAnimatorStateInfo.IsName("Locomotion.WalkRun");
		float dampTime = ((!flag2) ? m_SpeedDampTime : 0f);
		float dampTime2 = ((!flag4 && !flag) ? 0f : m_AnguarSpeedDampTime);
		float dampTime3 = ((flag3 || flag) ? 1000000 : 0);
		float value = direction / m_DirectionResponseTime;
		m_Animator.SetFloat(m_SpeedId, speed, dampTime, Time.deltaTime);
		m_Animator.SetFloat(m_AgularSpeedId, value, dampTime2, Time.deltaTime);
		m_Animator.SetFloat(m_DirectionId, direction, dampTime3, Time.deltaTime);
	}
}
