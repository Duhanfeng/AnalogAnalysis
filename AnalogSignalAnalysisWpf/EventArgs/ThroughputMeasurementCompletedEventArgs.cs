using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class ThroughputMeasurementCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 创建ThroughputMeasurementCompletedEventArgs新实例
        /// </summary>
        public ThroughputMeasurementCompletedEventArgs()
        {
            IsSuccess = false;
            Flow = -1;
        }

        /// <summary>
        /// 创建ThroughputMeasurementCompletedEventArgs新实例
        /// </summary>
        /// <param name="isSuccess">成功标志</param>
        /// <param name="flow">流量</param>
        public ThroughputMeasurementCompletedEventArgs(bool isSuccess, double flow)
        {
            IsSuccess = isSuccess;
            Flow = flow;
        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 流量
        /// </summary>
        public double Flow { get; }

    }
}
