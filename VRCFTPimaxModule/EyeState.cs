using VRCFaceTracking.Params;

namespace VRCFTPimaxModule
{ 
	public enum Eye
	{
		Any,
		Left,
		Right
	}
	
	public enum EyeParameter
	{
		GazeX,
		GazeY,
		GazeRawX,
		GazeRawY,
		GazeSmoothX,
		GazeSmoothY,
		GazeOriginX,
		GazeOriginY,
		GazeOriginZ,
		GazeDirectionX,
		GazeDirectionY,
		GazeDirectionZ,
		GazeReliability,
		PupilCenterX,
		PupilCenterY,
		PupilDistance,
		PupilMajorDiameter,
		PupilMajorUnitDiameter,
		PupilMinorDiameter,
		PupilMinorUnitDiameter,
		Blink,
		Openness,
		UpperEyelid,
		LowerEyelid
	}
	
	public struct EyeState
	{
		public Eye Eye { get; private set; }

		public Vector2 Gaze { get; private set; }

		public Vector2 GazeRaw { get; private set; }

		public Vector2 GazeSmooth { get; private set; }

		public Vector3 GazeOrigin { get; private set; }

		public Vector3 GazeDirection { get; private set; }

		public float GazeReliability { get; private set; }

		public Vector2 PupilCenter { get; private set; }

		public float PupilDistance { get; private set; }

		public float PupilMajorDiameter { get; private set; }

		public float PupilMajorUnitDiameter { get; private set; }

		public float PupilMinorDiameter { get; private set; }

		public float PupilMinorUnitDiameter { get; private set; }

		public float Blink { get; private set; }

		public float Openness { get; private set; }

		public float UpperEyelid { get; private set; }

		public float LowerEyelid { get; private set; }

		public EyeExpressionState Expression { get; private set; }

		public EyeState(Eye eyeType, EyeTracker eyeTracker)
		{
			Eye = eyeType;
			Gaze = new Vector2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeY));
			GazeRaw = new Vector2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeRawX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeRawY));
			GazeSmooth = new Vector2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothY));
			GazeOrigin = new Vector3(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginY),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginZ));
			GazeDirection = new Vector3(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionY),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionZ));
			GazeReliability = eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeReliability);
			PupilDistance = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilDistance);
			PupilMajorDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorDiameter);
			PupilMajorUnitDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorUnitDiameter);
			PupilMinorDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorDiameter);
			PupilMinorUnitDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorUnitDiameter);
			Blink = eyeTracker.GetEyeParameter(Eye, EyeParameter.Blink);
			UpperEyelid = eyeTracker.GetEyeParameter(Eye, EyeParameter.UpperEyelid);
			LowerEyelid = eyeTracker.GetEyeParameter(Eye, EyeParameter.LowerEyelid);
			Openness = eyeTracker.GetEyeParameter(Eye, EyeParameter.Openness);
			PupilCenter = new Vector2(eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterX),
				eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterY));
			Expression = new EyeExpressionState(eyeType, eyeTracker);
		}
	}
}