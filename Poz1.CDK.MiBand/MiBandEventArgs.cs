using System;

namespace Poz1.MiBandCDK
{
    public class HeartRateEventArgs : EventArgs
    {
        public int HeartRate;
        public HeartRateEventArgs(int heartRate) 
        {
           HeartRate = heartRate;
        }
    }

    public class RealTimeStepsEventArgs : EventArgs
    {
        public int Steps;
        public RealTimeStepsEventArgs(int steps)
        {
            Steps = steps;
        }
    }
}