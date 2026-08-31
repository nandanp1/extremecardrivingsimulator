public class TimeFormatter : ProgressFormatter
{
	public override string Format(float progress, float target)
	{
		float seconds = target - progress;
		return Converter.SecondsToHhMmSs(seconds);
	}
}
