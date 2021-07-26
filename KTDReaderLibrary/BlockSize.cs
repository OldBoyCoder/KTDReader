namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class BlockSize
        {
            internal long start;
            internal long end;

            public BlockSize(long start, long end)
            {
                this.start = start;
                this.end = end;
            }

            public long getStart()
            {
                return start;
            }

            public long getEnd()
            {
                return start;
            }
        }
    }
}
