using System;
using System.Text;

namespace Poz1.MiBandCDK.Model
{
    public class LEParameter
    {
        #region Properties

		public int MinConnectionInterval { get;}
		public int MaxConnectionInterval { get; }
		public int Latency { get; }
		public int Timeout { get; }
		public int AdvertisementInterval { get; }

        #endregion

        public LEParameter(int minConnectionInterval, int maxConnectionInterval, int latency, int timeout, int advertisementInterval)
        {
            MinConnectionInterval = minConnectionInterval;
            MaxConnectionInterval = maxConnectionInterval;
            Latency = latency;
			Timeout = timeout;
			AdvertisementInterval = advertisementInterval;
        }

        public LEParameter(byte[] data)
        {
            if (data.Length < 12)
            {
                throw new InvalidOperationException("Data is not valid for LEParameter");
            }

			MinConnectionInterval = (0xFF & data[1]) << 8 | (data[0] & 0xFF);
			MaxConnectionInterval = (0xFF & data[3]) << 8 | (data[2] & 0xFF);
			Latency = (0xFF & data[5]) << 8 | (data[6] & 0xFF);
			Timeout = (0xFF & data[7]) << 8 | (data[8] & 0xFF);
			AdvertisementInterval = (0xFF & data[11]) << 8 | (data[10] & 0xFF);
        }

        public byte[] ToByteArray()
        {
            byte[] _data = new byte[12];
			_data[0] = (byte)(MinConnectionInterval & 0xff);
			_data[1] = (byte)(0xff & MinConnectionInterval >> 8);
			_data[2] = (byte)(MaxConnectionInterval & 0xff);
			_data[3] = (byte)(0xff & MaxConnectionInterval >> 8);
			_data[4] = (byte)(Latency & 0xff);
			_data[5] = (byte)(0xff & Latency >> 8);
			_data[6] = (byte)(Timeout & 0xff);
			_data[7] = (byte)(0xff & Timeout >> 8);
            _data[8] = 0;
            _data[9] = 0;
			_data[10] = (byte)(AdvertisementInterval & 0xff);
			_data[11] = (byte)(0xff & AdvertisementInterval >> 8);

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
