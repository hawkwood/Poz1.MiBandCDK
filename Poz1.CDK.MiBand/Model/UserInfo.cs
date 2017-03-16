using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Poz1.MiBandCDK.Model
{
    public class UserInfo : BaseInfo
    {
        #region Properties 

		public int ID { get; }
		public Gender Gender { get; }
		public int Age { get; }
		public int Height { get; } //cm
		public int Weight { get; } //kg
		public string Alias { get; }
		public int Type { get; }

        #endregion

        public UserInfo(int uid, int gender, int age, int height, int weight, string alias, int type)
        {
			ID = uid;
			Gender = (Gender)gender;
            Age = (byte)age;
            Height = (byte)(height & 0xFF);
            Weight = (byte)weight;
            Alias = alias;
            Type = (byte)type;
        }

        public UserInfo(byte[] data)
        {
            if (data.Length < 20)
            {
                throw new InvalidDataException();
            }

            ID = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
			Alias = "";

			Gender = (Gender)data[4];
			Age = data[5];
			Height = data[6];
			Weight = data[7];
			Type = data[8];
        }

        public byte[] ToByteArray(string btAddress)
        {
            byte[] aliasBytes;
            try
            {
				aliasBytes = Encoding.UTF8.GetBytes(Alias);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                aliasBytes = new byte[0];
            }

            var stream = new MemoryStream(20);

			stream.WriteByte((byte)(ID & 0xff));
            stream.WriteByte((byte)(ID >> 8 & 0xff));
            stream.WriteByte((byte)(ID >> 16 & 0xff));
            stream.WriteByte((byte)(ID >> 24 & 0xff));
			stream.WriteByte((byte)Gender);
			stream.WriteByte((byte)Age);
			stream.WriteByte((byte)Height);
			stream.WriteByte((byte)Weight);
			stream.WriteByte((byte)Type);
            stream.WriteByte(4);
            stream.WriteByte(0);

            if (aliasBytes.Length <= 8)
            {
                stream.Write(aliasBytes, 0, aliasBytes.Length);
                stream.Write(new byte[8 - aliasBytes.Length], 0, 8 - aliasBytes.Length);
            }
            else
            {
                stream.Write(aliasBytes, 0, 8);
            }

            byte[] crcSequence = new byte[19];
            for (int u = 0; u < crcSequence.Length; u++)
                crcSequence[u] = stream.ToArray()[u];

            byte crcb = (byte)(GetCRC8(crcSequence) ^ int.Parse(btAddress.Substring(btAddress.Length - 2 , 16)) & 0xff);
            stream.WriteByte(crcb);
            return stream.ToArray();
        }      
        public override string ToString()
        {
            return "ID: " + ID + " Gender: " + Gender + " Age: " + Age + " Height: " + Height + " Weight: " + Weight + " Alias: " + Alias + " Type: " + Type;  
        }
    }
}
