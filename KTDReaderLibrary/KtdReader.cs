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
            ExtendedRandomAccessFile fileAccessor = null;
            try
            {
                try
                {
                    _filename = filename;
                    this.lineLengthInBytes = lineLengthInBytes;
                    fileAccessor = new ExtendedRandomAccessFile(filename);
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
                    fileAccessor.close();
                }
            }
        }

        public void DumpAllPrimaryIndexes(string outputFile)
        {
            ExtendedRandomAccessFile fileAccessor;
            fileAccessor = new ExtendedRandomAccessFile(_filename);
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
            fileAccessor.close();
        }

        private List<BlockSize> GetAllPrimaryIndexBlocks(ExtendedRandomAccessFile fin, long position, int keyLength)
        {
            var rc = new List<BlockSize>();
            fin.seek(position);
            long numberOfLocations = fin.readUnsignedIntReverse();
            int i = 0;
            while (i < numberOfLocations)
            {
                fin.read(keyLength);
                long blockStart = fin.readUnsignedIntReverse();
                long blockEnd = fin.readUnsignedIntReverse();
                rc.Add(new BlockSize(blockStart, blockEnd));
                ++i;
            }
            return rc;
        }


        private DataContent unpackBlock(ExtendedRandomAccessFile fin, BlockSize bSize)
        {
            fin.seek(bSize.start);
            var result = fin.read((int)(bSize.end - bSize.start));
            ByteArrayInputStream bais = new ByteArrayInputStream(result);
            var ms = new MemoryStream();
            BZip2.Decompress(bais, ms, false);
            var ba = ms.ToArray();
            return new DataContent(ba, ba.Length);
        }

        private List<DbRecord> GetAllPkLines(DataContent unpackedContent, bool lineHasLength, int length)
        {
            var rc = new List<DbRecord>();
            ByteArrayInputStream bais = new ByteArrayInputStream(unpackedContent.content, 0, (int)unpackedContent.getLength());
            int dataLength = length;
            do
            {
                if (lineHasLength)
                {
                    if (length == 1)
                    {
                        dataLength = bais.read();
                    }
                    else if (length == 2)
                    {
                        int byte1 = bais.read();
                        int byte2 = bais.read();
                        dataLength = byte1 + (byte2 << 8);
                    }
                }

                var content = bais.read(0, dataLength - length);
                rc.Add(unpacker.unpack(content, dataLength - length));
            } while (bais.EndOfStream() != true);

            return rc;
        }

        private List<Object> LoadReferenceTable(ExtendedRandomAccessFile fin, int refTableIndex)
        {
            List<Object> referenceTable = new List<Object>();
            fin.seek(_header.ReferenceTablePositions[refTableIndex]);
            int numberOfItems = (int)fin.readUnsignedIntReverse();
            int itemFixedWidth = (int)fin.readUnsignedIntReverse();
            int i = 0;
            while (i < numberOfItems)
            {
                var b = fin.read(itemFixedWidth);
                referenceTable.Add(b.Clone());
                ++i;
            }
            return referenceTable;
        }
    }
}
