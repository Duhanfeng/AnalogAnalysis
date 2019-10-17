using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class PNVoltageMeasurementCompletedEventArgs : EventArgs
    {
        public PNVoltageMeasurementCompletedEventArgs()
        {
            IsSuccess = false;
        }

        /// <summary>
        /// 创建PNVoltageMeasurementCompletedEventArgs新实例
        /// </summary>
        /// <param name="isSuccess">成功标志</param>
        /// <param name="positiveVoltage">有效电压/吸合电压(V)</param>
        /// <param name="negativeVoltage">无效电压/释放电压(V)</param>
        public PNVoltageMeasurementCompletedEventArgs(bool isSuccess, double positiveVoltage, double negativeVoltage)
        {
            IsSuccess = isSuccess;
            PositiveVoltage = positiveVoltage;
            NegativeVoltage = negativeVoltage;

        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 有效电压/吸合电压(V)
        /// </summary>
        public double PositiveVoltage { get; }

        /// <summary>
        /// 无效电压/释放电压(V)
        /// </summary>
        public double NegativeVoltage { get; }

    }
}
