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
        private List<List<Object>> referenceTables = new List<List<object>>();
        private DataUnpacker unpacker;
        private int lineLengthInBytes;

        public KtdReader(String filename) : this(filename, 1)
        {
        }

        public KtdReader(String filename, int lineLengthInBytes)
        {
            BinaryReader fileAccessor = null;
            try
            {
                try
                {
                    _filename = filename;
                    this.lineLengthInBytes = lineLengthInBytes;
                    fileAccessor = new BinaryReader(File.Open(filename, FileMode.Open));
                    _header = new DbTableHeader(fileAccessor);
                    if (_header.GetDatabaseVersion() != "F3")
                    {
                        throw new Exception("Inadequate version of KTD database.");
                    }
                    if (_header.ColumnItems.Length > 30)
                    {
                        throw new Exception("Corrupted or improper database file.");
                    }
                    int i = 0;
                    while (i < _header.NumberOfReferenceTables)
                    {
                        referenceTables.Add(LoadReferenceTable(fileAccessor, i));
                        ++i;
                    }
                    unpacker = new DataUnpacker(_header, referenceTables);
                }
                catch (FileNotFoundException)
                {
                    throw new Exception("Database file is not found.");
                }
                catch (IOException)
                {
                    throw new Exception("Error while reading the database table.");
                }
            }
            finally
            {
                if (fileAccessor != null)
                {
                    fileAccessor.Close();
                }
            }
        }

        public void DumpAllPrimaryIndexes(string outputFile)
        {
            BinaryReader fileAccessor;
            fileAccessor = new BinaryReader(File.Open(_filename, FileMode.Open));
            var bFirst = true;
            var blocks = this.GetAllPrimaryIndexBlocks(fileAccessor, _header.PrimaryKeyTablePosition, _header.PrimaryKeyLength);
            using (TextWriter tw = new StreamWriter(outputFile, false))
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    Console.WriteLine($"{i}/{blocks.Count}");
                    DataContent uc = unpackBlock(fileAccessor, blocks[i]);
                    var records = GetAllPkLines(uc, true, lineLengthInBytes);
                    if (bFirst)
                    {
                        foreach (var value in records[0].Values)
                        {
                            tw.Write($"{value.Key}\t");
                        }
                        tw.WriteLine();
                        bFirst = false;
                    }
                    foreach (var record in records)
                    {
                        foreach (var value in record.Values)
                        {
                            tw.Write($"{value.Value}\t");
                        }
                        tw.WriteLine();
                    }
                }
            }
            fileAccessor.Close();
        }

        private List<BlockSize> GetAllPrimaryIndexBlocks(BinaryReader fin, long position, int keyLength)
        {
            var rc = new List<BlockSize>();
            fin.BaseStream.Seek(position, SeekOrigin.Begin);
            long numberOfLocations = fin.ReadUInt32();
            int i = 0;
            while (i < numberOfLocations)
            {
                fin.ReadBytes(keyLength);
                long blockStart = fin.ReadUInt32();
                long blockEnd = fin.ReadUInt32();
                rc.Add(new BlockSize(blockStart, blockEnd));
                ++i;
            }
            return rc;
        }


        private DataContent unpackBlock(BinaryReader fin, BlockSize bSize)
        {
            fin.BaseStream.Seek(bSize.Start, SeekOrigin.Begin);
            var result = fin.ReadBytes((int)(bSize.End - bSize.Start));
            var bais = new MemoryStream(result);
            var ms = new MemoryStream();
            BZip2.Decompress(bais, ms, false);
            var ba = ms.ToArray();
            return new DataContent(ba, ba.Length);
        }

        private List<DbRecord> GetAllPkLines(DataContent unpackedContent, bool lineHasLength, int length)
        {
            var rc = new List<DbRecord>();
            var bais = new MemoryStream(unpackedContent.Content, 0, (int)unpackedContent.Length);
            int dataLength = length;
            do
            {
                if (lineHasLength)
                {
                    if (length == 1)
                    {
                        dataLength = bais.ReadByte();
                    }
                    else if (length == 2)
                    {
                        int byte1 = bais.ReadByte();
                        int byte2 = bais.ReadByte();
                        dataLength = byte1 + (byte2 << 8);
                    }
                }

                var content = new byte[dataLength - length];
                bais.Read(content, 0, dataLength - length);

                rc.Add(unpacker.Unpack(content, dataLength - length));
            } while (bais.Position < bais.Length);

            return rc;
        }

        private List<Object> LoadReferenceTable(BinaryReader fin, int refTableIndex)
        {
            List<Object> referenceTable = new List<Object>();
            fin.BaseStream.Seek(_header.ReferenceTablePositions[refTableIndex], SeekOrigin.Begin) ;
            int numberOfItems = (int)fin.ReadUInt32();
            int itemFixedWidth = (int)fin.ReadUInt32();
            int i = 0;
            while (i < numberOfItems)
            {
                var b = fin.ReadBytes(itemFixedWidth);
                referenceTable.Add(b.Clone());
                ++i;
            }
            return referenceTable;
        }
    }
}
