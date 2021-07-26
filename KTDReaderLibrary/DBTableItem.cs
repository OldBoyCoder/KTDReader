using System.IO;
using System.Text;

namespace KTDReaderLibrary
{
    class DbTableItem
    {
        // ReSharper disable once InconsistentNaming
        internal const int D_TYPE_STRING = 1;
        // ReSharper disable once InconsistentNaming
        internal const int D_TYPE_REFERENCE = 0;
        internal string FieldName;
        internal  int DataType;
        internal int StartPosition;
        internal int Length;


        public DbTableItem(BinaryReader eraf)
        {
            var b = eraf.ReadBytes(20);
            Init(b, eraf.ReadByte(), eraf.ReadByte(), eraf.ReadByte());
        }

        private void Init(byte[] fieldName, int type, int startPosition, int length)
        {
            FieldName = DbTableItem.GetFixedLengthString(fieldName);
            DataType = type;
            StartPosition = startPosition;
            Length = length;
        }

        public static string ReadFixedLengthString(BinaryReader eraf, int length)
        {
            var r = eraf.ReadBytes(length);
            return DbTableItem.GetFixedLengthString(r);
        }

        private static string GetFixedLengthString(byte[] strByte)
        {
            int endLength = strByte.Length;
            int i = 0;
            while (i < strByte.Length)
            {
                if (strByte[i] == 0)
                {
                    endLength = i;
                    break;
                }
                ++i;
            }

            return Encoding.Default.GetString(strByte, 0, endLength);
        }

    }
}
