using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.Measurement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAnalogAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            SigrokCli_Hantek6xxx scope = new SigrokCli_Hantek6xxx();
            VirtualPLC plc = new VirtualPLC();
            scope.Connect(0);
            plc.Connect(0);
            FrequencyMeasurement frequencyMeasurement = new FrequencyMeasurement(scope, plc);
            frequencyMeasurement.FrequencyMeasurementCompleted += FrequencyMeasurement_FrequencyMeasurementCompleted;
            frequencyMeasurement.MinVoltageThreshold = 1.0;
            frequencyMeasurement.MaxVoltageThreshold = 3.0;
            frequencyMeasurement.Start();

            Console.Read();

        }

        private static void FrequencyMeasurement_FrequencyMeasurementCompleted(object sender, AnalogSignalAnalysisWpf.FrequencyMeasurementCompletedEventArgs e)
        {
            Console.WriteLine($"Result={e.IsSuccess}: MaxFrequency={e.MaxLimitFrequency}Hz;");
        }
    }
}
