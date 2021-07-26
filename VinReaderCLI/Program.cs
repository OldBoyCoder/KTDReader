using KTDReaderLibrary;

namespace VinReaderCLI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main()
        {
            var reader = new KtdReader();
            reader.DumpAllData( @"C:\Temp\SP.RT.04210.FCTLR", 2, @"C:\Temp\SP.RT.04210.TXT");
            reader.DumpAllData(@"C:\Temp\SP.CH.04210.FCTLR", 1, @"C:\Temp\SP.CH.04210.TXT");
        }

    }
}
