using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Reporter : MonoBehaviour
{
	public enum _LogType
	{
		Assert = 1,
		Error = 0,
		Exception = 4,
		Log = 3,
		Warning = 2
	}

	public class Sample
	{
		public float time;

		public byte loadedScene;

		public float memory;

		public float fps;

		public string fpsText;

		public static float MemSize()
		{
			return 13f;
		}
	}

	public class Log
	{
		public int count = 1;

		public _LogType logType;

		public string condition;

		public string stacktrace;

		public int sampleId;

		public Log CreateCopy()
		{
			return (Log)MemberwiseClone();
		}

		public float GetMemoryUsage()
		{
			return 8 + condition.Length * 2 + stacktrace.Length * 2 + 4;
		}
	}

	private enum ReportView
	{
		None = 0,
		Logs = 1,
		Info = 2,
		Snapshot = 3
	}

	private enum DetailView
	{
		None = 0,
		StackTrace = 1,
		Graph = 2
	}

	private List<Sample> samples = new List<Sample>(216000);

	private List<Log> logs = new List<Log>();

	private List<Log> collapsedLogs = new List<Log>();

	private List<Log> currentLog = new List<Log>();

	private Dictionary<string, Log> logsDic = new Dictionary<string, Log>();

	private Dictionary<string, string> cachedString = new Dictionary<string, string>();

	[HideInInspector]
	public bool show;

	private bool collapse;

	private bool clearOnNewSceneLoaded;

	private bool showTime;

	private bool showScene;

	private bool showMemory;

	private bool showFps;

	private bool showGraph;

	private bool showLog = true;

	private bool showWarning = true;

	private bool showError = true;

	private int numOfLogs;

	private int numOfLogsWarning;

	private int numOfLogsError;

	private int numOfCollapsedLogs;

	private int numOfCollapsedLogsWarning;

	private int numOfCollapsedLogsError;

	private string buildDate;

	private string logDate;

	private float logsMemUsage;

	private float graphMemUsage;

	private float gcTotalMemory;

	public string UserData = string.Empty;

	public float fps;

	public string fpsText;

	private ReportView currentView = ReportView.Logs;

	private static bool created;

	private GUIContent clearContent;

	private GUIContent collapseContent;

	private GUIContent clearOnNewSceneContent;

	private GUIContent showTimeContent;

	private GUIContent showSceneContent;

	private GUIContent userContent;

	private GUIContent showMemoryContent;

	private GUIContent softwareContent;

	private GUIContent dateContent;

	private GUIContent showFpsContent;

	private GUIContent graphContent;

	private GUIContent infoContent;

	private GUIContent closeContent;

	private GUIContent buildFromContent;

	private GUIContent systemInfoContent;

	private GUIContent graphicsInfoContent;

	private GUIContent backContent;

	private GUIContent cameraContent;

	private Texture2D logIcon;

	private Texture2D warningIcon;

	private Texture2D errorIcon;

	private GUIContent logContent;

	private GUIContent warningContent;

	private GUIContent errorContent;

	private GUIStyle barStyle;

	private GUIStyle buttonActiveStyle;

	private GUIStyle nonStyle;

	private GUIStyle lowerLeftFontStyle;

	private GUIStyle backStyle;

	private GUIStyle evenLogStyle;

	private GUIStyle oddLogStyle;

	private GUIStyle logButtonStyle;

	private GUIStyle selectedLogStyle;

	private GUIStyle selectedLogFontStyle;

	private GUIStyle stackLabelStyle;

	private GUIStyle scrollerStyle;

	private GUISkin toolbarScrollerSkin;

	private GUISkin logScrollerSkin;

	private GUISkin graphScrollerSkin;

	public Vector2 size = new Vector2(32f, 32f);

	public float maxSize = 20f;

	public int numOfCircleToShow = 1;

	private string[] scenes;

	private string currentScene;

	private string deviceModel;

	private string deviceType;

	private string deviceName;

	private string graphicsMemorySize;

	private string maxTextureSize;

	private string systemMemorySize;

	public bool Initialized;

	private Rect screenRect;

	private Rect toolBarRect;

	private Rect logsRect;

	private Rect stackRect;

	private Rect graphRect;

	private Rect graphMinRect;

	private Rect graphMaxRect;

	private Rect buttomRect;

	private Vector2 stackRectTopLeft;

	private Rect detailRect;

	private Vector2 scrollPosition;

	private Vector2 scrollPosition2;

	private Vector2 toolbarScrollPosition;

	private int selectedIndex = -1;

	private Log selectedLog;

	private float toolbarOldDrag;

	private float oldDrag;

	private float oldDrag2;

	private float oldDrag3;

	private int startIndex;

	private Rect countRect;

	private Rect timeRect;

	private Rect timeLabelRect;

	private Rect sceneRect;

	private Rect sceneLabelRect;

	private Rect memoryRect;

	private Rect memoryLabelRect;

	private Rect fpsRect;

	private Rect fpsLabelRect;

	private GUIContent tempContent = new GUIContent();

	private Vector2 infoScrollPosition;

	private Vector2 oldInfoDrag;

	private Rect tempRect;

	private float graphSize = 4f;

	private int startFrame;

	private int currentFrame;

	private Vector3 tempVector1;

	private Vector3 tempVector2;

	private Vector2 graphScrollerPos;

	private float maxFpsValue;

	private float minFpsValue;

	private float maxMemoryValue;

	private float minMemoryValue;

	private List<Vector2> gestureDetector = new List<Vector2>();

	private Vector2 gestureSum = Vector2.zero;

	private float gestureLength;

	private int gestureCount;

	private float lastClickTime = -1f;

	private Vector2 startPos;

	private Vector2 downPos;

	private Vector2 mousePosition;

	private float lastUpdate;

	private float lastUpdate2;

	private Rect temp;

	public float TotalMemUsage
	{
		get
		{
			return logsMemUsage + graphMemUsage;
		}
	}

	private void Awake()
	{
		if (!Debug.isDebugBuild)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (!Initialized)
		{
			Initialize();
		}
	}

	private void OnEnable()
	{
		if (logs.Count == 0)
		{
			clear();
		}
	}

	private void OnDisable()
	{
	}

	private void addSample()
	{
		Sample sample = new Sample();
		sample.fps = fps;
		sample.fpsText = fpsText;
		sample.loadedScene = (byte)LegacySceneManager.LoadedLevel;
		sample.time = Time.realtimeSinceStartup;
		sample.memory = gcTotalMemory;
		samples.Add(sample);
		graphMemUsage = (float)samples.Count * Sample.MemSize() / 1024f / 1024f;
	}

	public void Initialize()
	{
		if (!created)
		{
			try
			{
				base.gameObject.SendMessage("OnPreStart");
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			scenes = new string[LegacySceneManager.LevelCount];
			currentScene = LegacySceneManager.LoadedLevelName;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			Application.RegisterLogCallback(CaptureLog);
			created = true;
			int num = (int)(size.x * 0.2f);
			int num2 = (int)(size.y * 0.2f);
			clearContent = new GUIContent(string.Empty, getImage("clear", size.x, size.y), "Clear logs");
			collapseContent = new GUIContent(string.Empty, getImage("collapse", size.x, size.y), "Collapse logs");
			clearOnNewSceneContent = new GUIContent(string.Empty, getImage("clearOnSceneLoaded", size.x, size.y), "Clear logs on new scene loaded");
			showTimeContent = new GUIContent(string.Empty, getImage("timer_1", size.x, size.y), "Show Hide Time");
			showSceneContent = new GUIContent(string.Empty, getImage("UnityIcon", size.x, size.y), "Show Hide Scene");
			showMemoryContent = new GUIContent(string.Empty, getImage("memory", size.x, size.y), "Show Hide Memory");
			softwareContent = new GUIContent(string.Empty, getImage("software", size.x, size.y), "Software");
			dateContent = new GUIContent(string.Empty, getImage("date", size.x, size.y), "Date");
			showFpsContent = new GUIContent(string.Empty, getImage("fps", size.x, size.y), "Show Hide fps");
			graphContent = new GUIContent(string.Empty, getImage("chart", size.x, size.y), "Show Graph");
			infoContent = new GUIContent(string.Empty, getImage("info", size.x, size.y), "Information about application");
			closeContent = new GUIContent(string.Empty, getImage("close", size.x, size.y), "Hide logs");
			userContent = new GUIContent(string.Empty, getImage("user", size.x, size.y), "User");
			buildFromContent = new GUIContent(string.Empty, getImage("buildFrom", size.x, size.y), "Build From");
			systemInfoContent = new GUIContent(string.Empty, getImage("ComputerIcon", size.x, size.y), "System Info");
			graphicsInfoContent = new GUIContent(string.Empty, getImage("graphicCard", size.x, size.y), "Graphics Info");
			backContent = new GUIContent(string.Empty, getImage("back", size.x, size.y), "Back");
			cameraContent = new GUIContent(string.Empty, getImage("camera", size.x, size.y), "Select Photo");
			logIcon = getImage("log_icon", size.x, size.y);
			warningIcon = getImage("warning_icon", size.x, size.y);
			errorIcon = getImage("error_icon", size.x, size.y);
			logContent = new GUIContent(string.Empty, logIcon, "show or hide logs");
			warningContent = new GUIContent(string.Empty, warningIcon, "show or hide warnings");
			errorContent = new GUIContent(string.Empty, errorIcon, "show or hide errors");
			nonStyle = new GUIStyle();
			nonStyle.clipping = TextClipping.Clip;
			nonStyle.border = new RectOffset(0, 0, 0, 0);
			nonStyle.normal.background = null;
			nonStyle.fontSize = (int)(size.y / 2f);
			nonStyle.alignment = TextAnchor.MiddleCenter;
			lowerLeftFontStyle = new GUIStyle();
			lowerLeftFontStyle.clipping = TextClipping.Clip;
			lowerLeftFontStyle.border = new RectOffset(0, 0, 0, 0);
			lowerLeftFontStyle.normal.background = null;
			lowerLeftFontStyle.fontSize = (int)(size.y / 2f);
			lowerLeftFontStyle.fontStyle = FontStyle.Bold;
			lowerLeftFontStyle.alignment = TextAnchor.LowerLeft;
			barStyle = new GUIStyle();
			barStyle.border = new RectOffset(1, 1, 1, 1);
			barStyle.normal.background = getImage("bar", size.x, size.y);
			barStyle.active.background = getImage("button_active", size.x, size.y);
			barStyle.alignment = TextAnchor.MiddleCenter;
			barStyle.margin = new RectOffset(1, 1, 1, 1);
			barStyle.clipping = TextClipping.Clip;
			barStyle.fontSize = (int)(size.y / 2f);
			buttonActiveStyle = new GUIStyle();
			buttonActiveStyle.border = new RectOffset(1, 1, 1, 1);
			buttonActiveStyle.normal.background = getImage("button_active", 32f, 32f);
			buttonActiveStyle.alignment = TextAnchor.MiddleCenter;
			buttonActiveStyle.margin = new RectOffset(1, 1, 1, 1);
			buttonActiveStyle.fontSize = (int)(size.y / 2f);
			backStyle = new GUIStyle();
			backStyle.normal.background = getImage("even_log", 16f, 16f);
			backStyle.clipping = TextClipping.Clip;
			backStyle.fontSize = (int)(size.y / 2f);
			evenLogStyle = new GUIStyle();
			evenLogStyle.normal.background = getImage("even_log", 16f, 16f);
			evenLogStyle.fixedHeight = size.y;
			evenLogStyle.clipping = TextClipping.Clip;
			evenLogStyle.alignment = TextAnchor.UpperLeft;
			evenLogStyle.imagePosition = ImagePosition.ImageLeft;
			evenLogStyle.fontSize = (int)(size.y / 2f);
			oddLogStyle = new GUIStyle();
			oddLogStyle.normal.background = getImage("odd_log", 16f, 16f);
			oddLogStyle.fixedHeight = size.y;
			oddLogStyle.clipping = TextClipping.Clip;
			oddLogStyle.alignment = TextAnchor.UpperLeft;
			oddLogStyle.imagePosition = ImagePosition.ImageLeft;
			oddLogStyle.fontSize = (int)(size.y / 2f);
			logButtonStyle = new GUIStyle();
			logButtonStyle.fixedHeight = size.y;
			logButtonStyle.clipping = TextClipping.Clip;
			logButtonStyle.alignment = TextAnchor.UpperLeft;
			logButtonStyle.fontSize = (int)(size.y / 2f);
			logButtonStyle.padding = new RectOffset(num, num, num2, num2);
			selectedLogStyle = new GUIStyle();
			selectedLogStyle.normal.background = getImage("selected", 16f, 16f);
			selectedLogStyle.fixedHeight = size.y;
			selectedLogStyle.clipping = TextClipping.Clip;
			selectedLogStyle.alignment = TextAnchor.UpperLeft;
			selectedLogStyle.normal.textColor = Color.white;
			selectedLogStyle.fontSize = (int)(size.y / 2f);
			selectedLogFontStyle = new GUIStyle();
			selectedLogFontStyle.normal.background = getImage("selected", 16f, 16f);
			selectedLogFontStyle.fixedHeight = size.y;
			selectedLogFontStyle.clipping = TextClipping.Clip;
			selectedLogFontStyle.alignment = TextAnchor.UpperLeft;
			selectedLogFontStyle.normal.textColor = Color.white;
			selectedLogFontStyle.fontSize = (int)(size.y / 2f);
			selectedLogFontStyle.padding = new RectOffset(num, num, num2, num2);
			stackLabelStyle = new GUIStyle();
			stackLabelStyle.wordWrap = true;
			stackLabelStyle.fontSize = (int)(size.y / 2f);
			stackLabelStyle.padding = new RectOffset(num, num, num2, num2);
			scrollerStyle = new GUIStyle();
			scrollerStyle.normal.background = getImage("bar", size.x, size.y);
			GUISkin original = Resources.Load("reporterScrollerSkin", typeof(GUISkin)) as GUISkin;
			toolbarScrollerSkin = (GUISkin)UnityEngine.Object.Instantiate(original);
			toolbarScrollerSkin.verticalScrollbar.fixedWidth = 0f;
			toolbarScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
			toolbarScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
			toolbarScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;
			logScrollerSkin = (GUISkin)UnityEngine.Object.Instantiate(original);
			logScrollerSkin.verticalScrollbar.fixedWidth = size.x * 2f;
			logScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
			logScrollerSkin.verticalScrollbarThumb.fixedWidth = size.x * 2f;
			logScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;
			graphScrollerSkin = (GUISkin)UnityEngine.Object.Instantiate(original);
			graphScrollerSkin.verticalScrollbar.fixedWidth = 0f;
			graphScrollerSkin.horizontalScrollbar.fixedHeight = size.x * 2f;
			graphScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
			graphScrollerSkin.horizontalScrollbarThumb.fixedHeight = size.x * 2f;
			currentView = (ReportView)PlayerPrefs.GetInt("Reporter_currentView", 1);
			show = PlayerPrefs.GetInt("Reporter_show") == 1;
			collapse = PlayerPrefs.GetInt("Reporter_collapse") == 1;
			clearOnNewSceneLoaded = PlayerPrefs.GetInt("Reporter_clearOnNewSceneLoaded") == 1;
			showTime = PlayerPrefs.GetInt("Reporter_showTime") == 1;
			showScene = PlayerPrefs.GetInt("Reporter_showScene") == 1;
			showMemory = PlayerPrefs.GetInt("Reporter_showMemory") == 1;
			showFps = PlayerPrefs.GetInt("Reporter_showFps") == 1;
			showGraph = PlayerPrefs.GetInt("Reporter_showGraph") == 1;
			showLog = PlayerPrefs.GetInt("Reporter_showLog", 1) == 1;
			showWarning = PlayerPrefs.GetInt("Reporter_showWarning", 1) == 1;
			showError = PlayerPrefs.GetInt("Reporter_showError", 1) == 1;
			Initialized = true;
			if (show)
			{
				doShow();
			}
			deviceModel = SystemInfo.deviceModel.ToString();
			deviceType = SystemInfo.deviceType.ToString();
			deviceName = SystemInfo.deviceName.ToString();
			graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
			maxTextureSize = SystemInfo.maxTextureSize.ToString();
			systemMemorySize = SystemInfo.systemMemorySize.ToString();
		}
		else
		{
			Debug.LogWarning("tow manager is exists delete the second");
			UnityEngine.Object.DestroyImmediate(base.gameObject, true);
		}
	}

	public void CatureLog()
	{
		Application.RegisterLogCallback(CaptureLog);
	}

	private void Start()
	{
		logDate = DateTime.Now.ToString();
		StartCoroutine("readInfo");
	}

	private void clear()
	{
		logs.Clear();
		collapsedLogs.Clear();
		currentLog.Clear();
		logsDic.Clear();
		selectedIndex = -1;
		numOfLogs = 0;
		numOfLogsWarning = 0;
		numOfLogsError = 0;
		numOfCollapsedLogs = 0;
		numOfCollapsedLogsWarning = 0;
		numOfCollapsedLogsError = 0;
		logsMemUsage = 0f;
		graphMemUsage = 0f;
		samples.Clear();
		GC.Collect();
		selectedLog = null;
	}

	private Texture2D getImage(string path, float width, float height)
	{
		return (Texture2D)Resources.Load(path, typeof(Texture2D));
	}

	private void calculateCurrentLog()
	{
		currentLog.Clear();
		if (collapse)
		{
			for (int i = 0; i < collapsedLogs.Count; i++)
			{
				Log log = collapsedLogs[i];
				if ((log.logType != _LogType.Log || showLog) && (log.logType != _LogType.Warning || showWarning) && (log.logType != _LogType.Error || showError) && (log.logType != _LogType.Assert || showError) && (log.logType != _LogType.Exception || showError))
				{
					currentLog.Add(log);
				}
			}
			return;
		}
		for (int j = 0; j < logs.Count; j++)
		{
			Log log2 = logs[j];
			if ((log2.logType != _LogType.Log || showLog) && (log2.logType != _LogType.Warning || showWarning) && (log2.logType != _LogType.Error || showError) && (log2.logType != _LogType.Assert || showError) && (log2.logType != _LogType.Exception || showError))
			{
				currentLog.Add(log2);
			}
		}
	}

	private void DrawInfo()
	{
		GUILayout.BeginArea(screenRect, backStyle);
		Vector2 drag = getDrag();
		if (drag.x != 0f && downPos != Vector2.zero)
		{
			infoScrollPosition.x -= drag.x - oldInfoDrag.x;
		}
		if (drag.y != 0f && downPos != Vector2.zero)
		{
			infoScrollPosition.y += drag.y - oldInfoDrag.y;
		}
		oldInfoDrag = drag;
		GUI.skin = toolbarScrollerSkin;
		infoScrollPosition = GUILayout.BeginScrollView(infoScrollPosition);
		GUILayout.Space(size.x);
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(buildFromContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(buildDate, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(systemInfoContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceModel, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceType, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceName, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(graphicsInfoContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(SystemInfo.graphicsDeviceName, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(graphicsMemorySize, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(maxTextureSize, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Label("Screen Width " + Screen.width, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("Screen Height " + Screen.height, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(systemMemorySize + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Label("Mem Usage Of Logs " + logsMemUsage.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("Mem Usage Of Graph " + graphMemUsage.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("GC Memory " + gcTotalMemory.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(softwareContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(SystemInfo.operatingSystem, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(dateContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(DateTime.Now.ToString(), nonStyle, GUILayout.Height(size.y));
		GUILayout.Label(" - Application Started At " + logDate, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(Time.realtimeSinceStartup.ToString("000"), nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(fpsText, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(userContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(UserData, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(currentScene, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		if (GUILayout.Button(backContent, barStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			currentView = ReportView.Logs;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawReport()
	{
		screenRect.x = 0f;
		screenRect.y = 0f;
		screenRect.width = Screen.width;
		screenRect.height = Screen.height;
		GUILayout.BeginArea(screenRect, backStyle);
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box(cameraContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.Label("Select Photo", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Comming Soon", nonStyle, GUILayout.Height(size.y));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(backContent, barStyle, GUILayout.Width(size.x), GUILayout.Height(size.y)))
		{
			currentView = ReportView.Logs;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void drawToolBar()
	{
		toolBarRect.x = 0f;
		toolBarRect.y = 0f;
		toolBarRect.width = Screen.width;
		toolBarRect.height = size.y * 2f;
		GUI.skin = toolbarScrollerSkin;
		Vector2 drag = getDrag();
		if (drag.x != 0f && downPos != Vector2.zero && downPos.y > (float)Screen.height - size.y * 2f)
		{
			toolbarScrollPosition.x -= drag.x - toolbarOldDrag;
		}
		toolbarOldDrag = drag.x;
		GUILayout.BeginArea(toolBarRect);
		toolbarScrollPosition = GUILayout.BeginScrollView(toolbarScrollPosition);
		GUILayout.BeginHorizontal(barStyle);
		if (GUILayout.Button(clearContent, barStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			clear();
		}
		if (GUILayout.Button(collapseContent, (!collapse) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			collapse = !collapse;
			calculateCurrentLog();
		}
		if (GUILayout.Button(clearOnNewSceneContent, (!clearOnNewSceneLoaded) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			clearOnNewSceneLoaded = !clearOnNewSceneLoaded;
		}
		if (GUILayout.Button(showTimeContent, (!showTime) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showTime = !showTime;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, Time.realtimeSinceStartup.ToString("0.0"), lowerLeftFontStyle);
		if (GUILayout.Button(showSceneContent, (!showScene) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showScene = !showScene;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, currentScene, lowerLeftFontStyle);
		if (GUILayout.Button(showMemoryContent, (!showMemory) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showMemory = !showMemory;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, gcTotalMemory.ToString("0.0"), lowerLeftFontStyle);
		if (GUILayout.Button(showFpsContent, (!showFps) ? barStyle : buttonActiveStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showFps = !showFps;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, fpsText, lowerLeftFontStyle);
		if (GUILayout.Button(infoContent, barStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			currentView = ReportView.Info;
		}
		GUILayout.FlexibleSpace();
		string text = " ";
		text = ((!collapse) ? (text + numOfLogs) : (text + numOfCollapsedLogs));
		string text2 = " ";
		text2 = ((!collapse) ? (text2 + numOfLogsWarning) : (text2 + numOfCollapsedLogsWarning));
		string text3 = " ";
		text3 = ((!collapse) ? (text3 + numOfLogsError) : (text3 + numOfCollapsedLogsError));
		GUILayout.BeginHorizontal((!showLog) ? barStyle : buttonActiveStyle);
		if (GUILayout.Button(logContent, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showLog = !showLog;
			calculateCurrentLog();
		}
		if (GUILayout.Button(text, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showLog = !showLog;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((!showWarning) ? barStyle : buttonActiveStyle);
		if (GUILayout.Button(warningContent, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		if (GUILayout.Button(text2, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((!showError) ? nonStyle : buttonActiveStyle);
		if (GUILayout.Button(errorContent, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showError = !showError;
			calculateCurrentLog();
		}
		if (GUILayout.Button(text3, nonStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			showError = !showError;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button(closeContent, barStyle, GUILayout.Width(size.x * 2f), GUILayout.Height(size.y * 2f)))
		{
			show = false;
			ReporterGUI component = base.gameObject.GetComponent<ReporterGUI>();
			UnityEngine.Object.DestroyImmediate(component);
			try
			{
				base.gameObject.SendMessage("OnHideReporter");
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawLogs()
	{
		GUILayout.BeginArea(logsRect, backStyle);
		GUI.skin = logScrollerSkin;
		Vector2 drag = getDrag();
		if (drag.y != 0f && logsRect.Contains(new Vector2(downPos.x, (float)Screen.height - downPos.y)))
		{
			scrollPosition.y += drag.y - oldDrag;
		}
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		oldDrag = drag.y;
		int a = (int)((float)Screen.height * 0.75f / size.y);
		int totalCount = getTotalCount();
		a = Mathf.Min(a, totalCount - startIndex);
		int num = 0;
		int num2 = (int)((float)startIndex * size.y);
		selectedIndex = Mathf.Clamp(selectedIndex, -1, totalCount - 1);
		if (num2 > 0)
		{
			GUILayout.BeginHorizontal(GUILayout.Height(num2));
			GUILayout.Label("---");
			GUILayout.EndHorizontal();
		}
		int value = startIndex + a;
		value = Mathf.Clamp(value, 0, totalCount);
		bool flag = a < totalCount;
		int num3 = startIndex;
		while (startIndex + num < value && num3 < currentLog.Count)
		{
			Log log = currentLog[num3];
			if ((log.logType != _LogType.Log || showLog) && (log.logType != _LogType.Warning || showWarning) && (log.logType != _LogType.Error || showError) && (log.logType != _LogType.Assert || showError) && (log.logType != _LogType.Exception || showError))
			{
				if (num >= a)
				{
					break;
				}
				GUIContent gUIContent = null;
				gUIContent = ((log.logType == _LogType.Log) ? logContent : ((log.logType != _LogType.Warning) ? errorContent : warningContent));
				GUIStyle gUIStyle = (((startIndex + num) % 2 != 0) ? oddLogStyle : evenLogStyle);
				if (selectedIndex == startIndex + num)
				{
					selectedLog = log;
					gUIStyle = selectedLogStyle;
				}
				tempContent.text = log.count.ToString();
				float num4 = 0f;
				if (collapse)
				{
					num4 = barStyle.CalcSize(tempContent).x + 3f;
				}
				countRect.x = (float)Screen.width - num4;
				countRect.y = size.y * (float)num3;
				if (num2 > 0)
				{
					countRect.y += 8f;
				}
				countRect.width = num4;
				countRect.height = size.y;
				if (flag)
				{
					countRect.x -= size.x * 2f;
				}
				Sample sample = samples[log.sampleId];
				fpsRect = countRect;
				if (showFps)
				{
					tempContent.text = sample.fpsText;
					num4 = gUIStyle.CalcSize(tempContent).x + size.x;
					fpsRect.x -= num4;
					fpsRect.width = size.x;
					fpsLabelRect = fpsRect;
					fpsLabelRect.x += size.x;
					fpsLabelRect.width = num4 - size.x;
				}
				memoryRect = fpsRect;
				if (showMemory)
				{
					tempContent.text = sample.memory.ToString("0.000");
					num4 = gUIStyle.CalcSize(tempContent).x + size.x;
					memoryRect.x -= num4;
					memoryRect.width = size.x;
					memoryLabelRect = memoryRect;
					memoryLabelRect.x += size.x;
					memoryLabelRect.width = num4 - size.x;
				}
				sceneRect = memoryRect;
				if (showScene)
				{
					tempContent.text = scenes[sample.loadedScene];
					num4 = gUIStyle.CalcSize(tempContent).x + size.x;
					sceneRect.x -= num4;
					sceneRect.width = size.x;
					sceneLabelRect = sceneRect;
					sceneLabelRect.x += size.x;
					sceneLabelRect.width = num4 - size.x;
				}
				timeRect = sceneRect;
				if (showTime)
				{
					tempContent.text = sample.time.ToString("0.000");
					num4 = gUIStyle.CalcSize(tempContent).x + size.x;
					timeRect.x -= num4;
					timeRect.width = size.x;
					timeLabelRect = timeRect;
					timeLabelRect.x += size.x;
					timeLabelRect.width = num4 - size.x;
				}
				GUILayout.BeginHorizontal(gUIStyle);
				if (selectedIndex == startIndex + num)
				{
					GUILayout.Box(gUIContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
					GUILayout.Label(log.condition, selectedLogFontStyle);
					if (showTime)
					{
						GUI.Box(timeRect, showTimeContent, gUIStyle);
						GUI.Label(timeLabelRect, sample.time.ToString("0.000"), gUIStyle);
					}
					if (showScene)
					{
						GUI.Box(sceneRect, showSceneContent, gUIStyle);
						GUI.Label(sceneLabelRect, scenes[sample.loadedScene], gUIStyle);
					}
					if (showMemory)
					{
						GUI.Box(memoryRect, showMemoryContent, gUIStyle);
						GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", gUIStyle);
					}
					if (showFps)
					{
						GUI.Box(fpsRect, showFpsContent, gUIStyle);
						GUI.Label(fpsLabelRect, sample.fpsText, gUIStyle);
					}
				}
				else
				{
					if (GUILayout.Button(gUIContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y)))
					{
						selectedIndex = startIndex + num;
					}
					if (GUILayout.Button(log.condition, logButtonStyle))
					{
						selectedIndex = startIndex + num;
					}
					if (showTime)
					{
						GUI.Box(timeRect, showTimeContent, gUIStyle);
						GUI.Label(timeLabelRect, sample.time.ToString("0.000"), gUIStyle);
					}
					if (showScene)
					{
						GUI.Box(sceneRect, showSceneContent, gUIStyle);
						GUI.Label(sceneLabelRect, scenes[sample.loadedScene], gUIStyle);
					}
					if (showMemory)
					{
						GUI.Box(memoryRect, showMemoryContent, gUIStyle);
						GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", gUIStyle);
					}
					if (showFps)
					{
						GUI.Box(fpsRect, showFpsContent, gUIStyle);
						GUI.Label(fpsLabelRect, sample.fpsText, gUIStyle);
					}
				}
				if (collapse)
				{
					GUI.Label(countRect, log.count.ToString(), barStyle);
				}
				GUILayout.EndHorizontal();
				num++;
			}
			num3++;
		}
		int num5 = (int)((float)(totalCount - (startIndex + a)) * size.y);
		if (num5 > 0)
		{
			GUILayout.BeginHorizontal(GUILayout.Height(num5));
			GUILayout.Label(" ");
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		buttomRect.x = 0f;
		buttomRect.y = (float)Screen.height - size.y;
		buttomRect.width = Screen.width;
		buttomRect.height = size.y;
		if (showGraph)
		{
			drawGraph();
		}
		else
		{
			drawStack();
		}
	}

	private void drawGraph()
	{
		graphRect = stackRect;
		graphRect.height = (float)Screen.height * 0.25f;
		GUI.skin = graphScrollerSkin;
		Vector2 drag = getDrag();
		if (graphRect.Contains(new Vector2(downPos.x, (float)Screen.height - downPos.y)))
		{
			if (drag.x != 0f)
			{
				graphScrollerPos.x -= drag.x - oldDrag3;
				graphScrollerPos.x = Mathf.Max(0f, graphScrollerPos.x);
			}
			Vector2 vector = downPos;
			if (vector != Vector2.zero)
			{
				currentFrame = startFrame + (int)(vector.x / graphSize);
			}
		}
		oldDrag3 = drag.x;
		GUILayout.BeginArea(graphRect, backStyle);
		graphScrollerPos = GUILayout.BeginScrollView(graphScrollerPos);
		startFrame = (int)(graphScrollerPos.x / graphSize);
		if (graphScrollerPos.x >= (float)samples.Count * graphSize - (float)Screen.width)
		{
			graphScrollerPos.x += graphSize;
		}
		GUILayout.Label(" ", GUILayout.Width((float)samples.Count * graphSize));
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		maxFpsValue = 0f;
		minFpsValue = 100000f;
		maxMemoryValue = 0f;
		minMemoryValue = 100000f;
		for (int i = 0; (float)i < (float)Screen.width / graphSize; i++)
		{
			int num = startFrame + i;
			if (num >= samples.Count)
			{
				break;
			}
			Sample sample = samples[num];
			if (maxFpsValue < sample.fps)
			{
				maxFpsValue = sample.fps;
			}
			if (minFpsValue > sample.fps)
			{
				minFpsValue = sample.fps;
			}
			if (maxMemoryValue < sample.memory)
			{
				maxMemoryValue = sample.memory;
			}
			if (minMemoryValue > sample.memory)
			{
				minMemoryValue = sample.memory;
			}
		}
		if (currentFrame != -1 && currentFrame < samples.Count)
		{
			Sample sample2 = samples[currentFrame];
			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.BeginHorizontal();
			GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample2.time.ToString("0.0"), nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(scenes[sample2.loadedScene], nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample2.memory.ToString("0.000"), nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample2.fpsText, nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(graphContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(currentFrame.ToString(), nonStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		graphMaxRect = stackRect;
		graphMaxRect.height = size.y;
		GUILayout.BeginArea(graphMaxRect);
		GUILayout.BeginHorizontal();
		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(maxMemoryValue.ToString("0.000"), nonStyle);
		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(maxFpsValue.ToString("0.000"), nonStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		graphMinRect = stackRect;
		graphMinRect.y = stackRect.y + stackRect.height - size.y;
		graphMinRect.height = size.y;
		GUILayout.BeginArea(graphMinRect);
		GUILayout.BeginHorizontal();
		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(minMemoryValue.ToString("0.000"), nonStyle);
		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(minFpsValue.ToString("0.000"), nonStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private void drawStack()
	{
		if (selectedIndex != -1 && selectedLog != null)
		{
			Vector2 drag = getDrag();
			if (drag.y != 0f && stackRect.Contains(new Vector2(downPos.x, (float)Screen.height - downPos.y)))
			{
				scrollPosition2.y += drag.y - oldDrag2;
			}
			oldDrag2 = drag.y;
			GUILayout.BeginArea(stackRect, backStyle);
			scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
			Sample sample = null;
			try
			{
				sample = samples[selectedLog.sampleId];
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label(selectedLog.condition, stackLabelStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y * 0.25f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(selectedLog.stacktrace, stackLabelStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y);
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.BeginHorizontal();
			GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample.time.ToString("0.0"), nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(scenes[sample.loadedScene], nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample.memory.ToString("0.000"), nonStyle);
			GUILayout.Space(size.x);
			GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(sample.fpsText, nonStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		else
		{
			GUILayout.BeginArea(stackRect, backStyle);
			GUILayout.EndArea();
			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.EndArea();
		}
	}

	public void OnGUIDraw()
	{
		if (show)
		{
			screenRect.x = 0f;
			screenRect.y = 0f;
			screenRect.width = Screen.width;
			screenRect.height = Screen.height;
			getDownPos();
			logsRect.x = 0f;
			logsRect.y = size.y * 2f;
			logsRect.width = Screen.width;
			logsRect.height = (float)Screen.height * 0.75f - size.y * 2f;
			stackRectTopLeft.x = 0f;
			stackRect.x = 0f;
			stackRectTopLeft.y = (float)Screen.height * 0.75f;
			stackRect.y = (float)Screen.height * 0.75f;
			stackRect.width = Screen.width;
			stackRect.height = (float)Screen.height * 0.25f - size.y;
			detailRect.x = 0f;
			detailRect.y = (float)Screen.height - size.y * 3f;
			detailRect.width = Screen.width;
			detailRect.height = size.y * 3f;
			if (currentView == ReportView.Info)
			{
				DrawInfo();
			}
			else if (currentView == ReportView.Logs)
			{
				drawToolBar();
				DrawLogs();
			}
		}
	}

	private bool isGestureDone()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				gestureDetector.Clear();
				gestureCount = 0;
			}
			else if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[0].phase == TouchPhase.Ended)
			{
				gestureDetector.Clear();
			}
			else if (Input.touches[0].phase == TouchPhase.Moved)
			{
				Vector2 position = Input.touches[0].position;
				if (gestureDetector.Count == 0 || (position - gestureDetector[gestureDetector.Count - 1]).magnitude > 10f)
				{
					gestureDetector.Add(position);
				}
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			gestureDetector.Clear();
			gestureCount = 0;
		}
		else if (Input.GetMouseButton(0))
		{
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			if (gestureDetector.Count == 0 || (vector - gestureDetector[gestureDetector.Count - 1]).magnitude > 10f)
			{
				gestureDetector.Add(vector);
			}
		}
		if (gestureDetector.Count < 10)
		{
			return false;
		}
		gestureSum = Vector2.zero;
		gestureLength = 0f;
		Vector2 rhs = Vector2.zero;
		for (int i = 0; i < gestureDetector.Count - 2; i++)
		{
			Vector2 vector2 = gestureDetector[i + 1] - gestureDetector[i];
			float magnitude = vector2.magnitude;
			gestureSum += vector2;
			gestureLength += magnitude;
			float num = Vector2.Dot(vector2, rhs);
			if (num < 0f)
			{
				gestureDetector.Clear();
				gestureCount = 0;
				return false;
			}
			rhs = vector2;
		}
		int num2 = (Screen.width + Screen.height) / 4;
		if (gestureLength > (float)num2 && gestureSum.magnitude < (float)(num2 / 2))
		{
			gestureDetector.Clear();
			gestureCount++;
			if (gestureCount >= numOfCircleToShow)
			{
				return true;
			}
		}
		return false;
	}

	private bool isDoubleClickDone()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				lastClickTime = -1f;
			}
			else if (Input.touches[0].phase == TouchPhase.Began)
			{
				if (lastClickTime == -1f)
				{
					lastClickTime = Time.realtimeSinceStartup;
				}
				else
				{
					if (Time.realtimeSinceStartup - lastClickTime < 0.2f)
					{
						lastClickTime = -1f;
						return true;
					}
					lastClickTime = Time.realtimeSinceStartup;
				}
			}
		}
		else if (Input.GetMouseButtonDown(0))
		{
			if (lastClickTime == -1f)
			{
				lastClickTime = Time.realtimeSinceStartup;
			}
			else
			{
				if (Time.realtimeSinceStartup - lastClickTime < 0.2f)
				{
					lastClickTime = -1f;
					return true;
				}
				lastClickTime = Time.realtimeSinceStartup;
			}
		}
		return false;
	}

	private int getTotalCount()
	{
		int num = 0;
		if (collapse)
		{
			if (showLog)
			{
				num += numOfCollapsedLogs;
			}
			if (showWarning)
			{
				num += numOfCollapsedLogsWarning;
			}
			if (showError)
			{
				num += numOfCollapsedLogsError;
			}
		}
		else
		{
			if (showLog)
			{
				num += numOfLogs;
			}
			if (showWarning)
			{
				num += numOfLogsWarning;
			}
			if (showError)
			{
				num += numOfLogsError;
			}
		}
		return num;
	}

	private Vector2 getDownPos()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began)
			{
				downPos = Input.touches[0].position;
				return downPos;
			}
		}
		else if (Input.GetMouseButtonDown(0))
		{
			downPos.x = Input.mousePosition.x;
			downPos.y = Input.mousePosition.y;
			return downPos;
		}
		return Vector2.zero;
	}

	private Vector2 getDrag()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				return Vector2.zero;
			}
			return Input.touches[0].position - downPos;
		}
		if (Input.GetMouseButton(0))
		{
			mousePosition = Input.mousePosition;
			return mousePosition - downPos;
		}
		return Vector2.zero;
	}

	private void calculateStartIndex()
	{
		startIndex = (int)(scrollPosition.y / size.y);
		startIndex = Mathf.Clamp(startIndex, 0, getTotalCount());
	}

	private void doShow()
	{
		show = true;
		currentView = ReportView.Logs;
		base.gameObject.AddComponent<ReporterGUI>();
		try
		{
			base.gameObject.SendMessage("OnShowReporter");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		fpsText = fps.ToString("0.000");
		gcTotalMemory = (float)GC.GetTotalMemory(false) / 1024f / 1024f;
		if (string.IsNullOrEmpty(scenes[LegacySceneManager.LoadedLevel]))
		{
			scenes[LegacySceneManager.LoadedLevel] = LegacySceneManager.LoadedLevelName;
		}
		float num = Time.realtimeSinceStartup - lastUpdate;
		fps = 1f / num;
		lastUpdate = Time.realtimeSinceStartup;
		calculateStartIndex();
		if (!show && isGestureDone())
		{
			doShow();
		}
		float num2 = Time.realtimeSinceStartup - lastUpdate2;
		if (num2 > 1f)
		{
			lastUpdate2 = Time.realtimeSinceStartup;
			Application.RegisterLogCallback(CaptureLog);
		}
	}

	private void CaptureLog(string condition, string stacktrace, LogType type)
	{
		float num = 0f;
		string empty = string.Empty;
		if (cachedString.ContainsKey(condition))
		{
			empty = cachedString[condition];
		}
		else
		{
			empty = condition;
			cachedString.Add(empty, empty);
			num += (float)((!string.IsNullOrEmpty(empty)) ? (empty.Length * 2) : 0);
			num += (float)IntPtr.Size;
		}
		string empty2 = string.Empty;
		if (cachedString.ContainsKey(stacktrace))
		{
			empty2 = cachedString[stacktrace];
		}
		else
		{
			empty2 = stacktrace;
			cachedString.Add(empty2, empty2);
			num += (float)((!string.IsNullOrEmpty(empty2)) ? (empty2.Length * 2) : 0);
			num += (float)IntPtr.Size;
		}
		bool flag = false;
		addSample();
		Log log = new Log();
		log.logType = (_LogType)type;
		log.condition = empty;
		log.stacktrace = empty2;
		log.sampleId = samples.Count - 1;
		Log log2 = log;
		num += log2.GetMemoryUsage();
		logsMemUsage += num / 1024f / 1024f;
		if (TotalMemUsage > maxSize)
		{
			clear();
			Debug.Log("Memory Usage Reach" + maxSize + " mb So It is Cleared");
			return;
		}
		bool flag2 = false;
		if (logsDic.ContainsKey(empty) && logsDic[empty].stacktrace == stacktrace)
		{
			flag2 = false;
			logsDic[empty].count++;
		}
		else
		{
			flag2 = true;
			collapsedLogs.Add(log2);
			logsDic.Add(empty, log2);
			switch (type)
			{
			case LogType.Log:
				numOfCollapsedLogs++;
				break;
			case LogType.Warning:
				numOfCollapsedLogsWarning++;
				break;
			default:
				numOfCollapsedLogsError++;
				break;
			}
		}
		switch (type)
		{
		case LogType.Log:
			numOfLogs++;
			break;
		case LogType.Warning:
			numOfLogsWarning++;
			break;
		default:
			numOfLogsError++;
			break;
		}
		logs.Add(log2);
		if (!collapse || flag2)
		{
			bool flag3 = false;
			if (log2.logType == _LogType.Log && !showLog)
			{
				flag3 = true;
			}
			if (log2.logType == _LogType.Warning && !showWarning)
			{
				flag3 = true;
			}
			if (log2.logType == _LogType.Error && !showError)
			{
				flag3 = true;
			}
			if (log2.logType == _LogType.Assert && !showError)
			{
				flag3 = true;
			}
			if (log2.logType == _LogType.Exception && !showError)
			{
				flag3 = true;
			}
			if (!flag3)
			{
				currentLog.Add(log2);
				flag = true;
			}
		}
		if (flag)
		{
			calculateStartIndex();
			int totalCount = getTotalCount();
			int num2 = (int)((float)Screen.height * 0.75f / size.y);
			if (startIndex >= totalCount - num2)
			{
				scrollPosition.y += size.y;
			}
		}
		try
		{
			base.gameObject.SendMessage("OnLog", log2);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnLevelWasLoaded()
	{
		if (clearOnNewSceneLoaded)
		{
			clear();
		}
		currentScene = LegacySceneManager.LoadedLevelName;
		if (Debug.isDebugBuild)
		{
			Debug.Log("Scene " + LegacySceneManager.LoadedLevelName + " is loaded");
		}
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("Reporter_currentView", (int)currentView);
		PlayerPrefs.SetInt("Reporter_show", show ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_collapse", collapse ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_clearOnNewSceneLoaded", clearOnNewSceneLoaded ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showTime", showTime ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showScene", showScene ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showMemory", showMemory ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showFps", showFps ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showGraph", showGraph ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showLog", showLog ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showWarning", showWarning ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showError", showError ? 1 : 0);
		PlayerPrefs.Save();
	}

	private IEnumerator readInfo()
	{
		string prefFile = "build_info.txt";
		string url = prefFile;
		if (prefFile.IndexOf("://") == -1)
		{
			string streamingAssetsPath = Application.streamingAssetsPath;
			if (streamingAssetsPath == string.Empty)
			{
				streamingAssetsPath = Application.dataPath + "/StreamingAssets/";
			}
			url = Path.Combine(streamingAssetsPath, prefFile);
		}
		if (!url.Contains("://"))
		{
			url = "file://" + url;
		}
		WWW www = new WWW(url);
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError(www.error);
		}
		else
		{
			buildDate = www.text;
		}
	}
}
