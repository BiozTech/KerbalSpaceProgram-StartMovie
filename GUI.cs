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
		static bool isCaptureTimeMode = Core.IsCaptureTimeMode;
		static bool isDeltaTimeMode = !Core.IsCaptureTimeMode;

		static bool wasGUIVisible = Core.IsGUIVisible;
		static Rect windowPosition = new Rect(UnityEngine.Random.Range(0.23f, 0.27f) * Screen.width, UnityEngine.Random.Range(0.13f, 0.17f) * Screen.height, 0, 0);

		void OnGUI()
		{
			if (Core.IsGUIVisible)
			{
				GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
				windowStyle.padding = new RectOffset(11, 15, 20, 11);
				windowPosition = GUILayout.Window(("StartMovieGUI").GetHashCode(), windowPosition, OnWindow, "StartMovie", windowStyle);
			}
		}

		void OnWindow(int windowId)
		{

			GUILayoutOption[] textFieldLayoutOptions = { GUILayout.Width(200f), GUILayout.Height(20f) };
			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.alignment = TextAnchor.MiddleLeft;
			textFieldStyle.padding = boxStyle.padding = new RectOffset(4, 4, 3, 3);
			GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
			toggleStyle.margin = new RectOffset(4, 4, 4, 8);
			GUIStyle buttonApplyStyle = new GUIStyle(GUI.skin.button);
			buttonApplyStyle.margin = new RectOffset(90, 4, 13, 4);

			GUILayout.BeginVertical(GUILayout.Width(300f));

			GUILayout.Space(6f);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Recording");
			ColorizeFieldIsEnabled(toggleStyle, !Core.IsRecording);
			if (Core.IsEnabled != GUILayout.Toggle(Core.IsEnabled, Core.IsEnabled ? " Enabled" : " Disabled", toggleStyle, textFieldLayoutOptions)) // without " " it looks ugly >_>
			{
				if (!Core.IsRecording) Core.IsEnabled = !Core.IsEnabled;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Key");
			ColorizeFieldIsWrong(boxStyle, !Core.IsReadingKeys);
			GUILayout.Box(strKeyRecord, boxStyle, GUILayout.Width(125f), GUILayout.Height(20f));
			GUILayout.Space(1f);
			if (GUILayout.Button("Set", GUILayout.Width(70f), GUILayout.Height(20f)))
			{
				if (!Core.IsRecording) Core.IsReadingKeys = !Core.IsReadingKeys;
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
			if (Core.IsRecording)
			{
				ColorizeFieldIsWrong(boxStyle, true);
				GUILayout.Box(strFramerate, boxStyle, textFieldLayoutOptions);
			} else {
				ColorizeFieldIsWrong(textFieldStyle, isStrFramerateOk);
				strFramerate = GUILayout.TextField(strFramerate, textFieldStyle, textFieldLayoutOptions);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("SuperSize");
			if (Core.IsRecording)
			{
				ColorizeFieldIsWrong(boxStyle, true);
				GUILayout.Box(strSuperSize, boxStyle, textFieldLayoutOptions);
			} else {
				ColorizeFieldIsWrong(textFieldStyle, isStrSuperSizeOk);
				strSuperSize = GUILayout.TextField(strSuperSize, textFieldStyle, textFieldLayoutOptions);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			ColorizeFieldIsEnabled(toggleStyle, !Core.IsRecording);
			if (isCaptureTimeMode != GUILayout.Toggle(isCaptureTimeMode, " CaptureTime mode", toggleStyle))
			{
				if (!Core.IsRecording)
				{
					Core.IsCaptureTimeMode = isCaptureTimeMode = isStrDeltaTimeOk = true;
					isDeltaTimeMode = false;
					strDeltaTime = Settings.DeltaTimeLimit.ToString();
				}
			}
			GUILayout.Space(10f);
			if (isDeltaTimeMode != GUILayout.Toggle(isDeltaTimeMode, " DeltaTime mode", toggleStyle))
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
			if (Core.IsCaptureTimeMode || Core.IsRecording)
			{
				ColorizeFieldIsWrong(boxStyle, true);
				GUILayout.Box(strDeltaTime, boxStyle, textFieldLayoutOptions);
			} else {
				ColorizeFieldIsWrong(textFieldStyle, isStrDeltaTimeOk);
				strDeltaTime = GUILayout.TextField(strDeltaTime, textFieldStyle, textFieldLayoutOptions);
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Screenshots directory");
			if (Core.IsRecording)
			{
				ColorizeFieldIsWrong(boxStyle, true);
				GUILayout.Box(strDirectory, boxStyle);
			} else {
				ColorizeFieldIsWrong(textFieldStyle, isStrDirectoryOk);
				strDirectory = GUILayout.TextField(strDirectory, textFieldStyle);
			}

			if (GUILayout.Button("Apply", buttonApplyStyle, GUILayout.Width(125f)))
			{
				ApplySettings();
			}

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		public static void ApplySettings()
		{
			if (!Core.IsRecording)
			{

				bool isOk = true;

			//	KeyCode key;
			//	isStrKeyRecordOk = Enum.TryParse(strKeyRecord, out key);
			//	if (isStrKeyRecordOk) Settings.KeyRecord = key;
			//	isOk &= isStrKeyRecordOk;

				int fps;
				isStrFramerateOk = Int32.TryParse(strFramerate, out fps);
				if (isStrFramerateOk) Settings.Framerate = fps;
				isOk &= isStrFramerateOk;

				int size;
				isStrSuperSizeOk = Int32.TryParse(strSuperSize, out size);
				if (isStrSuperSizeOk) Settings.SuperSize = size;
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
			strDirectory = Settings.ShotsDirectoryToOutput();
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
				Byte[] bytes = resources.GetObject("ToolbarIcon") as Byte[];
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
