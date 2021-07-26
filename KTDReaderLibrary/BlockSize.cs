namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class BlockSize
        {
            internal readonly long Start;
            internal readonly long End;

            public BlockSize(long start, long end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
