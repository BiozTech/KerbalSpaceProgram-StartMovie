using System;
using System.IO;
using UnityEngine;

namespace StartMovie
{

	public static class Core
	{

		public static bool IsRecording = false, IsEnabled = false, IsGUIVisible = false, IsCaptureTimeMode = true, IsReadingKeys = false;

		public static void Log(object message)
		{
			Debug.Log("[StartMovie] " + message);
		}

	}

	public static class Settings
	{

		public static KeyCode KeyRecord = KeyCode.F6;
		public static int Framerate = 60, SuperSize = GameSettings.SCREENSHOT_SUPERSIZE;
		public static float DeltaTimeLimit = GameSettings.PHYSICS_FRAME_DT_LIMIT; // 0.02 ~ 0.35 -> inf
		public static string ShotsDirectoryDefault = Path.Combine(KSPUtil.ApplicationRootPath, "Screenshots"), ShotsDirectory = ShotsDirectoryDefault;

		static string pluginDataDir = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/BiozTech/PluginData"), settingsFileName = Path.Combine(pluginDataDir, "StartMovieSettings.cfg");
		static string configTagMain = "StartMovieSettings", configTagKey = "Key", configTagFramerate = "Framerate", configTagSize = "SuperSize", configTagDeltaTimeLimit = "DeltaTimeLimit", configTagShotsDirectory = "ScreenshotsDirectory";

		public static void Load()
		{
			if (!File.Exists(settingsFileName))
			{
				Save();
				Core.Log("Have no settings file, huh? Here is one for you (づ｡◕‿‿◕｡)づ");
				return;
			}
			ConfigNode configNode = ConfigNode.Load(settingsFileName);
			if (!configNode.HasNode(configTagMain))
			{
				BackupAndSave();
				Core.Log("General Failure reading settings file");
				return;
			}
			configNode = configNode.GetNode(configTagMain); // ;)
			try
			{

				bool isOk = true, isOkCurrent;

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
					if (isOkCurrent)
					{
						isOkCurrent = 1 <= fps;
						Framerate = isOkCurrent ? fps : 1;
					}
				}
				isOk &= isOkCurrent;

				isOkCurrent = configNode.HasValue(configTagSize);
				if (isOkCurrent)
				{
					int size;
					isOkCurrent = Int32.TryParse(configNode.GetValue(configTagSize), out size);
					if (isOkCurrent)
					{
						isOkCurrent = 1 <= size;
						SuperSize = isOkCurrent ? size : 1;
					}
				}
				isOk &= isOkCurrent;

				isOkCurrent = configNode.HasValue(configTagDeltaTimeLimit);
				if (isOkCurrent)
				{
					float dtLimit;
					isOkCurrent = float.TryParse(configNode.GetValue(configTagDeltaTimeLimit).Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out dtLimit);
					if (isOkCurrent)
					{
						isOkCurrent = 0.02f <= dtLimit;
						DeltaTimeLimit = isOkCurrent ? dtLimit : 0.02f;
					}
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
					Core.Log("General Failure reading settings file");
				}

			}
			catch (Exception stupid)
			{
				BackupAndSave();
				Core.Log("Not to worry, we are still flying half a ship.");
				Core.Log(stupid.Message);
			}
			StartMovieGUI.Initialize();
		}

		public static void Save()
		{
			ConfigNode configNode = new ConfigNode(configTagMain);
			configNode.AddValue(configTagKey, KeyRecord, "Key to start recording (Alt+Key to open menu)");
			configNode.AddValue(configTagFramerate, Framerate, "Target framerate");
			configNode.AddValue(configTagSize, SuperSize, "SCREENSHOT_SUPERSIZE");
			configNode.AddValue(configTagDeltaTimeLimit, DeltaTimeLimit, "Affects on physics accuracy");
			configNode.AddValue(configTagShotsDirectory, DirectoryToOutput(ShotsDirectory, ShotsDirectoryDefault), "Folder the shots will be saved to");
			if (!Directory.Exists(pluginDataDir)) Directory.CreateDirectory(pluginDataDir);
			File.WriteAllText(settingsFileName, configNode.ToString(), System.Text.Encoding.Unicode); // non-latin letters? nah, never heard
		}
		static void BackupAndSave()
		{
			string settingsFileBackupName = settingsFileName + ".bak";
			if (File.Exists(settingsFileBackupName)) File.Delete(settingsFileBackupName);
			File.Move(settingsFileName, settingsFileBackupName);
			Save();
		}

		public static string DirectoryToOutput(string directory, string directoryDefault)
		{
			return (ComparePaths(directory, directoryDefault) != 0) ? directory : string.Empty;
		}
		public static int ComparePaths(string path1, string path2)
		{
			return String.Compare(Path.GetFullPath(path1).TrimEnd('\\'), Path.GetFullPath(path2).TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase);
		}

	}

}
