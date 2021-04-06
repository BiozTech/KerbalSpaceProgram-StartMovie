using System;
using System.IO;
using UnityEngine;

namespace StartMovie
{

	[KSPAddon(KSPAddon.Startup.Instantly, true)]

	public class StartMovie : MonoBehaviour
	{

		static bool isRecording = false;
		static int counter;
		static string activeDirectory = Settings.ShotsDirectory;

		void Update()
		{
			if (Input.GetKeyDown(Settings.KeyRecord))
			{
				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr))
				{
					StartMovieGUI.ToggleByKey();
				} else {
					if (Settings.IsEnabled)
					{
						isRecording = !isRecording;
						if (isRecording)
						{
							if (Settings.IsCaptureTimeMode)
							{
								Time.captureFramerate = Settings.Framerate;
								Settings.Log("Starting in CaptureTime mode.");
								Settings.Log(String.Format("Framerate = {0}.", Settings.Framerate));
							} else {
								Time.maximumDeltaTime = Settings.DeltaTimeLimit;
								Time.timeScale = 1f / Time.maximumDeltaTime / Settings.Framerate;
								Settings.Log("Starting in DeltaTime mode.");
								Settings.Log(String.Format("Framerate = {0}, DeltaTimeLimit = {1}, TimeScale = {2:0.000}.", Settings.Framerate, Time.maximumDeltaTime, Time.timeScale));
							}
							counter = 0;
							activeDirectory = Path.Combine(Settings.ShotsDirectory, DateTime.Now.ToString("yyMMdd-HHmmss"));
							if (!Directory.Exists(activeDirectory)) Directory.CreateDirectory(activeDirectory);
							Settings.Log("Recording…");
						} else {
							Time.captureFramerate = 0;
							Time.maximumDeltaTime = GameSettings.PHYSICS_FRAME_DT_LIMIT;
							Time.timeScale = 1f;
							Settings.Log(String.Format("Stopped. Recorded {0} frames.", counter));
						}
					}
				}
			}
			if (isRecording)
			{
				ScreenCapture.CaptureScreenshot(Path.Combine(activeDirectory, String.Format("{0:00000}.png", counter++)), Settings.SuperSize);
			}
		}

		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			Settings.Load();
		}

	}

}
