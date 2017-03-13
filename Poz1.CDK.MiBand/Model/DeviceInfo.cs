using System;

namespace Poz1.CDK.MiBand
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

        private int _feature;
        public int Feature { get { return _feature; } }

        private int _appearance;
        public int Appearance { get { return _appearance; } }

        private string _deviceId;
        public string DeviceId { get { return _deviceId; } }

        private int _profileVersion;
        public int ProfileVersion { get { return _profileVersion; } }
        
        private int _firmwareVersion;
        public int FirmwareVersion { get { return _firmwareVersion; } }

        private int _hwVersion;
        public int HWVersion { get { return _hwVersion; } }

        private int _heartRateFirmwareVersion;
        public int HeartRateFirmwareVersion
        {
            get
            {
                return _heartRateFirmwareVersion;
            }
        }

        public MiBandModel Model
        {
            get
            {
                if (_hwVersion == 2)
                    return MiBandModel.MI1;

                if (_feature == 5 && _appearance == 0 || _feature == 0 && _hwVersion == 208)
                    return MiBandModel.MI1A;

                if (_feature == 4 && _appearance == 0 || _feature == 4 && _hwVersion == 4)
                    return MiBandModel.MI1S;

                throw new InvalidOperationException("Model not Recognized");
            }
        }
        #endregion

        public DeviceInfo(byte[] data)
        {
            if ((data.Length == 16 || data.Length == 20) && CheckChecksum(data))
            {
                _deviceId = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}:{6:X2}:{7:X2}", data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);
                _profileVersion = BitConverter.ToInt32(data, 8);
                _firmwareVersion = BitConverter.ToInt32(data, 12);
                _hwVersion = data[6] & 255;
                _appearance = data[5] & 255;
                _feature = data[4] & 255;

                if (data.Length == 20)
                {
                    int s = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        s |= (data[16 + i] & 255) << i * 8;
                    }

                    _heartRateFirmwareVersion = s;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid Checksum");
            }
        }

        private bool CheckChecksum(byte[] data)
        {
            int crc8 = GetCRC8(new byte[] { data[0], data[1], data[2], data[3], data[4], data[5], data[6] });
            return (data[7] & 255) == (crc8 ^ data[3] & 255);
        }

        public override string ToString()
        {
            return "DeviceInfo{" +
                    "deviceId='" + DeviceId + '\'' +
                    ", profileVersion=" + ProfileVersion +
                    ", fwVersion=" + _firmwareVersion +
                    ", hwVersion=" + _hwVersion +
                    ", feature=" + _feature +
                    ", appearance=" + _appearance +
                    ", fw2Version (hr)=" + _heartRateFirmwareVersion +
                    '}';
        }
    }
}