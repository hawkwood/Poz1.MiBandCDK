using System;

namespace Poz1.MiBandCDK.Model
{
    public class ActivityData
    {
        public MiBandActivity Category { get; }
        public int Intensity { get; }
        public int Steps { get;  }
        public int HeartRate { get ; }
		public DateTime StartTime { get ; }

        public ActivityData(byte[] data, DateTime startTime)
        {
            Category = (MiBandActivity)data[0];
            Intensity = data[1];
            Steps = data[2];
            HeartRate = data[3];
            StartTime = startTime;
        }

        public override string ToString()
        {
			return "ActivityData [Intensity=" + Intensity + ", Steps=" + Steps + ", Category=" + Category + "]";
        }
    }
}
