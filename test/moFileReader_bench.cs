using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace moFileLib
{
    [HtmlExporter,  RPlotExporter]
    public class moFileLibBench
    {
        private string MO_TEST_FILE = "languages/nl.mo";

        private moFileReader moFR;
        
        public moFileLibBench()
        {
            moFR = new moFileReader();
            var res = moFR.ReadFile(MO_TEST_FILE);
            if (res != moFileReader.ErrorCode.SUCCESS)
            {
                throw new Exception(moFR.GetErrorDescription());
            }
        }
        
        [Benchmark]
        public void Lookup()
        {
            moFR.Lookup("String English One");
        }
        
        [Benchmark]
        public void LookupWithContext()
        {
            moFR.LookupWithContext("TEST|String|1", "String English");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<moFileLibBench>();
        }
    }
}