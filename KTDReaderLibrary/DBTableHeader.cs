using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KTDReaderLibrary
{
    class DbTableHeader
    {
        private readonly string _dbVersion;
        internal readonly long[] ReferenceTablePositions;
        internal readonly long PrimaryKeyTablePosition;
        internal readonly DbTableItem[] ColumnItems;
        internal readonly int PrimaryKeyLength;
        internal int[] ColumnReferenceIndex;
        internal int[] ColumnPackedPosition;
        private int _referenceAddition;
        internal readonly int NumberOfReferenceTables;


        public string GetDatabaseVersion()
        {
            return _dbVersion;
        }

        public DbTableHeader(BinaryReader fin)
        {
            int i;
            fin.BaseStream.Seek(0, SeekOrigin.Begin);
            var fType = fin.ReadBytes(2);
            _dbVersion = Encoding.Default.GetString(fType);
            fin.ReadUInt32();
            fin.ReadUInt16();
            ReferenceTablePositions = new long[5];
            for (i = 0; i < 5; i++)
            {
                ReferenceTablePositions[i] = fin.ReadUInt32();
            }
            PrimaryKeyTablePosition = fin.ReadUInt32();
            var secondaryKeyTablePositions = new long[3];
            for (i = 0; i < 3; i++)
            {
                secondaryKeyTablePositions[i] = fin.ReadUInt32();
            }

            fin.ReadBytes(20);

            var primaryKeyItems = new DbTableItem[fin.ReadByte()];
            for (i = 0; i < primaryKeyItems.Length; i++)
            {
                primaryKeyItems[i] = new DbTableItem(fin);
            }
            var secondaryKeyItems = new DbTableItem[fin.ReadByte()][];
            for (var j = 0; j < secondaryKeyItems.Length; j++)
            {
                secondaryKeyItems[j] = new DbTableItem[fin.ReadByte()];
                for (i = 0; i < secondaryKeyItems[j].Length; i++)
                {
                    secondaryKeyItems[j][i] = new DbTableItem(fin);
                }
            }
            ColumnItems = new DbTableItem[fin.ReadByte()];
            for (i = 0;i < ColumnItems.Length;i++)
            {
                ColumnItems[i] = new DbTableItem(fin);
            }

            PrimaryKeyLength = primaryKeyItems.Sum(x => x.Length);

            var secondaryKeyLengths = new int[secondaryKeyItems.Length];
            for  (var j = 0;j < secondaryKeyLengths.Length;j++)
            {
                secondaryKeyLengths[j] = secondaryKeyItems[j].Sum(x=>x.Length);
            }
            NumberOfReferenceTables = 0;
            while (NumberOfReferenceTables < ReferenceTablePositions.Length)
            {
                if (ReferenceTablePositions[NumberOfReferenceTables] == 0L) break;
                ++NumberOfReferenceTables;
            }
            ConfigureAdditionalProperties();
        }



        public int GetUnpackedRecordSize(int packedSize)
        {
            return packedSize + _referenceAddition;
        }

        private void ConfigureAdditionalProperties()
        {
            ColumnPackedPosition = new int[ColumnItems.Length];
            ColumnReferenceIndex = new int[ColumnItems.Length];
            int curPos = 0;
            int curRef = 0;
            for (var i = 0;i < ColumnItems.Length;i++)
            {
                ColumnPackedPosition[i] = curPos;
                if (ColumnItems[i].DataType == DbDataType.DTypeReference)
                {
                    ColumnReferenceIndex[i] = curRef++;
                    curPos += 2;
                }
                else if (ColumnItems[i].DataType == DbDataType.DTypeString)
                {
                    ColumnReferenceIndex[i] = -1;
                    curPos += ColumnItems[i].Length;
                }
                else
                    throw new Exception($"Unknown data type {ColumnItems[i].DataType}");

            }
            _referenceAddition = ColumnItems.Where(x => x.DataType == DbDataType.DTypeReference)
                .Sum(x => x.Length - 2);
        }

    }
}
