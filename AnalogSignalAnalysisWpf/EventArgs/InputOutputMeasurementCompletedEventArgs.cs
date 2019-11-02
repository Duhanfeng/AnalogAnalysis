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

        public InputOutputMeasurementCompletedEventArgs(bool isSuccess, List<InputOutputMeasurementInfo> infos)
        {
            IsSuccess = isSuccess;
            Infos = infos;

        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 输入输出信息
        /// </summary>
        public List<InputOutputMeasurementInfo> Infos { get; private set; } = new List<InputOutputMeasurementInfo>();

    }
}
