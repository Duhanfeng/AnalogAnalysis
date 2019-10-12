using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    public interface IPLC : IPLCCommunication
    {
        /// <summary>
        /// 电压值(V)
        /// </summary>
        double Voltage { get; set; }

        /// <summary>
        /// 电流值(A)
        /// </summary>
        double Current { get; set; }

        /// <summary>
        /// 开关频率(Hz)
        /// </summary>
        int Frequency { get; set; }
    }
}
