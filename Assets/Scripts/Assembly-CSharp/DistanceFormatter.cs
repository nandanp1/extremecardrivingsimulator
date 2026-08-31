public class DistanceFormatter : ProgressFormatter
{
	public override string Format(float progress, float target)
	{
		string text = Converter.MetersToCurrentUnits(progress, false);
		string text2 = Converter.MetersToCurrentUnits(target, false);
		return text + " / " + text2 + " " + Converter.GetDistanceUnit();
	}
}
