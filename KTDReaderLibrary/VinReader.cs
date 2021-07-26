namespace KTDReaderLibrary
{
    public static class VinReader
    {

        public static void DumpAllRecords(string path, int lineLength, string outputFile)
        {
            var newVinReader = new KtdReader(path, lineLength);
            newVinReader.DumpAllPrimaryIndexes(outputFile);
        }
    }
}
