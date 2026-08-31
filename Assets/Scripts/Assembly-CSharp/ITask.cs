public interface ITask
{
	bool IsComplete();

	void AddProgress(float deltaValue);

	float GetProgress01();
}
