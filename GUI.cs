using System;
using System.IO;
using UnityEngine;
using KSP.UI.Screens;

namespace StartMovie
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class StartMovieGUI : MonoBehaviour
	{

		static string strKeyRecord, strFramerate, strSuperSize, strDeltaTime, strDirectory;
		static bool isStrFramerateOk, isStrSuperSizeOk, isStrDeltaTimeOk, isStrDirectoryOk;
		static bool isCaptureTimeMode = Core.IsCaptureTimeMode, isDeltaTimeMode = !Core.IsCaptureTimeMode;

		static bool wasGUIVisible = Core.IsGUIVisible;
		static Rect windowPosition = new Rect(UnityEngine.Random.Range(0.23f, 0.27f) * Screen.width, UnityEngine.Random.Range(0.13f, 0.17f) * Screen.height, 0, 0);
		static GUILayoutOption[] layoutTextField;
		static GUIStyle styleTextField, styleBox, styleToggle, styleButtonSetKey, styleButtonApply;

		void OnGUI()
		{
			if (Core.IsGUIVisible)
			{
				GUIStyle windowStyle = new GUIStyle(GUI.skin.window) { padding = new RectOffset(11, 15, 20, 11) };
				windowPosition = GUILayout.Window(("StartMovieGUI").GetHashCode(), windowPosition, OnWindow, "StartMovie", windowStyle);
			}
		}

		void OnWindow(int windowId)
		{

			layoutTextField = new[] { GUILayout.Width(200f), GUILayout.Height(20f) };
			styleTextField = new GUIStyle(GUI.skin.textField) { padding = new RectOffset(4, 4, 3, 3) };
			styleBox = new GUIStyle(GUI.skin.box) { padding = new RectOffset(4, 4, 3, 3), alignment = TextAnchor.MiddleLeft };
			styleToggle = new GUIStyle(GUI.skin.toggle) { margin = new RectOffset(4, 4, 4, 8) };
			styleButtonSetKey = new GUIStyle(GUI.skin.button);
			styleButtonApply = new GUIStyle(GUI.skin.button) { margin = new RectOffset(90, 4, 13, 4) };

			GUILayout.BeginVertical(GUILayout.Width(300f));

			GUILayout.Space(6f);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Recording");
			ColorizeFieldIsEnabled(styleToggle, !Core.IsRecording);
			if (Core.IsEnabled != GUILayout.Toggle(Core.IsEnabled, Core.IsEnabled ? " Enabled" : " Disabled", styleToggle, GUILayout.Width(196f))) // without " " it looks ugly >_>
			{
				if (!Core.IsRecording) Core.IsEnabled = !Core.IsEnabled;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Key");
			ColorizeFieldIsWrong(styleBox, !Core.IsReadingKeys);
			GUILayout.Box(strKeyRecord, styleBox, GUILayout.Width(125f), GUILayout.Height(20f));
			GUILayout.Space(1f);
			ColorizeFieldIsEnabled(styleButtonSetKey, !Core.IsRecording);
			if (GUILayout.Button("Set", styleButtonSetKey, GUILayout.Width(70f), GUILayout.Height(20f)))
			{
				if (!Core.IsRecording)
				{
					Core.IsReadingKeys = !Core.IsReadingKeys;
					Initialize();
				}
			}
			if (Core.IsReadingKeys)
			{
				if (Event.current.isKey && Event.current.type == EventType.KeyUp && Event.current.keyCode != KeyCode.LeftAlt && Event.current.keyCode != KeyCode.RightAlt && Event.current.keyCode != KeyCode.AltGr)
				{
					Core.IsReadingKeys = false;
					Settings.KeyRecord = Event.current.keyCode;
					strKeyRecord = Settings.KeyRecord.ToString();
					Settings.Save();
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Framerate");
			if (Core.IsRecording || Core.IsReadingKeys)
			{
				ColorizeFieldIsWrong(styleBox, true);
				GUILayout.Box(strFramerate, styleBox, layoutTextField);
			} else {
				ColorizeFieldIsWrong(styleTextField, isStrFramerateOk);
				strFramerate = GUILayout.TextField(strFramerate, styleTextField, layoutTextField);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("SuperSize");
			if (Core.IsRecording || Core.IsReadingKeys)
			{
				ColorizeFieldIsWrong(styleBox, true);
				GUILayout.Box(strSuperSize, styleBox, layoutTextField);
			} else {
				ColorizeFieldIsWrong(styleTextField, isStrSuperSizeOk);
				strSuperSize = GUILayout.TextField(strSuperSize, styleTextField, layoutTextField);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			ColorizeFieldIsEnabled(styleToggle, !Core.IsRecording);
			if (isCaptureTimeMode != GUILayout.Toggle(isCaptureTimeMode, " CaptureTime mode", styleToggle))
			{
				if (!Core.IsRecording)
				{
					Core.IsCaptureTimeMode = isCaptureTimeMode = isStrDeltaTimeOk = true;
					isDeltaTimeMode = false;
					strDeltaTime = Settings.DeltaTimeLimit.ToString();
				}
			}
			GUILayout.Space(10f);
			if (isDeltaTimeMode != GUILayout.Toggle(isDeltaTimeMode, " DeltaTime mode", styleToggle))
			{
				if (!Core.IsRecording)
				{
					Core.IsCaptureTimeMode = isCaptureTimeMode = false;
					isDeltaTimeMode = true;
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("DeltaTime");
			if (Core.IsCaptureTimeMode || Core.IsRecording || Core.IsReadingKeys)
			{
				ColorizeFieldIsWrong(styleBox, true);
				GUILayout.Box(strDeltaTime, styleBox, layoutTextField);
			} else {
				ColorizeFieldIsWrong(styleTextField, isStrDeltaTimeOk);
				strDeltaTime = GUILayout.TextField(strDeltaTime, styleTextField, layoutTextField);
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Screenshots directory");
			if (Core.IsRecording || Core.IsReadingKeys)
			{
				ColorizeFieldIsWrong(styleBox, true);
				GUILayout.Box(strDirectory, styleBox);
			} else {
				ColorizeFieldIsWrong(styleTextField, isStrDirectoryOk);
				strDirectory = GUILayout.TextField(strDirectory, styleTextField);
			}

			ColorizeFieldIsEnabled(styleButtonApply, !Core.IsRecording && !Core.IsReadingKeys);
			if (GUILayout.Button("Apply", styleButtonApply, GUILayout.Width(125f)))
			{
				if (!Core.IsRecording && !Core.IsReadingKeys) ApplySettings();
			}

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		public static void ApplySettings()
		{
			bool isOk = true;

		//	KeyCode key;
		//	isStrKeyRecordOk = Enum.TryParse(strKeyRecord, out key);
		//	if (isStrKeyRecordOk) Settings.KeyRecord = key;
		//	isOk &= isStrKeyRecordOk;

			int fps;
			isStrFramerateOk = Int32.TryParse(strFramerate, out fps);
			if (isStrFramerateOk)
			{
				Settings.Framerate = Mathf.Max(1, fps);
				strFramerate = Settings.Framerate.ToString();
			}
			isOk &= isStrFramerateOk;

			int size;
			isStrSuperSizeOk = Int32.TryParse(strSuperSize, out size);
			if (isStrSuperSizeOk)
			{
				Settings.SuperSize = Mathf.Max(1, size);
				strSuperSize = Settings.SuperSize.ToString();
			}
			isOk &= isStrSuperSizeOk;

			if (!Core.IsCaptureTimeMode)
			{
				float dtLimit;
				isStrDeltaTimeOk = float.TryParse(strDeltaTime.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out dtLimit);
				if (isStrDeltaTimeOk)
				{
					Settings.DeltaTimeLimit = Math.Max(0.02f, dtLimit); // (ノಠ益ಠ)ノ彡┻━┻
					strDeltaTime = Settings.DeltaTimeLimit.ToString();
				}
				isOk &= isStrDeltaTimeOk;
			}

			if (strDirectory != string.Empty)
			{
				isStrDirectoryOk = Directory.Exists(strDirectory);
				if (isStrDirectoryOk) Settings.ShotsDirectory = strDirectory;
				isOk &= isStrDirectoryOk;
			} else {
				isStrDirectoryOk = true;
				Settings.ShotsDirectory = Settings.ShotsDirectoryDefault;
			}

			if (isOk) Settings.Save();
		}

		static void ColorizeFieldIsWrong(GUIStyle style, bool isOk)
		{
			style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = (isOk) ? Color.white : Color.red;
		}
		static void ColorizeFieldIsEnabled(GUIStyle style, bool isOk)
		{
			style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = (isOk) ? Color.white : Color.grey;
		}

		public static void ToggleByKey()
		{
			if (StartMovieToolbarButton.IsButtonAdded)
			{
				if (Core.IsGUIVisible) StartMovieToolbarButton.Button.SetFalse(); else StartMovieToolbarButton.Button.SetTrue();
			} else {
				Core.IsGUIVisible = !Core.IsGUIVisible;
			}
			Core.IsReadingKeys = false;
		}
		public static void ToggleByButton()
		{
			Core.IsGUIVisible = !Core.IsGUIVisible;
			Core.IsReadingKeys = false;
		}
		void OnShowUI()
		{
			if (wasGUIVisible) ToggleByKey();
			wasGUIVisible = false;
		}
		void OnHideUI()
		{
			wasGUIVisible = Core.IsGUIVisible;
			if (wasGUIVisible) ToggleByKey();
		}

		public static void Initialize()
		{
			strKeyRecord = Settings.KeyRecord.ToString();
			strFramerate = Settings.Framerate.ToString();
			strSuperSize = Settings.SuperSize.ToString();
			strDeltaTime = Settings.DeltaTimeLimit.ToString();
			strDirectory = Settings.DirectoryToOutput(Settings.ShotsDirectory, Settings.ShotsDirectoryDefault);
			isStrFramerateOk = isStrSuperSizeOk = isStrDeltaTimeOk = isStrDirectoryOk = true;
		}

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onHideUI.Add(OnHideUI);
			Initialize();
		}

	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class StartMovieToolbarButton : MonoBehaviour
	{

		public static ApplicationLauncherButton Button;
		public static bool IsButtonAdded = false;

		void Start()
		{

			if (!IsButtonAdded && ApplicationLauncher.Instance != null)
			{
				var resources = new System.Resources.ResourceManager("StartMovie.Resources", typeof(StartMovieToolbarButton).Assembly);
				Byte[] bytes = resources.GetObject("StartMovieToolbarIcon") as Byte[];
				Texture2D buttonTexture = new Texture2D(38, 38, TextureFormat.ARGB32, false);
				buttonTexture.LoadRawTextureData(bytes);
				buttonTexture.Apply();
				bool isAlreadyVisible = Core.IsGUIVisible;
				Core.IsGUIVisible = false;
				Button = ApplicationLauncher.Instance.AddModApplication(StartMovieGUI.ToggleByButton, StartMovieGUI.ToggleByButton, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
				if (isAlreadyVisible) Button.SetTrue(); // bruh
				IsButtonAdded = true;
			}

			// convert png to byte array
		//	Texture2D pngToTexture = GameDatabase.Instance.GetTexture("StartMovie/Textures/ToolbarIcon", false);
		//	Byte[] textureToBytes = pngToTexture.GetRawTextureData();
		//	File.WriteAllBytes("ToolbarIcon.txt", textureToBytes);

		}

	}

}
