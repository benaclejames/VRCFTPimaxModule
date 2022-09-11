using System;
using VRCFaceTracking;

namespace VRCFTPimaxModule
{
	public class VRCFTPimaxModule : VRCFaceTracking.ExtTrackingModule
	{
		private readonly EyeTracker _eyeTracker = new EyeTracker();
		
		private float lastGoodLeftX;
		private float lastGoodRightX;
		private float lastGoodLeftY;
		private float lastGoodRightY;
		private int blinkTimerCombined;
		private int blinkTimerLeft;
		private int blinkTimerRight;
		private int trackingLossTimerLeft;
		private int trackingLossTimerRight;
		
		// Configurable
		private int _movingAverageBufferSize = 4;
		private int _averageSteps = 10;
		private int _blinkTime = 2, _winkTime = 6;
		private SimpleMovingAverage MovingAverageLeftX, MovingAverageLeftY, MovingAverageRightX, MovingAverageRightY;
		private MinMaxRange _xLeftRange, _xRightRange, _yLeftRange, _yRightRange;

		public VRCFTPimaxModule()
		{
			MovingAverageLeftX = new SimpleMovingAverage(_averageSteps);
			MovingAverageLeftY = new SimpleMovingAverage(_averageSteps);
			MovingAverageRightX = new SimpleMovingAverage(_averageSteps);
			MovingAverageRightY = new SimpleMovingAverage(_averageSteps);
			
			_xLeftRange = new MinMaxRange(0, 1);
			_xRightRange = new MinMaxRange(0, 1);
			_yLeftRange = new MinMaxRange(0, 1);
			_yRightRange = new MinMaxRange(0, 1);
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
				trackingLossTimerLeft = 0;
				if (!rightTrackingLoss)
				{
					pupilCenterLeftX = pupilCenterRightX;
					pupilCenterLeftY = pupilCenterRightY;
				}
				else
				{
					pupilCenterLeftX = lastGoodLeftX;
					pupilCenterLeftY = lastGoodLeftY;
				}
			}
			else if (trackingLossTimerLeft < _movingAverageBufferSize)
			{
				trackingLossTimerLeft++;
				if (!rightTrackingLoss)
				{
					pupilCenterLeftX = pupilCenterRightX;
					pupilCenterLeftY = pupilCenterRightY;
				}
				else
				{
					pupilCenterLeftX = lastGoodLeftX;
					pupilCenterLeftY = lastGoodLeftY;
				}
			}
			else
			{
				lastGoodLeftX = pupilCenterLeftX;
				lastGoodLeftY = pupilCenterLeftY;
			}

			if (rightTrackingLoss)
			{
				trackingLossTimerRight = 0;
				if (!leftTrackingLoss)
				{
					pupilCenterRightX = pupilCenterLeftX;
					pupilCenterRightY = pupilCenterLeftY;
				}
				else
				{
					pupilCenterRightX = lastGoodRightX;
					pupilCenterRightY = lastGoodRightY;
				}
			}
			else if (trackingLossTimerRight < _movingAverageBufferSize)
			{
				trackingLossTimerRight++;
				if (!leftTrackingLoss)
				{
					pupilCenterRightX = pupilCenterLeftX;
					pupilCenterRightY = pupilCenterLeftY;
				}
				else
				{
					pupilCenterRightX = lastGoodRightX;
					pupilCenterRightY = lastGoodRightY;
				}
			}
			else
			{
				lastGoodRightX = pupilCenterRightX;
				lastGoodRightY = pupilCenterRightY;
			}

			int num = 1;
			int num2 = 1;
			if (leftBlink == 1f && rightBlink == 1f)
			{
				blinkTimerCombined++;
				blinkTimerLeft++;
				blinkTimerRight++;
				if (blinkTimerCombined >= _blinkTime)
				{
					num = 0;
					num2 = 0;
				}
			}

			if (leftBlink == 1f || rightBlink == 1f)
			{
				if (leftBlink == 1f)
				{
					blinkTimerLeft++;
					if (blinkTimerLeft >= _winkTime)
					{
						num = 0;
					}
				}
				else
				{
					blinkTimerLeft = 0;
				}

				if (rightBlink == 1f)
				{
					blinkTimerRight++;
					if (blinkTimerRight >= _winkTime)
					{
						num2 = 0;
					}
				}
				else
				{
					blinkTimerRight = 0;
				}
			}

			if (leftBlink == 0f && rightBlink == 0f)
			{
				blinkTimerCombined = 0;
				blinkTimerLeft = 0;
				blinkTimerRight = 0;
			}

			UnifiedTrackingData.LatestEyeData.Left.Openness = num;
			UnifiedTrackingData.LatestEyeData.Right.Openness = num2;
			
			pupilCenterLeftX = MovingAverageLeftX.Update(pupilCenterLeftX);
			pupilCenterRightX = MovingAverageRightX.Update(pupilCenterRightX);
			pupilCenterLeftY = MovingAverageLeftY.Update(pupilCenterLeftY);
			pupilCenterRightY = MovingAverageRightY.Update(pupilCenterRightY);
			pupilCenterLeftX = NormalizeFloatAroundZero(pupilCenterLeftX, _xLeftRange);
			pupilCenterRightX = NormalizeFloatAroundZero(pupilCenterRightX, _xRightRange);
			pupilCenterLeftY = NormalizeFloatAroundZero(pupilCenterLeftY, _yLeftRange);
			pupilCenterRightY = NormalizeFloatAroundZero(pupilCenterRightY, _yRightRange);

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