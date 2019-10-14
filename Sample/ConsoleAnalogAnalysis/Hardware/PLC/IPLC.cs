using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    public interface IPLC : IPLCCommunication
    {
        #region 控制接口

        /// <summary>
        /// 电压值
        /// </summary>
        double Voltage { get; set; }

        /// <summary>
        /// 电流值
        /// </summary>
        double Current { get; set; }

        /// <summary>
        /// 使能
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// 开关频率
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// 实际电压值
        /// </summary>
        double RealityVoltage { get; }

        /// <summary>
        /// 实际电压值
        /// </summary>
        double RealityCurrent { get; }

        /// <summary>
        /// 实际电压值
        /// </summary>
        double RealityTemperature { get; }

        #endregion
    }
}
