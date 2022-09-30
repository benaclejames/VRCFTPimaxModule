using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using VRCFaceTracking;

namespace VRCFTPimaxModule
{
	public class VRCFTPimaxModule : ExtTrackingModule
	{
		private readonly EyeTracker _eyeTracker = new EyeTracker();
		
		private float _lastGoodLeftX;
		private float _lastGoodRightX;
		private float _lastGoodLeftY;
		private float _lastGoodRightY;
		private int _blinkTimerCombined;
		private int _blinkTimerLeft;
		private int _blinkTimerRight;
		private int _trackingLossTimerLeft;
		private int _trackingLossTimerRight;
		private readonly SimpleMovingAverage _movingAverageLeftX, _movingAverageLeftY, _movingAverageRightX, _movingAverageRightY;
		
		// Configurable
		private class Config
		{
			public int MovingAverageBufferSize { get; set; } = 4;
			public int AverageSteps { get; set; } = 10;
			public int BlinkTime { get; set; } = 2;
			public int WinkTime { get; set; } = 6;
			public MinMaxRange XLeftRange { get; set; } = new MinMaxRange(0, 1);
			public MinMaxRange XRightRange { get; set; }  = new MinMaxRange(0, 1);
			public MinMaxRange YLeftRange { get; set; } = new MinMaxRange(0, 1);
			public MinMaxRange YRightRange { get; set; } = new MinMaxRange(0, 1);
			public int MovementMultiplierX { get; set; } = 1;
			public int MovementMultiplierY { get; set; } = 1;
		}
		
		private Config _config = new Config();
		
		public VRCFTPimaxModule()
		{
			// Get location of currently executing assembly, then create or open the config file
			var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "VRCFTPimaxModule.json");
			
			// If the config file doesn't exist, create it with default values
			if (!File.Exists(configPath))
				File.WriteAllText(configPath, JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
				
				// Now open the config file and read the values
			var config = File.ReadAllText(configPath);
			JsonSerializer.Deserialize<Config>(config);
			

			_movingAverageLeftX = new SimpleMovingAverage(_config.AverageSteps);
			_movingAverageLeftY = new SimpleMovingAverage(_config.AverageSteps);
			_movingAverageRightX = new SimpleMovingAverage(_config.AverageSteps);
			_movingAverageRightY = new SimpleMovingAverage(_config.AverageSteps);
		}
		
		public override (bool SupportsEye, bool SupportsLip) Supported => (true, false);

		public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
		{
			bool success = _eyeTracker.Start();
			_eyeTracker.OnUpdate =
				(EyeTrackerEventHandler)Delegate.Combine(_eyeTracker.OnUpdate,
					new EyeTrackerEventHandler(UpdateValues));
			return (success, false);
		}

