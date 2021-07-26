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

        public Dictionary<string, string> Unpack(byte[] line, int length)
        {
            var record = new Dictionary<string, string>();
            byte[] unpackedData = UnpackRecord(line, length);
            foreach (var columnItem in _header.ColumnItems)
            {
                var colLength = columnItem.Length;
                if (colLength == 0)
                    colLength = unpackedData.Length - columnItem.StartPosition;
//                var colBytes = new byte[colLength];
  //              Array.Copy(unpackedData, columnItem.StartPosition, colBytes, 0, colLength);
                record.Add(columnItem.FieldName, Encoding.ASCII.GetString(unpackedData, columnItem.StartPosition, colLength));
            }
            return record;
        }

        private byte[] UnpackRecord(byte[] line, int length)
        {
            byte[] result = new byte[_header.GetUnpackedRecordSize(length)];
            int i = 0;
            foreach (var columnItem in _header.ColumnItems)
            {
                if (columnItem.DataType == DbDataType.DTypeReference)
                {
                    var refIndex = BitConverter.ToUInt16(line, _header.ColumnPackedPosition[i]);
                    byte[] refValue = (byte[])_referenceTables[_header.ColumnReferenceIndex[i]][refIndex];
                    Array.Copy(refValue, 0, result, columnItem.StartPosition, columnItem.Length);
                }
                else if (columnItem.DataType == DbDataType.DTypeString)
                {
                    int colLength = columnItem.Length;
                    if (colLength == 0)
                    {
                        colLength = result.Length - columnItem.StartPosition;
                    }

                    Array.Copy(line, _header.ColumnPackedPosition[i], result, columnItem.StartPosition, colLength);
                }
                else
                    throw new Exception($"Unknown data type {columnItem.DataType}");
                ++i;
            }
            return result;
        }

    }
}
