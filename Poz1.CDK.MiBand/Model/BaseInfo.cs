namespace Poz1.MiBandCDK.Model
{
    public class BaseInfo
    {
        internal int GetCRC8(byte[] seq)
        {
            int len = seq.Length;
            int i = 0;
            byte crc = 0x00;

            while (len-- > 0)
            {
                byte extract = seq[i++];
                for (byte tempI = 8; tempI != 0; tempI--)
                {
                    byte sum = (byte)((crc & 0xff) ^ (extract & 0xff));
                    sum = (byte)((sum & 0xff) & 0x01);
                    crc = (byte)((uint)(crc & 0xff) >> 1);
                    if (sum != 0)
                    {
                        crc = (byte)((crc & 0xff) ^ 0x8c);
                    }
                    extract = (byte)((uint)(extract & 0xff) >> 1);
                }
            }
            return (crc & 0xff);
        }

        internal int GetInt(byte[] data, int from, int len)
        {
            int ret = 0;
            for (int i = 0; i < len; ++i)
            {
                ret |= (data[from + i] & 255) << i * 8;
            }
            return ret;
        }
    }
}
