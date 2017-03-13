using System;
using System.Text;

namespace Poz1.CDK.MiBand
{
    public class LEParameter
    {
        #region Properties

        private int _minConnectionInterval;
        public int MinConnectionInterval { get { return _minConnectionInterval; } }

        private int _maxConnectionInterval;
        public int MaxConnectionInterval { get { return _maxConnectionInterval; } }

        private int _latency;
        public int Latency { get { return _latency; } }

        private int _timeout;
        public int Timeout { get { return _timeout; } }

        private int _advertisementInterval;
        public int AdvertisementInterval { get { return _advertisementInterval; } }

        #endregion

        public LEParameter(int minConnectionInterval, int maxConnectionInterval, int latency, int timeout, int advertisementInterval)
        {
            _minConnectionInterval = minConnectionInterval;
            _maxConnectionInterval = maxConnectionInterval;
            _latency = latency;
            _timeout = timeout;
            _advertisementInterval = advertisementInterval;
        }
        public LEParameter(byte[] data)
        {
            if (data.Length < 12)
            {
                throw new InvalidOperationException("Data is not valid for LEParameter");
            }

            _minConnectionInterval = (0xFF & data[1]) << 8 | (data[0] & 0xFF);
            _maxConnectionInterval = (0xFF & data[3]) << 8 | (data[2] & 0xFF);
            _latency = (0xFF & data[5]) << 8 | (data[6] & 0xFF);
            _timeout = (0xFF & data[7]) << 8 | (data[8] & 0xFF);
            _advertisementInterval = (0xFF & data[11]) << 8 | (data[10] & 0xFF);
        }

        public byte[] ToByteArray()
        {
            byte[] _data = new byte[12];
            _data[0] = (byte)(_minConnectionInterval & 0xff);
            _data[1] = (byte)(0xff & _minConnectionInterval >> 8);
            _data[2] = (byte)(_maxConnectionInterval & 0xff);
            _data[3] = (byte)(0xff & _maxConnectionInterval >> 8);
            _data[4] = (byte)(_latency & 0xff);
            _data[5] = (byte)(0xff & _latency >> 8);
            _data[6] = (byte)(_timeout & 0xff);
            _data[7] = (byte)(0xff & _timeout >> 8);
            _data[8] = 0;
            _data[9] = 0;
            _data[10] = (byte)(_advertisementInterval & 0xff);
            _data[11] = (byte)(0xff & _advertisementInterval >> 8);

            return _data;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(160);
            stringBuilder.Append("[[[LeParameter]]]");
            stringBuilder.Append("\n  connIntMin: " + ((int)(((double)this.MinConnectionInterval) * 1.25d)) + "ms");
            stringBuilder.Append("\n  connIntMax: " + ((int)(((double)this.MaxConnectionInterval) * 1.25d)) + "ms");
            stringBuilder.Append("\n     latency: " + this.Latency + "ms");
            stringBuilder.Append("\n     timeout: " + (this.Timeout * 10) + "ms");
            //stringBuilder.append("\n     connInt: " + ((int)(((double)this.f11253e) * 1.25d)) + "ms");
            stringBuilder.Append("\n      advInt: " + ((int)(((double)this.AdvertisementInterval) * 0.625d)) + "ms");
            return stringBuilder.ToString();
        }


        public static LEParameter High
        {
            get
            {
                return new LEParameter(460, 500, 0, 500, 0);
            }
        }

        public static LEParameter Low
        {
            get
            {
                return new LEParameter(39, 49, 0, 500, 0);
            }
        }
    }
}
