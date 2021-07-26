using System;
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


        public String GetDatabaseVersion()
        {
            return _dbVersion;
        }

        public DbTableHeader(ExtendedRandomAccessFile fin)
        {
            int i;
            fin.seek(0L);
            var fType = fin.read(2);
            _dbVersion = Encoding.Default.GetString(fType);
            fin.readUnsignedIntReverse();
            fin.readUnsignedShortReverse();
            ReferenceTablePositions = new long[5];
            int i2 = 0;
            while (i2 < 5)
            {
                ReferenceTablePositions[i2] = fin.readUnsignedIntReverse();
                ++i2;
            }
            PrimaryKeyTablePosition = fin.readUnsignedIntReverse();
            var secondaryKeyTablePositions = new long[3];
            i2 = 0;
            while (i2 < 3)
            {
                secondaryKeyTablePositions[i2] = fin.readUnsignedIntReverse();
                ++i2;
            }
            DbTableItem.ReadFixedLengthString(fin, 20);
            var primaryKeyItems = new DbTableItem[fin.readUnsignedByte()];
            i2 = 0;
            while (i2 < primaryKeyItems.Length)
            {
                primaryKeyItems[i2] = new DbTableItem(fin);
                ++i2;
            }
            var secondaryKeyItems = new DbTableItem[fin.readUnsignedByte()][];
            int j = 0;
            while (j < secondaryKeyItems.Length)
            {
                secondaryKeyItems[j] = new DbTableItem[fin.readUnsignedByte()];
                i = 0;
                while (i < secondaryKeyItems[j].Length)
                {
                    secondaryKeyItems[j][i] = new DbTableItem(fin);
                    ++i;
                }
                ++j;
            }
            ColumnItems = new DbTableItem[fin.readUnsignedByte()];
            i2 = 0;
            while (i2 < ColumnItems.Length)
            {
                ColumnItems[i2] = new DbTableItem(fin);
                ++i2;
            }
            PrimaryKeyLength = 0;
            i2 = 0;
            while (i2 < primaryKeyItems.Length)
            {
                PrimaryKeyLength += primaryKeyItems[i2].Length;
                ++i2;
            }
            var secondaryKeyLengths = new int[secondaryKeyItems.Length];
            j = 0;
            while (j < secondaryKeyLengths.Length)
            {
                secondaryKeyLengths[j] = 0;
                i = 0;
                while (i < secondaryKeyItems[j].Length)
                {
                    int n = j;
                    secondaryKeyLengths[n] = secondaryKeyLengths[n] + secondaryKeyItems[j][i].Length;
                    ++i;
                }
                ++j;
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
            int i = 0;
            while (i < ColumnItems.Length)
            {
                ColumnPackedPosition[i] = curPos;
                if (ColumnItems[i].DataType == DbTableItem.D_TYPE_REFERENCE)
                {
                    ColumnReferenceIndex[i] = curRef++;
                    curPos += 2;
                }
                else
                {
                    ColumnReferenceIndex[i] = -1;
                    curPos += ColumnItems[i].Length;
                }
                ++i;
            }
            _referenceAddition = 0;
            i = 0;
            while (i < ColumnItems.Length)
            {
                if (ColumnItems[i].DataType == DbTableItem.D_TYPE_REFERENCE)
                {
                    _referenceAddition += ColumnItems[i].Length - 2;
                }
                ++i;
            }
        }

    }
}
