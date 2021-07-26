using System;
using System.Collections.Generic;
using System.Text;

namespace KTDReaderLibrary
{
    class DataUnpacker
    {
        private List<List<object>> referenceTables;
        private DbTableHeader header;

        public DataUnpacker(DbTableHeader header, List<List<Object>> referenceTables)
        {
            this.referenceTables = referenceTables;
            this.header = header;
        }

        public DbRecord Unpack(byte[] line, int length)
        {
            DbRecord record = new DbRecord();
            byte[] unpackedData = UnpackRecord(line, length);
            int i = 0;
            while (i < header.ColumnItems.Length)
            {
                int colLength = header.ColumnItems[i].Length;
                if (colLength == 0)
                {
                    colLength = unpackedData.Length - header.ColumnItems[i].StartPosition;
                }
                byte[] colBytes = new byte[colLength];
                int j = 0;
                while (j < colLength)
                {
                    colBytes[j] = unpackedData[header.ColumnItems[i].StartPosition + j];
                    ++j;
                }
                record.Values.Add(header.ColumnItems[i].FieldName, Encoding.ASCII.GetString(colBytes));
                ++i;
            }
            return record;
        }

        private byte[] UnpackRecord(byte[] line, int length)
        {
            byte[] result = new byte[header.GetUnpackedRecordSize(length)];
            int i = 0;
            while (i < header.ColumnItems.Length)
            {
                int j;
                if (header.ColumnItems[i].DataType == DbTableItem.D_TYPE_REFERENCE)
                {
                    byte[] tempb = (byte[])referenceTables[header.ColumnReferenceIndex[i]][DataTransformations.ReadUnsignedShortReverse(line, header.ColumnPackedPosition[i])];
                    j = 0;
                    while (j < header.ColumnItems[i].Length)
                    {
                        result[header.ColumnItems[i].StartPosition + j] = tempb[j];
                        ++j;
                    }
                }
                else
                {
                    int colLength = header.ColumnItems[i].Length;
                    if (colLength == 0)
                    {
                        colLength = result.Length - header.ColumnItems[i].StartPosition;
                    }
                    j = 0;
                    while (j < colLength)
                    {
                        result[header.ColumnItems[i].StartPosition + j] = line[header.ColumnPackedPosition[i] + j];
                        ++j;
                    }
                }
                ++i;
            }
            return result;
        }

    }
}