		public void UpdateValues()
		{
			if (Status.EyeState != ModuleState.Active)
				return;
			
			float pupilCenterLeftX = _eyeTracker.GetEyeParameter(_eyeTracker.LeftEye.Eye, EyeParameter.PupilCenterX);
			float pupilCenterRightX = _eyeTracker.GetEyeParameter(_eyeTracker.RightEye.Eye, EyeParameter.PupilCenterX);
			float pupilCenterLeftY = _eyeTracker.GetEyeParameter(_eyeTracker.LeftEye.Eye, EyeParameter.PupilCenterY);
			float pupilCenterRightY = _eyeTracker.GetEyeParameter(_eyeTracker.RightEye.Eye, EyeParameter.PupilCenterY);
			float leftBlink = _eyeTracker.GetEyeExpression(_eyeTracker.LeftEye.Eye, EyeExpression.Blink);
			float rightBlink = _eyeTracker.GetEyeExpression(_eyeTracker.RightEye.Eye, EyeExpression.Blink);
			bool leftTrackingLoss = pupilCenterLeftX == 0f;
			bool rightTrackingLoss = pupilCenterRightX == 0f;
			
			if (leftTrackingLoss)
			{
				_trackingLossTimerLeft = 0;
				if (!rightTrackingLoss)
				{
					pupilCenterLeftX = pupilCenterRightX;
					pupilCenterLeftY = pupilCenterRightY;
				}
				else
				{
					pupilCenterLeftX = _lastGoodLeftX;
					pupilCenterLeftY = _lastGoodLeftY;
				}
			}
			else if (_trackingLossTimerLeft < _config.MovingAverageBufferSize)
			{
				_trackingLossTimerLeft++;
				if (!rightTrackingLoss)
				{
					pupilCenterLeftX = pupilCenterRightX;
					pupilCenterLeftY = pupilCenterRightY;
				}
				else
				{
					pupilCenterLeftX = _lastGoodLeftX;
					pupilCenterLeftY = _lastGoodLeftY;
				}
			}
			else
			{
				_lastGoodLeftX = pupilCenterLeftX;
				_lastGoodLeftY = pupilCenterLeftY;
			}

			if (rightTrackingLoss)
			{
				_trackingLossTimerRight = 0;
				if (!leftTrackingLoss)
				{
					pupilCenterRightX = pupilCenterLeftX;
					pupilCenterRightY = pupilCenterLeftY;
				}
				else
				{
					pupilCenterRightX = _lastGoodRightX;
					pupilCenterRightY = _lastGoodRightY;
				}
			}
			else if (_trackingLossTimerRight < _config.MovingAverageBufferSize)
			{
				_trackingLossTimerRight++;
				if (!leftTrackingLoss)
				{
					pupilCenterRightX = pupilCenterLeftX;
					pupilCenterRightY = pupilCenterLeftY;
				}
				else
				{
					pupilCenterRightX = _lastGoodRightX;
					pupilCenterRightY = _lastGoodRightY;
				}
			}
			else
			{
				_lastGoodRightX = pupilCenterRightX;
				_lastGoodRightY = pupilCenterRightY;
			}

			int num = 1;
			int num2 = 1;
			if (leftBlink == 1f && rightBlink == 1f)
			{
				_blinkTimerCombined++;
				_blinkTimerLeft++;
				_blinkTimerRight++;
				if (_blinkTimerCombined >= _config.BlinkTime)
				{
					num = 0;
					num2 = 0;
				}
			}

			if (leftBlink == 1f || rightBlink == 1f)
			{
				if (leftBlink == 1f)
				{
					_blinkTimerLeft++;
					if (_blinkTimerLeft >= _config.WinkTime)
					{
						num = 0;
					}
				}
				else
				{
					_blinkTimerLeft = 0;
				}

				if (rightBlink == 1f)
				{
					_blinkTimerRight++;
					if (_blinkTimerRight >= _config.WinkTime)
					{
						num2 = 0;
					}
				}
				else
				{
					_blinkTimerRight = 0;
				}
			}

			if (leftBlink == 0f && rightBlink == 0f)
			{
				_blinkTimerCombined = 0;
				_blinkTimerLeft = 0;
				_blinkTimerRight = 0;
			}

			UnifiedTrackingData.LatestEyeData.Left.Openness = num;
			UnifiedTrackingData.LatestEyeData.Right.Openness = num2;

			pupilCenterLeftX *= _config.MovementMultiplierX;
			pupilCenterLeftY *= _config.MovementMultiplierY;
			
			pupilCenterRightX *= _config.MovementMultiplierX;
			pupilCenterRightY *= _config.MovementMultiplierY;
			
			pupilCenterLeftX = _movingAverageLeftX.Update(pupilCenterLeftX);
			pupilCenterRightX = _movingAverageRightX.Update(pupilCenterRightX);
			pupilCenterLeftY = _movingAverageLeftY.Update(pupilCenterLeftY);
			pupilCenterRightY = _movingAverageRightY.Update(pupilCenterRightY);
			pupilCenterLeftX = NormalizeFloatAroundZero(pupilCenterLeftX, _config.XLeftRange);
			pupilCenterRightX = NormalizeFloatAroundZero(pupilCenterRightX, _config.XRightRange);
			pupilCenterLeftY = NormalizeFloatAroundZero(pupilCenterLeftY, _config.YLeftRange);
			pupilCenterRightY = NormalizeFloatAroundZero(pupilCenterRightY, _config.YRightRange);

			UnifiedTrackingData.LatestEyeData.Left.Look.x = pupilCenterLeftX;
			UnifiedTrackingData.LatestEyeData.Right.Look.x = pupilCenterRightX;
			
			float y = 0f - (pupilCenterLeftY + pupilCenterRightY / 2f);
			UnifiedTrackingData.LatestEyeData.Left.Look.y = y;
			UnifiedTrackingData.LatestEyeData.Right.Look.y = y;
		}
		
		private static float NormalizeFloatAroundZero(float input, MinMaxRange inputRange)
		{
			float num = 2f / (inputRange.Max - inputRange.Min);
			return -1f + num * (input - inputRange.Min);
		}

		public override Action GetUpdateThreadFunc()
		{
			// Return a null action as we're not using a thread
			return () => { };
		}

		public override void Teardown()
		{
			_eyeTracker.Stop();
		}
	}
}