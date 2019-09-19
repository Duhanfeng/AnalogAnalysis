using CsvHelper;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Runtime.InteropServices;

namespace DataAnalysisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] deviceData = new double[0];

            //从CSV中获取数据
            using (var reader = new StreamReader(@"data.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<double>();
                deviceData = records.ToArray();
            }

            Console.ReadKey();

        }
    }
}
