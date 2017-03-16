using System.Text;

namespace Poz1.MiBandCDK.Model
{
    public class Statistics
    {
        #region Properties

		public int Wake { get; }
		public int Vibrate { get; }
		public int Light { get; }
		public int Conn { get; }
		public int Adv { get; }

        #endregion

        public Statistics(byte[] data)
        {
			Wake = data[0];
			Vibrate = data[1];
			Light = data[2];
			Conn = data[3];
			Adv = data[4];
        }

        public Statistics(int wake, int vibrate, int light, int conn, int adv)
        {
			Wake = wake;
			Vibrate = vibrate;
			Light = light;
			Conn = conn;
			Adv = adv;
        }

        public byte[] ToByteArray()
        {
            var _data = new byte[5];

			_data[0] = (byte)Wake;
			_data[1] = (byte)Vibrate;
			_data[2] = (byte)Light;
			_data[3] = (byte)Conn;
			_data[4] = (byte)Adv;

            return _data;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[[[Statistic]]]");
            stringBuilder.Append("\n  Wake: " + ((int)(((double)this.Wake) * 1.25d)) + "ms");
            stringBuilder.Append("\n  Vibrate: " + ((int)(((double)this.Vibrate) * 1.25d)) + "ms");
            stringBuilder.Append("\n  Light: " + this.Light + "ms");
            stringBuilder.Append("\n  Conn: " + (this.Conn * 10) + "ms");
            stringBuilder.Append("\n  Adv: " + ((int)(((double)this.Adv) * 0.625d)) + "ms");
            return stringBuilder.ToString();
        }
    }
}
