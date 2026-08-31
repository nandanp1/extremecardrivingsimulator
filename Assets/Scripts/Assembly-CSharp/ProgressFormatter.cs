public class ProgressFormatter : IProgressFormatter
{
	public virtual string Format(float progress, float target)
	{
		return progress + "/" + target;
	}
}
