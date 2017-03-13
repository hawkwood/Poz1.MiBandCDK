using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Poz1.CDK.MiBand
{
    public class UserInfo : BaseInfo
    {
        #region Properties 

        private int _id;
        public int ID { get { return _id; } }

        private byte _gender;
        public Gender Gender { get { return (Gender)_gender; } }

        private byte _age;
        public int Age { get { return _age; } }

        private byte _height; // cm
        public int Height { get { return _height; } }

        private byte _weight; // kg
        public int Weight { get { return _weight; } }
        
        private string _alias = "";
        public string Alias { get { return _alias; } }

        private byte _type;
        public int Type { get { return _type; } }

        #endregion

        public UserInfo(int uid, int gender, int age, int height, int weight, string alias, int type)
        {
            _id = uid;
            _gender = (byte)gender;
            _age = (byte)age;
            _height = (byte)(height & 0xFF);
            _weight = (byte)weight;
            _alias = alias;
            _type = (byte)type;
        }
        public UserInfo(byte[] data)
        {
            if (data.Length < 20)
            {
                throw new InvalidDataException();
            }

            _id = data[3] << 24 | (data[2] & 0xFF) << 16 | (data[1] & 0xFF) << 8 | (data[0] & 0xFF);
            _alias = "";

            _gender = data[4];
            _age = data[5];
            _height = data[6];
            _weight = data[7];
            _type = data[8];
        }

        public byte[] ToByteArray(string btAddress)
        {
            byte[] aliasBytes;
            try
            {
                aliasBytes = Encoding.UTF8.GetBytes(_alias);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                aliasBytes = new byte[0];
            }

            var stream = new MemoryStream(20);

            stream.WriteByte((byte)(_id & 0xff));
            stream.WriteByte((byte)(_id >> 8 & 0xff));
            stream.WriteByte((byte)(_id >> 16 & 0xff));
            stream.WriteByte((byte)(_id >> 24 & 0xff));
            stream.WriteByte(_gender);
            stream.WriteByte(_age);
            stream.WriteByte(_height);
            stream.WriteByte(_weight);
            stream.WriteByte(_type);
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
