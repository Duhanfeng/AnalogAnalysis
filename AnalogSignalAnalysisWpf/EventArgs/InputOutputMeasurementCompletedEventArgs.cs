using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class InputOutputMeasurementCompletedEventArgs : EventArgs
    {
        public InputOutputMeasurementCompletedEventArgs()
        {
            IsSuccess = false;
        }

        public InputOutputMeasurementCompletedEventArgs(bool isSuccess, List<double> inputs, List<double> outputs)
        {
            IsSuccess = isSuccess;
            Inputs = inputs;
            Outputs = outputs;

        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 输入信号
        /// </summary>
        public List<double> Inputs { get; private set; } = new List<double>();

        /// <summary>
        /// 输出信号
        /// </summary>
        public List<double> Outputs { get; private set; } = new List<double>();

    }
}
