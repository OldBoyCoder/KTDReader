using System;
using System.Collections.Generic;
using System.Text;

namespace KTDReaderLibrary
{
    class DataUnpacker
    {
        private readonly List<List<object>> _referenceTables;
        private readonly DbTableHeader _header;

        public DataUnpacker(DbTableHeader header, List<List<Object>> referenceTables)
        {
            _referenceTables = referenceTables;
            _header = header;
        }

        public DbRecord Unpack(byte[] line, int length)
        {
            DbRecord record = new DbRecord();
            byte[] unpackedData = UnpackRecord(line, length);
            int i = 0;
            while (i < _header.ColumnItems.Length)
            {
                int colLength = _header.ColumnItems[i].Length;
                if (colLength == 0)
                {
                    colLength = unpackedData.Length - _header.ColumnItems[i].StartPosition;
                }
                byte[] colBytes = new byte[colLength];
                int j = 0;
                while (j < colLength)
                {
                    colBytes[j] = unpackedData[_header.ColumnItems[i].StartPosition + j];
                    ++j;
                }
                record.Values.Add(_header.ColumnItems[i].FieldName, Encoding.ASCII.GetString(colBytes));
                ++i;
            }
            return record;
        }

        private byte[] UnpackRecord(byte[] line, int length)
        {
            byte[] result = new byte[_header.GetUnpackedRecordSize(length)];
            int i = 0;
            while (i < _header.ColumnItems.Length)
            {
                int j;
                if (_header.ColumnItems[i].DataType == DbTableItem.D_TYPE_REFERENCE)
                {
                    byte[] tempb = (byte[])_referenceTables[_header.ColumnReferenceIndex[i]][DataTransformations.ReadUnsignedShortReverse(line, _header.ColumnPackedPosition[i])];
                    j = 0;
                    while (j < _header.ColumnItems[i].Length)
                    {
                        result[_header.ColumnItems[i].StartPosition + j] = tempb[j];
                        ++j;
                    }
                }
                else
                {
                    int colLength = _header.ColumnItems[i].Length;
                    if (colLength == 0)
                    {
                        colLength = result.Length - _header.ColumnItems[i].StartPosition;
                    }
                    j = 0;
                    while (j < colLength)
                    {
                        result[_header.ColumnItems[i].StartPosition + j] = line[_header.ColumnPackedPosition[i] + j];
                        ++j;
                    }
                }
                ++i;
            }
            return result;
        }

    }
}
