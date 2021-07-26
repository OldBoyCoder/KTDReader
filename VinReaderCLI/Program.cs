using KTDReaderLibrary;

namespace VinReaderCLI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Program
    {
        static void Main()
        {
            VinReader.DumpAllRecords(@"C:\Temp\SP.RT.04210.FCTLR", 2, @"C:\Temp\SP.RT.04210.TXT");
            VinReader.DumpAllRecords(@"C:\Temp\SP.CH.04210.FCTLR", 1, @"C:\Temp\SP.CH.04210.TXT");
        }

    }
}
