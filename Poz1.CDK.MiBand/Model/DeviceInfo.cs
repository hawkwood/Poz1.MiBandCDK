using System;

namespace Poz1.MiBandCDK.Model
{

    //Byte[0-3] --> device ID
    //Byte[4 - 7] --> profile version
    //Byte[8 - 9] --> feature
    //Byte[10 - 11] --> appearance
    //Byte[12 - 13] --> hardware version
    //Byte[14 - 15] --> firmware version

    /// <summary>
    /// 0xff01, DEVICE_INFO The length of value is 16 bytes.
    /// </summary>
    public class DeviceInfo : BaseInfo
    {
        #region Properties

		public int Feature { get; }
        public int Appearance { get; }
		public string MacAddress { get;}
        public int ProfileVersion { get; }
        public int FirmwareVersion { get; }
        public int HWVersion { get; }
		public int HeartRateFirmwareVersion { get;}

        public MiBandModel Model
        {
            get
            {
				if (HWVersion == 2)
                    return MiBandModel.MI1;

				if (Feature == 5 && Appearance == 0 || Feature == 0 && HWVersion == 208)
                    return MiBandModel.MI1A;

				if (Feature == 4 && Appearance == 0 || Feature == 4 && HWVersion == 4)
                    return MiBandModel.MI1S;

                throw new InvalidOperationException("Model not Recognized");
            }
        }
        #endregion

        public DeviceInfo(byte[] data)
        {
            if ((data.Length == 16 || data.Length == 20) && CheckChecksum(data))
            {
                MacAddress = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}:{6:X2}:{7:X2}", data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);
				ProfileVersion = BitConverter.ToInt32(data, 8);
				FirmwareVersion = BitConverter.ToInt32(data, 12);
				HWVersion = data[6] & 255;
				Appearance = data[5] & 255;
				Feature = data[4] & 255;

                if (data.Length == 20)
                {
                    int s = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        s |= (data[16 + i] & 255) << i * 8;
                    }

					HeartRateFirmwareVersion = s;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid Checksum");
            }
        }

        bool CheckChecksum(byte[] data)
        {
            var crc8 = GetCRC8(new byte[] { data[0], data[1], data[2], data[3], data[4], data[5], data[6] });
            return (data[7] & 255) == (crc8 ^ data[3] & 255);
        }

        public override string ToString()
        {
            return "DeviceInfo{" +
                    "DeviceId='" + MacAddress + '\'' +
                    ", ProfileVersion=" + ProfileVersion +
                    ", FWVersion=" + FirmwareVersion +
                    ", HWVersion=" + HWVersion +
                    ", Feature=" + Feature +
                    ", Appearance=" + Appearance +
                    ", FW2Version (hr)=" + HeartRateFirmwareVersion +
                    '}';
        }
    }
}