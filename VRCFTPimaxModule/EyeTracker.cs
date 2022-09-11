// Cheers Guppy or NGenesis or whoever originally made this

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using VRCFaceTracking;

namespace VRCFTPimaxModule
{
	public enum CallbackType
	{
		Start,
		Stop,
		Update
	}
	
	public delegate void EyeTrackerEventHandler();

	public class EyeTracker
	{
		private EyeTrackerEventHandler _OnStartHandler;

		private EyeTrackerEventHandler _OnStopHandler;

		private EyeTrackerEventHandler _OnUpdateHandler;

		public EyeTrackerEventHandler OnStart { get; set; }

		public EyeTrackerEventHandler OnStop { get; set; }

		public EyeTrackerEventHandler OnUpdate { get; set; }

		public EyeState LeftEye { get; private set; }

		public EyeState RightEye { get; private set; }

		public EyeState RecommendedEye { get; private set; }

		public long Timestamp => _GetTimestamp();

		public bool Active => _IsActive();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "RegisterCallback")]
		private static extern void _RegisterCallback(CallbackType type, EyeTrackerEventHandler callback);

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "Start")]
		private static extern bool _Start();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "Stop")]
		private static extern void _Stop();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "IsActive")]
		private static extern bool _IsActive();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "GetTimestamp")]
		private static extern long _GetTimestamp();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "GetRecommendedEye")]
		private static extern Eye _GetRecommendedEye();

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "GetEyeParameter")]
		private static extern float _GetEyeParameter(Eye eye, EyeParameter param);

		[DllImport("PimaxEyeTracker.dll", EntryPoint = "GetEyeExpression")]
		private static extern float _GetEyeExpression(Eye eye, EyeExpression expression);

		public bool Start()
		{
			_OnStartHandler = _OnStart;
			_RegisterCallback(CallbackType.Start, _OnStartHandler);
			_OnStopHandler = _OnStop;
			_RegisterCallback(CallbackType.Stop, _OnStopHandler);
			_OnUpdateHandler = _OnUpdate;
			_RegisterCallback(CallbackType.Update, _OnUpdateHandler);
			return _Start();
		}

		public void Stop()
		{
			_Stop();
		}

		public float GetEyeParameter(Eye eye, EyeParameter param)
		{
			return _GetEyeParameter(eye, param);
		}

		public float GetEyeExpression(Eye eye, EyeExpression expression)
		{
			return _GetEyeExpression(eye, expression);
		}

		private void _OnUpdate()
		{
			if (Active)
			{
				LeftEye = new EyeState(Eye.Left, this);
				RightEye = new EyeState(Eye.Right, this);
				RecommendedEye = new EyeState(_GetRecommendedEye(), this);
				OnUpdate?.Invoke();
			}
		}

		private void _OnStart()
		{
			OnStart?.Invoke();
		}

		private void _OnStop()
		{
			OnStop?.Invoke();
		}

		public EyeTracker()
		{
			// Extract the Embedded DLL
			var dirName = Path.Combine(Utils.PersistentDataDirectory, "CustomLibs");
			if (!Directory.Exists(dirName))
				Directory.CreateDirectory(dirName);

			var dllPath = Path.Combine(dirName, "PimaxEyeTracker.dll");

			using (var stm = Assembly.GetExecutingAssembly()
				       .GetManifestResourceStream("VRCFTPimaxModule.PimaxEyeTracker.dll"))
			{
				try
				{
					using (Stream outFile = File.Create(dllPath))
					{
						const int sz = 4096;
						var buf = new byte[sz];
						while (true)
						{
							if (stm == null) continue;
							var nRead = stm.Read(buf, 0, sz);
							if (nRead < 1)
								break;
							outFile.Write(buf, 0, nRead);
						}
					}
					
					// Load the DLL
					LoadLibrary(dllPath);
				}
				catch (Exception e)
				{
					Logger.Error("Failed to get DLL: " + e.Message);
				}
			}
		}
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr LoadLibrary(string lpFileName);
	}
}