using System;

namespace Poz1.CDK.MiBand
{
    public class ActivityData
    {
        private MiBandActivity _category;
        public MiBandActivity Category { get { return _category; } }

        private int _intensity;
        public int Intensity { get { return _intensity; } }

        private int _steps;
        public int Steps { get { return _steps; } }

        private int _heartRate;
        public int HeartRate { get { return _heartRate; } }

        private DateTime _startTime;
        public DateTime StartTime { get { return _startTime; } }

        public ActivityData(byte[] data, DateTime startTime)
        {
            _category = (MiBandActivity)data[0];
            _intensity = data[1];
            _steps = data[2];
            _heartRate = data[3];
            _startTime = startTime;
        }

        public override string ToString()
        {
            return "ActivityData [intensity=" + _intensity + ", steps=" + _steps + ", category=" + _category + "]";
        }
    }
}
