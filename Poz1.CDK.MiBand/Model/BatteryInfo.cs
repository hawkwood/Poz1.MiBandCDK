using System;

namespace Poz1.MiBandCDK.Model
{
    public class BatteryInfo
    {
        #region Properties

		public int Level { get; }
		public int Cycles { get; }
		public BatteryStatus Status { get; }
		public DateTime LastChargedDate { get; }

        #endregion

        public BatteryInfo (byte[] data)
        {
            if (data.Length < 10)
            {
                throw new InvalidOperationException("Data is not valid for BatteryInfo");
            }

            Level = data[0];
			Status = StatusFromByte(data[9]);
			Cycles = 0xffff & (0xff & data[7] | (0xff & data[8]) << 8);
			LastChargedDate = new DateTime(data[1] + 2000, data[2], data[3], data[4], data[5], data[6]);
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
