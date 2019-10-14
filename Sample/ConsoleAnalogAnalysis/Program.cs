using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.PWM;
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
            ModbusPLC plc = new ModbusPLC("COM1");
            IPWM pwm = new SerialPortPWM("COM1");
            scope.Connect(0);
            plc.Connect(0);

            var measurement = new PositiveAndNegativeVoltageMeasurement(scope, plc);
            measurement.MeasurementCompleted += Measurement_MeasurementCompleted;
            measurement.MinVoltageThreshold = 1.0;
            measurement.MaxVoltageThreshold = 3.0;
            //measurement.Start();

            FrequencyMeasurement frequencyMeasurement = new FrequencyMeasurement(scope, pwm);
            frequencyMeasurement.MeasurementCompleted += FrequencyMeasurement_FrequencyMeasurementCompleted;
            frequencyMeasurement.MinVoltageThreshold = 1.0;
            frequencyMeasurement.MaxVoltageThreshold = 3.0;
            frequencyMeasurement.Start();

            Console.Read();

        }

        private static void Measurement_MeasurementCompleted(object sender, AnalogSignalAnalysisWpf.PositiveAndNegativeVoltageMeasurementCompletedEventArgs e)
        {
            Console.WriteLine($"Result={e.IsSuccess}: P={e.PositiveVoltage}V N={e.NegativeVoltage}V;");
        }

        private static void FrequencyMeasurement_FrequencyMeasurementCompleted(object sender, AnalogSignalAnalysisWpf.FrequencyMeasurementCompletedEventArgs e)
        {
            Console.WriteLine($"Result={e.IsSuccess}: MaxFrequency={e.MaxLimitFrequency}Hz;");
        }
    }
}
