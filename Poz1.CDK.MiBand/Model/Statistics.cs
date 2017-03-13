using System.Text;

namespace Poz1.CDK.MiBand
{
    public class Statistics
    {
        #region Properties

        private int _wake;
        public int Wake { get { return _wake; } }

        private int _vibrate;
        public int Vibrate { get { return _vibrate; } }

        private int _light;
        public int Light { get { return _light; } }

        private int _conn;
        public int Conn { get { return _conn; } }

        private int _adv;
        public int Adv { get { return _adv; } }

        #endregion

        public Statistics(byte[] data)
        {
            _wake = data[0];
            _vibrate = data[1];
            _light = data[2];
            _conn = data[3];
            _adv = data[4];
        }

        public Statistics(int wake, int vibrate, int light, int conn, int adv)
        {
            _wake = wake;
            _vibrate = vibrate;
            _light = light;
            _conn = conn;
            _adv = adv;
        }

        public byte[] ToByteArray()
        {
            var _data = new byte[5];

            _data[0] = (byte)_wake;
            _data[1] = (byte)_vibrate;
            _data[2] = (byte)_light;
            _data[3] = (byte)_conn;
            _data[4] = (byte)_adv;

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
