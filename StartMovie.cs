using System;
using System.IO;
using UnityEngine;

namespace StartMovie
{

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]

	public class StartMovie : MonoBehaviour
	{

		static string activeDirectory = Settings.ShotsDirectory;
		static int counter;
		static bool isRecording = false;

		public void Start()
		{
			Settings.Load();
		}

		public void Update()
		{
			//if (Event.current.isKey && Event.current.keyCode == Settings.KeyRecord)
			if (Input.GetKeyDown(Settings.KeyRecord))
			{
				isRecording = !isRecording;
				if (isRecording)
				{
					counter = 0;
					Time.maximumDeltaTime = Settings.deltaTimeMin;
					Time.timeScale = 1f / Settings.deltaTimeMin / Settings.Framerate;
					activeDirectory = Path.Combine(Settings.ShotsDirectory, String.Format("{0:yyMMdd-HHmmss}", DateTime.Now));
					if (!Directory.Exists(activeDirectory)) Directory.CreateDirectory(activeDirectory);
					Log("Recording…");
				} else {
					Time.maximumDeltaTime = GameSettings.PHYSICS_FRAME_DT_LIMIT;
					Time.timeScale = 1.0f;
					Log(String.Format("Stopped. Recorded {0} frames.", counter));
				}
			}
			if (isRecording)
			{
				ScreenCapture.CaptureScreenshot(Path.Combine(activeDirectory, String.Format("{0:00000}.png", counter)), Settings.SuperSize);
				counter++;
			}
		}

		static void Log(string message)
		{
			Debug.Log("StartMovie » " + message);
		}

		static class Settings
		{

			static public KeyCode KeyRecord = KeyCode.F6;
			static public int Framerate = 60;
			static public int SuperSize = GameSettings.SCREENSHOT_SUPERSIZE;
			static public string ShotsDirectory = Path.Combine(KSPUtil.ApplicationRootPath, "Screenshots");

			static public float deltaTimeMin = 0.02f;

			static string pluginDataDir = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/StartMovie/PluginData");
			static string settingsFileName = Path.Combine(pluginDataDir, "settings.cfg");
			static string configTagMain = "StartMovieSettings";
			static string configTagKey = "Key";
			static string configTagFramerate = "Framerate";
			static string configTagSize = "SuperSize";
			static string configTagShotsDirectory = "ScreenshotsDirectory";

			static public void Load()
			{
				if (!File.Exists(settingsFileName))
				{
					Save();
					Log("Have no settings file, huh? Here is one for you (づ｡◕‿‿◕｡)づ");
					return;
				}
				ConfigNode configNode = ConfigNode.Load(settingsFileName);
				if (!configNode.HasNode(configTagMain))
				{
					BackupAndSave();
					Log("General Failure reading settings file");
					return;
				}
				configNode = configNode.GetNode(configTagMain); // ;)
				try
				{
					bool isOk = true;
					bool isOkCurrent;

					isOkCurrent = configNode.HasValue(configTagKey);
					if (isOkCurrent)
					{
						KeyCode key;
						isOkCurrent = Enum.TryParse(configNode.GetValue(configTagKey), out key);
						if (isOkCurrent) KeyRecord = key;
					}
					isOk &= isOkCurrent;

					isOkCurrent = configNode.HasValue(configTagFramerate);
					if (isOkCurrent)
					{
						int fps;
						isOkCurrent = Int32.TryParse(configNode.GetValue(configTagFramerate), out fps);
						if (isOkCurrent) Framerate = fps;
					}
					isOk &= isOkCurrent;

					isOkCurrent = configNode.HasValue(configTagSize);
					if (isOkCurrent)
					{
						int size;
						isOkCurrent = Int32.TryParse(configNode.GetValue(configTagSize), out size);
						if (isOkCurrent) SuperSize = size;
					}
					isOk &= isOkCurrent;

					isOkCurrent = configNode.HasValue(configTagShotsDirectory);
					if (isOkCurrent)
					{
						string directory = configNode.GetValue(configTagShotsDirectory);
						if (directory != string.Empty)
						{
							isOkCurrent = Directory.Exists(directory);
							if (isOkCurrent) ShotsDirectory = directory;
						}
					}
					isOk &= isOkCurrent;

					if (!isOk)
					{
						BackupAndSave();
						Log("General Failure reading settings file");
					}
				}
				catch (Exception stupid)
				{
					BackupAndSave();
					Log("Not to worry, we are still flying half a ship.");
					Log(stupid.Message);
				}
			}

			static public void Save()
			{
				if (!Directory.Exists(pluginDataDir)) Directory.CreateDirectory(pluginDataDir);
				ConfigNode configNode = new ConfigNode(configTagMain);
				configNode.AddValue(configTagKey, KeyRecord, "Key to start recording");
				configNode.AddValue(configTagFramerate, Framerate, "Desirable framerate");
				configNode.AddValue(configTagSize, SuperSize, "SCREENSHOT_SUPERSIZE");
				configNode.AddValue(configTagShotsDirectory, (ShotsDirectory != Path.Combine(KSPUtil.ApplicationRootPath, "Screenshots")) ? ShotsDirectory : string.Empty, "Folder the shots will be saved to");
				File.WriteAllText(settingsFileName, configNode.ToString(), System.Text.Encoding.Unicode); // non-latin letters? nah, never heard
			}

			static void BackupAndSave()
			{
				string settingsFileBackupName = settingsFileName + ".bak";
				if (File.Exists(settingsFileBackupName)) File.Delete(settingsFileBackupName);
				File.Move(settingsFileName, settingsFileBackupName);
				Save();
			}

		}

	}

}
