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


        public DbTableItem(ExtendedRandomAccessFile eraf)
        {
            var b = eraf.read(20);
            Init(b, eraf.readUnsignedByte(), eraf.readUnsignedByte(), eraf.readUnsignedByte());
        }

        private void Init(byte[] fieldName, int type, int startPosition, int length)
        {
            FieldName = DbTableItem.GetFixedLengthString(fieldName);
            DataType = type;
            StartPosition = startPosition;
            Length = length;
        }

        public static string ReadFixedLengthString(ExtendedRandomAccessFile eraf, int length)
        {
            var r = eraf.read(length);
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
