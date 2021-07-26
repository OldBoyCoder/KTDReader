using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;

namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private DbTableHeader _header;
        public void DumpAllData(string filename, int lineLengthInBytes, string outputFile)
        {
            try
            {
                var ms = new MemoryStream(File.ReadAllBytes(filename));
                using (var fileAccessor = new BinaryReader(ms))
                {
                    _header = new DbTableHeader(fileAccessor);
                    if (_header.GetDatabaseVersion() != "F3")
                    {
                        throw new Exception("Inadequate version of KTD database.");
                    }
                    if (_header.ColumnItems.Count > 30)
                    {
                        throw new Exception("Corrupted or improper database file.");
                    }
                    var blocks = GetAllPrimaryIndexBlocks(fileAccessor);
                    using (TextWriter tw = new StreamWriter(outputFile, false))
                    {
                        foreach (var columnItem in _header.ColumnItems)
                        {
                            tw.Write($"{columnItem.FieldName}\t");
                        }
                        tw.WriteLine();
                        for (var i = 0; i < blocks.Count; i++)
                        {
                            if (i % 500 == 0) Console.WriteLine($"{i}/{blocks.Count}");
                            var uc = UnpackBlock(fileAccessor, blocks[i]);
                            GetAllRecordsInBlock(uc, lineLengthInBytes, tw);
                        }
                    }
                    fileAccessor.Close();
                }
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Database file is not found.");
            }
            catch (Exception)
            {
                throw new Exception("Error while reading the database table.");
            }
        }

        private List<BlockSize> GetAllPrimaryIndexBlocks(BinaryReader fin)
        {
            var rc = new List<BlockSize>();
            fin.BaseStream.Seek(_header.PrimaryKeyTablePosition , SeekOrigin.Begin);
            long numberOfLocations = fin.ReadUInt32();
            for (var i = 0; i < numberOfLocations; i++)
            {
                fin.ReadBytes(_header.PrimaryKeyLength);
                long blockStart = fin.ReadUInt32();
                long blockEnd = fin.ReadUInt32();
                rc.Add(new BlockSize(blockStart, blockEnd));
            }
            return rc;
        }


        private static MemoryStream UnpackBlock(BinaryReader fin, BlockSize bSize)
        {
            fin.BaseStream.Seek(bSize.Start, SeekOrigin.Begin);
            var result = fin.ReadBytes((int)(bSize.End - bSize.Start));
            var inMs = new MemoryStream(result);
            var outMs = new MemoryStream();
            BZip2.Decompress(inMs, outMs, false);
            outMs.Position = 0;
            return outMs;
        }

        private void GetAllRecordsInBlock(Stream unpackedContent, int length, TextWriter tw)
        {
            do
            {
                var dataLength = unpackedContent.ReadByte();
                if (length == 2)
                    dataLength += unpackedContent.ReadByte() << 8;
                var content = new byte[dataLength - length];
                unpackedContent.Read(content, 0, dataLength - length);
                var record = Unpack(content, dataLength - length);
                foreach (var value in record.Values)
                {
                    tw.Write($"{value}\t");
                }
                tw.WriteLine();
            } while (unpackedContent.Position < unpackedContent.Length);

        }

        private Dictionary<string, string> Unpack(byte[] line, int length)
        {
            var record = new Dictionary<string, string>();
            var unpackedData = UnpackRecord(line, length);
            foreach (var columnItem in _header.ColumnItems)
            {
                var colLength = columnItem.Length;
                if (colLength == 0)
                    colLength = unpackedData.Length - columnItem.StartPosition;
                record.Add(columnItem.FieldName, Encoding.ASCII.GetString(unpackedData, columnItem.StartPosition, colLength));
            }
            return record;
        }
        private byte[] UnpackRecord(byte[] line, int length)
        {
            var result = new byte[_header.GetUnpackedRecordSize(length)];
            foreach (var columnItem in _header.ColumnItems)
            {
                switch (columnItem.DataType)
                {
                    case DbDataType.DTypeReference:
                    {
                        var refIndex = BitConverter.ToUInt16(line, columnItem.PackedPosition);
                        var refValue = _header.ReferenceTables[columnItem.ReferenceIndex][refIndex];
                        Array.Copy(refValue, 0, result, columnItem.StartPosition, columnItem.Length);
                        break;
                    }
                    case DbDataType.DTypeString:
                    {
                        var colLength = columnItem.Length;
                        if (colLength == 0)
                        {
                            colLength = result.Length - columnItem.StartPosition;
                        }

                        Array.Copy(line, columnItem.PackedPosition, result, columnItem.StartPosition, colLength);
                        break;
                    }
                    default:
                        throw new Exception($"Unknown data type {columnItem.DataType}");
                }
            }
            return result;
        }
    }
}
