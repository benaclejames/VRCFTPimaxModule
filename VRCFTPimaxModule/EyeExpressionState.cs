using VRCFaceTracking.Params;

namespace VRCFTPimaxModule
{
    public enum EyeExpression
    {
        PupilCenterX,
        PupilCenterY,
        Openness,
        Blink
    }
    
    public struct EyeExpressionState
    {
        public Eye Eye { get; private set; }

        public Vector2 PupilCenter { get; private set; }

        public float Openness { get; private set; }

        public bool Blink { get; private set; }

        public EyeExpressionState(Eye eyeType, EyeTracker eyeTracker)
        {
            Eye = eyeType;
            PupilCenter = new Vector2(eyeTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterX), eyeTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterY));
            Openness = eyeTracker.GetEyeExpression(Eye, EyeExpression.Openness);
            Blink = eyeTracker.GetEyeExpression(Eye, EyeExpression.Blink) != 0f;
        }
    }
}