using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTDReaderLibrary
{
    public class VinReader
    {
        public static KtdReader newVINReader = null;

        public static void DumpAllRecords(string path, int LineLength, string outputFile)
        {
            newVINReader = new KtdReader(path, LineLength);
            newVINReader.DumpAllPrimaryIndexes(outputFile);

        }
    }
}
