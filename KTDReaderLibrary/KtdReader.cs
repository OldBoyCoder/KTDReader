using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private readonly string _filename;
        private readonly DbTableHeader _header;
        private readonly List<List<object>> _referenceTables = new List<List<object>>();
        private readonly DataUnpacker _unpacker;
        private readonly int _lineLengthInBytes;

        public KtdReader(String filename, int lineLengthInBytes)
        {
            try
            {
                _filename = filename;
                _lineLengthInBytes = lineLengthInBytes;
                using (var fileAccessor = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    _header = new DbTableHeader(fileAccessor);
                    if (_header.GetDatabaseVersion() != "F3")
                    {
                        throw new Exception("Inadequate version of KTD database.");
                    }
                    if (_header.ColumnItems.Length > 30)
                    {
                        throw new Exception("Corrupted or improper database file.");
                    }
                    for (var i = 0; i < _header.NumberOfReferenceTables; i++)
                    {
                        _referenceTables.Add(LoadReferenceTable(fileAccessor, i));
                    }
                    _unpacker = new DataUnpacker(_header, _referenceTables);
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

        public void DumpAllPrimaryIndexes(string outputFile)
        {
            using (var fileAccessor = new BinaryReader(File.Open(_filename, FileMode.Open)))
            {
                var blocks = GetAllPrimaryIndexBlocks(fileAccessor, _header.PrimaryKeyTablePosition, _header.PrimaryKeyLength);
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
                        GetAllPkLines(uc, _lineLengthInBytes, tw);
                    }
                }
                fileAccessor.Close();
            }
        }

        private List<BlockSize> GetAllPrimaryIndexBlocks(BinaryReader fin, long position, int keyLength)
        {
            var rc = new List<BlockSize>();
            fin.BaseStream.Seek(position, SeekOrigin.Begin);
            long numberOfLocations = fin.ReadUInt32();
            for (var i = 0; i < numberOfLocations; i++)
            {
                fin.ReadBytes(keyLength);
                long blockStart = fin.ReadUInt32();
                long blockEnd = fin.ReadUInt32();
                rc.Add(new BlockSize(blockStart, blockEnd));
            }
            return rc;
        }


        private MemoryStream UnpackBlock(BinaryReader fin, BlockSize bSize)
        {
            fin.BaseStream.Seek(bSize.Start, SeekOrigin.Begin);
            var result = fin.ReadBytes((int)(bSize.End - bSize.Start));
            var inMs = new MemoryStream(result);
            var outMs = new MemoryStream();
            BZip2.Decompress(inMs, outMs, false);
            outMs.Position = 0;
            return outMs;
        }

        private void GetAllPkLines(MemoryStream unpackedContent, int length, TextWriter tw)
        {
            int dataLength = length;
            do
            {
                if (length == 1)
                {
                    dataLength = unpackedContent.ReadByte();
                }
                else if (length == 2)
                {
                    var byte1 = unpackedContent.ReadByte();
                    var byte2 = unpackedContent.ReadByte();
                    dataLength = byte1 + (byte2 << 8);
                }
                var content = new byte[dataLength - length];
                unpackedContent.Read(content, 0, dataLength - length);
                var record = _unpacker.Unpack(content, dataLength - length);
                foreach (var value in record.Values)
                {
                    tw.Write($"{value}\t");
                }
                tw.WriteLine();
            } while (unpackedContent.Position < unpackedContent.Length);

        }

        private List<Object> LoadReferenceTable(BinaryReader fin, int refTableIndex)
        {
            List<object> referenceTable = new List<object>();
            fin.BaseStream.Seek(_header.ReferenceTablePositions[refTableIndex], SeekOrigin.Begin);
            var numberOfItems = (int)fin.ReadUInt32();
            var itemFixedWidth = (int)fin.ReadUInt32();
            for (var i = 0; i < numberOfItems; i++)
            {
                var b = fin.ReadBytes(itemFixedWidth);
                referenceTable.Add(b.Clone());
            }
            return referenceTable;
        }
    }
}
