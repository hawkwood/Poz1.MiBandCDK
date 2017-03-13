using System;

namespace Poz1.CDK.MiBand
{
    public class BatteryInfo
    {
        #region Properties

        private int _level;
        public int Level { get { return _level; } }

        private int _cycles;
        public int Cycles { get { return _cycles; } }

        private BatteryStatus _status;
        public BatteryStatus Status { get { return _status; } }

        private DateTime _lastChargedDate;
        public DateTime LastChargedDate { get { return _lastChargedDate; } }

        #endregion

        public BatteryInfo (byte[] data)
        {
            if (data.Length < 10)
            {
                throw new InvalidOperationException("Data is not valid for BatteryInfo");
            }

            _level = data[0];
            _status = StatusFromByte(data[9]);
            _cycles = 0xffff & (0xff & data[7] | (0xff & data[8]) << 8);
            _lastChargedDate = new DateTime(data[1] + 2000, data[2], data[3], data[4], data[5], data[6]);
        }

        private BatteryStatus StatusFromByte(byte b)
        {
            switch (b)
            {
                case 1:
                    return BatteryStatus.Low;
                case 2:
                    return BatteryStatus.Charging;
                case 3:
                    return BatteryStatus.Full;
                case 4:
                    return BatteryStatus.NotCharging;

                default:
                    return BatteryStatus.Normal;
            }
        }

        public override string ToString()
        {
            return "Cycles: " + Cycles + " - Charge Level: " + Level + " - Status: " + Status + " - Last Recharged On: " + LastChargedDate.ToString();
        }
    }
}
