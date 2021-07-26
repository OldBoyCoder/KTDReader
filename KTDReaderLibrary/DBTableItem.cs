﻿using System;
using System.IO;
using System.Text;

namespace KTDReaderLibrary
{
    class DbTableItem
    {
        internal readonly string FieldName;
        internal readonly DbDataType DataType;
        internal readonly int StartPosition;
        internal readonly int Length;

        public DbTableItem(BinaryReader br)
        {
            FieldName = GetFixedLengthString(br.ReadBytes(20));
            DataType = (DbDataType) br.ReadByte();
            StartPosition = br.ReadByte();
            Length = br.ReadByte();
        }
        private static string GetFixedLengthString(byte[] strByte)
        {
            var endLength = Array.IndexOf(strByte, (byte)0);
            if (endLength < 0) endLength = strByte.Length;
            return Encoding.Default.GetString(strByte, 0, endLength);
        }

    }
}
