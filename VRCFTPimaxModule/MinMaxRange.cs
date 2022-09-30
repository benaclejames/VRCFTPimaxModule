namespace VRCFTPimaxModule
{
    public class MinMaxRange
    {
        public float Max { get; set; }
        public float Min { get; set; }

        public MinMaxRange(float min, float max)
        {
            Max = max;
            Min = min;
        }
    }
}