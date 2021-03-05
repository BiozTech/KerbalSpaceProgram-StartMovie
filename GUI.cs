using System;
using System.IO;
using UnityEngine;

namespace StartMovie
{

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]

	public class StartMovieGUI : MonoBehaviour
	{

		static string strKeyRecord = Settings.KeyRecord.ToString();
		static string strFramerate = Settings.Framerate.ToString();
		static string strSuperSize = Settings.SuperSize.ToString();
		static string strDirectory = Settings.ShotsDirectoryToOutput;

		static bool isGUIVisible = false;
		static bool isStrKeyRecordOk = true;
		static bool isStrFramerateOk = true;
		static bool isStrSuperSizeOk = true;
		static bool isStrDirectoryOk = true;

		static Rect windowPosition = new Rect(100, 250, 0, 0);
		static GUILayoutOption TFSettingWidth = GUILayout.Width(150f);

		void OnGUI()
		{
			if (isGUIVisible)
			{
				windowPosition = GUILayout.Window(0, windowPosition, OnWindow, "StartMovie Settings");
			}
		}

		void OnWindow(int windowId)
		{

			GUIStyle TFstyle = new GUIStyle(GUI.skin.textField);
			GUILayout.BeginVertical(GUILayout.Width(250f));

			GUILayout.BeginHorizontal();
			GUILayout.Label("Key");
			ColorizeTextField(TFstyle, isStrKeyRecordOk);
			strKeyRecord = GUILayout.TextField(strKeyRecord, TFstyle, TFSettingWidth);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Framerate");
			ColorizeTextField(TFstyle, isStrFramerateOk);
			strFramerate = GUILayout.TextField(strFramerate, TFstyle, TFSettingWidth);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("SuperSize");
			ColorizeTextField(TFstyle, isStrSuperSizeOk);
			strSuperSize = GUILayout.TextField(strSuperSize, TFstyle, TFSettingWidth);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Screenshots directory");
			ColorizeTextField(TFstyle, isStrDirectoryOk);
			strDirectory = GUILayout.TextField(strDirectory, TFstyle, TFSettingWidth);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Apply", GUILayout.Width(100f)))
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

		static void ColorizeTextField(GUIStyle style, bool isStrOk)
		{
			style.normal.textColor = style.focused.textColor = style.hover.textColor = (isStrOk) ? Color.white : Color.red;
		}

		public static void Toggle()
		{
			isGUIVisible = !isGUIVisible;
		}

		public static void Initialize()
		{
			strKeyRecord = Settings.KeyRecord.ToString();
			strFramerate = Settings.Framerate.ToString();
			strSuperSize = Settings.SuperSize.ToString();
			strDirectory = Settings.ShotsDirectoryToOutput;
		}

	}

}
