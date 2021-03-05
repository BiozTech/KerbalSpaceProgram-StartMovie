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

		void Start()
		{
			Settings.Load();
		}

		void Update()
		{
			if (Input.GetKeyDown(Settings.KeyRecord))
			{
				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr))
				{
					StartMovieGUI.Toggle();
				} else {
					isRecording = !isRecording;
					if (isRecording)
					{
						counter = 0;
						//Time.maximumDeltaTime = Settings.DeltaTimeLimit;
						//Time.timeScale = 1f / Time.maximumDeltaTime / Settings.Framerate;
						Time.captureFramerate = Settings.Framerate;
						activeDirectory = Path.Combine(Settings.ShotsDirectory, String.Format("{0:yyMMdd-HHmmss}", DateTime.Now));
						if (!Directory.Exists(activeDirectory)) Directory.CreateDirectory(activeDirectory);
						Settings.Log("Recording…");
					} else {
						//Time.maximumDeltaTime = GameSettings.PHYSICS_FRAME_DT_LIMIT;
						//Time.timeScale = 1f;
						Time.captureFramerate = 0;
						Settings.Log(String.Format("Stopped. Recorded {0} frames.", counter));
					}
				}
			}
			if (isRecording)
			{
				ScreenCapture.CaptureScreenshot(Path.Combine(activeDirectory, String.Format("{0:00000}.png", counter)), Settings.SuperSize);
				counter++;
			}
		}

	}

}
