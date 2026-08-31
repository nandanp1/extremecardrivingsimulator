using UnityEngine;

public class AutoQualityChooser : MonoBehaviour
{
	public float minAcceptableFramerate = 30f;

	public string currentQuality;

	public float currentFramerate;

	public bool forceBestQualityOnStart;

	public bool disableAfterManualQualityChange = true;

	public bool disabled;

	public bool verbose;

	public float updateRate = 1f;

	public float threshold = 5f;

	private int currQuality;

	private float minThreshold;

	private float maxThreshold;

	private int previousQuality = -1;

	private int frameCount;

	private float nextUpdate;

	private bool ignoreOneIteration = true;

	private bool testIteration;

	public int currentQualityInt
	{
		get
		{
			return currQuality;
		}
	}

	private void afterQualityChange()
	{
	}

	private void Start()
	{
		minThreshold = threshold;
		if (forceBestQualityOnStart)
		{
			QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1);
			currQuality = QualitySettings.GetQualityLevel();
			currentQuality = string.Empty + currQuality + " (" + QualitySettings.names[currQuality] + ")";
			if (verbose)
			{
				Debug.Log("Quality on start: " + currentQuality);
			}
		}
		else
		{
			aproxQuality();
		}
		restartComponent();
		nextUpdate = Time.realtimeSinceStartup + 1f / updateRate;
	}

	private void aproxQuality()
	{
		float num = SystemInfo.graphicsPixelFillrate;
		float num2 = SystemInfo.graphicsShaderLevel;
		float num3 = SystemInfo.graphicsMemorySize;
		float num4 = SystemInfo.processorCount;
		if (num < 0f)
		{
			num = ((num2 < 10f) ? 1000f : ((num2 < 20f) ? 1300f : ((!(num2 < 30f)) ? 3000f : 2000f)));
			if (num4 >= 6f)
			{
				num *= 3f;
			}
			else if (num4 >= 3f)
			{
				num *= 2f;
			}
			if (num3 >= 512f)
			{
				num *= 2f;
			}
			else if (num3 <= 128f)
			{
				num /= 2f;
			}
		}
		float num5 = (float)(Screen.width * Screen.height + 120000) * (minAcceptableFramerate / 1000000f);
		float[] array = new float[6] { 5f, 30f, 80f, 130f, 200f, 320f };
		int i;
		for (i = 0; i < QualitySettings.names.Length - 1 && num > num5 * array[i + 1]; i++)
		{
		}
		QualitySettings.SetQualityLevel(i);
		currQuality = QualitySettings.GetQualityLevel();
		currentQuality = string.Empty + currQuality + " (" + QualitySettings.names[currQuality] + ")";
		if (verbose)
		{
			Debug.Log("Quality on start: " + currentQuality);
		}
	}

	public void restartComponent()
	{
		threshold = minThreshold;
		maxThreshold = minThreshold;
		currQuality = QualitySettings.GetQualityLevel();
	}

	private void Update()
	{
		frameCount++;
		if (!(Time.realtimeSinceStartup > nextUpdate))
		{
			return;
		}
		nextUpdate = Time.realtimeSinceStartup + 1f / updateRate;
		currentFramerate = (float)frameCount * updateRate;
		frameCount = 0;
		if (threshold > minThreshold)
		{
			threshold--;
		}
		if (currQuality != QualitySettings.GetQualityLevel())
		{
			currQuality = QualitySettings.GetQualityLevel();
			currentQuality = string.Empty + currQuality + " (" + QualitySettings.names[currQuality] + ")";
			if (disableAfterManualQualityChange)
			{
				disabled = true;
				return;
			}
		}
		if (disabled)
		{
			ignoreOneIteration = true;
			return;
		}
		currQuality = QualitySettings.GetQualityLevel();
		if (ignoreOneIteration)
		{
			ignoreOneIteration = false;
			return;
		}
		if (testIteration)
		{
			testIteration = false;
			if (currentFramerate < minAcceptableFramerate)
			{
				decreaseQuality();
				return;
			}
		}
		if (currentFramerate < minAcceptableFramerate)
		{
			decreaseQuality();
		}
		else if (currentFramerate - threshold > minAcceptableFramerate)
		{
			increaseQuality();
		}
	}

	public void increaseQuality()
	{
		changeQuality(1);
	}

	public void decreaseQuality()
	{
		changeQuality(-1);
	}

	private void changeQuality(int amount)
	{
		if (amount > 0)
		{
			if (currQuality + amount >= QualitySettings.names.Length)
			{
				return;
			}
		}
		else if (currQuality + amount < 0)
		{
			return;
		}
		if (currQuality + amount == previousQuality)
		{
			maxThreshold *= 2f;
			threshold = maxThreshold;
			minThreshold += 0.5f;
		}
		previousQuality = currQuality;
		QualitySettings.SetQualityLevel(currQuality + amount);
		currQuality = QualitySettings.GetQualityLevel();
		currentQuality = string.Empty + currQuality + " (" + QualitySettings.names[currQuality] + ")";
		ignoreOneIteration = true;
		if (amount > 0)
		{
			testIteration = true;
			if (verbose)
			{
				Debug.Log("Quality increased to " + currQuality + ", framerate: " + currentFramerate);
			}
		}
		else if (verbose)
		{
			Debug.Log("Quality decreased to " + currQuality + ", framerate: " + currentFramerate);
		}
		afterQualityChange();
	}
}
