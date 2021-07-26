namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class BlockSize
        {
            internal long Start;
            internal long End;

            public BlockSize(long start, long end)
            {
                Start = start;
                End = end;
            }

        }
    }
}
