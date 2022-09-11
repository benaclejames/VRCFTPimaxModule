using System;

namespace VRCFTPimaxModule
{
    public class SimpleMovingAverage
    {
        private readonly int _k;

        private readonly float[] _values;

        private int _index;

        private float _sum;

        private int _count;

        private int _totalBuffer;

        private float _floatingSum;

        public SimpleMovingAverage(int k)
        {
            if (k <= 0)
            {
                throw new ArgumentOutOfRangeException("k", "Must be greater than 0");
            }
            _k = k;
            _totalBuffer = 20;
            _values = new float[_k];
        }

        public float Update(float nextInput)
        {
            if (_count < _totalBuffer && nextInput > 1E-05f)
            {
                _count++;
            }
            else if (_count > 0 && nextInput < 1E-05f)
            {
                _count--;
            }
            _sum = _sum - _values[_index] + nextInput;
            if (_count == _totalBuffer)
            {
                _floatingSum = _sum;
            }
            _values[_index] = nextInput;
            _index = (_index + 1) % _k;
            if (_sum / (float)_k <= 1E-05f || _count < _totalBuffer)
            {
                return 0f;
            }
            return _floatingSum / (float)_k;
        }
    }

}