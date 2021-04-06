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
		static bool isStrKeyRecordOk, isStrFramerateOk, isStrSuperSizeOk, isStrDeltaTimeOk, isStrDirectoryOk;

		static bool isGUIVisible = false;
		static bool isCaptureTimeMode = Settings.IsCaptureTimeMode;
		static bool isDeltaTimeMode = !Settings.IsCaptureTimeMode;

		static Rect windowPosition = new Rect(0.25f * Screen.width + UnityEngine.Random.Range(-40f, 40f), 0.15f * Screen.height + UnityEngine.Random.Range(-40f, 40f), 0, 0);

		void OnGUI()
		{
			if (isGUIVisible)
			{
				GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
				windowStyle.padding = new RectOffset(11, 15, 20, 11);
				windowPosition = GUILayout.Window(("StartMovieGUI").GetHashCode(), windowPosition, OnWindow, "StartMovie Settings", windowStyle);
			}
		}

		void OnWindow(int windowId)
		{

			GUILayoutOption[] textFieldLayoutOptions = { GUILayout.Width(200f), GUILayout.Height(20f) };
			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.alignment = TextAnchor.MiddleLeft;
			boxStyle.padding = new RectOffset(3, 3, 3, 3);
			GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
			toggleStyle.margin = new RectOffset(4, 4, 4, 8);
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.margin = new RectOffset(4, 4, 13, 4);

			GUILayout.BeginVertical(GUILayout.Width(300f));

			GUILayout.Space(6f);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Recording");
			if (Settings.IsEnabled != GUILayout.Toggle(Settings.IsEnabled, Settings.IsEnabled ? " Enabled" : " Disabled", textFieldLayoutOptions)) // without " " it looks ugly >_>
			{
				Settings.IsEnabled = !Settings.IsEnabled;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Key");
			ColorizeFieldIsWrong(textFieldStyle, isStrKeyRecordOk);
			strKeyRecord = GUILayout.TextField(strKeyRecord, textFieldStyle, textFieldLayoutOptions);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Framerate");
			ColorizeFieldIsWrong(textFieldStyle, isStrFramerateOk);
			strFramerate = GUILayout.TextField(strFramerate, textFieldStyle, textFieldLayoutOptions);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("SuperSize");
			ColorizeFieldIsWrong(textFieldStyle, isStrSuperSizeOk);
			strSuperSize = GUILayout.TextField(strSuperSize, textFieldStyle, textFieldLayoutOptions);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (isCaptureTimeMode != GUILayout.Toggle(isCaptureTimeMode, " CaptureTime mode", toggleStyle))
			{
				Settings.IsCaptureTimeMode = isCaptureTimeMode = isStrDeltaTimeOk = true;
				isDeltaTimeMode = false;
				strDeltaTime = Settings.DeltaTimeLimit.ToString();
			}
			GUILayout.Space(10f);
			if (isDeltaTimeMode != GUILayout.Toggle(isDeltaTimeMode, " DeltaTime mode", toggleStyle))
			{
				Settings.IsCaptureTimeMode = isCaptureTimeMode = false;
				isDeltaTimeMode = true;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("DeltaTime");
			if (Settings.IsCaptureTimeMode)
			{
				GUILayout.Box(strDeltaTime, boxStyle, textFieldLayoutOptions);
			} else {
				ColorizeFieldIsWrong(textFieldStyle, isStrDeltaTimeOk);
				strDeltaTime = GUILayout.TextField(strDeltaTime, textFieldStyle, textFieldLayoutOptions);
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Screenshots directory");
			ColorizeFieldIsWrong(textFieldStyle, isStrDirectoryOk);
			strDirectory = GUILayout.TextField(strDirectory, textFieldStyle);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Apply", buttonStyle, GUILayout.Width(125f)))
			{

				bool isOk = true;

				KeyCode key;
				isStrKeyRecordOk = Enum.TryParse(strKeyRecord, out key);
				if (isStrKeyRecordOk) Settings.KeyRecord = key;
				isOk &= isStrKeyRecordOk;

				int fps;
				isStrFramerateOk = Int32.TryParse(strFramerate, out fps);
				if (isStrFramerateOk) Settings.Framerate = fps;
				isOk &= isStrFramerateOk;

				int size;
				isStrSuperSizeOk = Int32.TryParse(strSuperSize, out size);
				if (isStrSuperSizeOk) Settings.SuperSize = size;
				isOk &= isStrSuperSizeOk;

				if (!Settings.IsCaptureTimeMode)
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
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUI.DragWindow();

		}

		public static void ToggleByKey()
		{
			if (StartMovieToolbarButton.IsButtonAdded)
			{
				if (isGUIVisible) StartMovieToolbarButton.Button.SetFalse(); else StartMovieToolbarButton.Button.SetTrue();
			} else {
				isGUIVisible = !isGUIVisible;
			}
		}
		public static void ToggleByButton()
		{
			isGUIVisible = !isGUIVisible;
		}

		public static void Initialize()
		{
			strKeyRecord = Settings.KeyRecord.ToString();
			strFramerate = Settings.Framerate.ToString();
			strSuperSize = Settings.SuperSize.ToString();
			strDeltaTime = Settings.DeltaTimeLimit.ToString();
			strDirectory = Settings.ShotsDirectoryToOutput();
			isStrKeyRecordOk = isStrFramerateOk = isStrSuperSizeOk = isStrDeltaTimeOk = isStrDirectoryOk = true;
		}

		static void ColorizeFieldIsWrong(GUIStyle style, bool isOk)
		{
			style.normal.textColor = style.focused.textColor = style.hover.textColor = (isOk) ? Color.white : Color.red;
		}

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
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
				Button = ApplicationLauncher.Instance.AddModApplication(StartMovieGUI.ToggleByButton, StartMovieGUI.ToggleByButton, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
				IsButtonAdded = true;
			}

			// convert png to byte array
		//	Texture2D pngToTexture = GameDatabase.Instance.GetTexture("StartMovie/Textures/ToolbarIcon", false);
		//	Byte[] textureToBytes = pngToTexture.GetRawTextureData();
		//	File.WriteAllBytes("ToolbarIcon.txt", textureToBytes);

		}

	}

}
