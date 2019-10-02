using AnalogSignalAnalysisWpf.Hardware.PLC;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class PLCControlViewModel : Screen
    {
        public PLCControlViewModel(IPLC plc)
        {
            PLC = plc;
        }

        /// <summary>
        /// PLC实例
        /// </summary>
        IPLC PLC { get; set; }

        /// <summary>
        /// 连接标志位
        /// </summary>
        public bool IsConnect
        {
            get
            {
                return PLC?.IsConnect ?? false;
            }
        }

        /// <summary>
        /// 电压
        /// </summary>
        public double Voltage
        {
            get
            {
                return PLC?.Voltage ?? -1;
            }
            set
            {
                PLC.Voltage = value;
                NotifyOfPropertyChange(() => Voltage);
            }
        }

        /// <summary>
        /// 电流
        /// </summary>
        public double Current
        {
            get
            {
                return PLC?.Current ?? -1;
            }
            set
            {
                PLC.Current = value;
                NotifyOfPropertyChange(() => Current);
            }
        }

        /// <summary>
        /// 频率
        /// </summary>
        public int Frequency
        {
            get
            {
                return PLC?.Frequency ?? -1;
            }
            set
            {
                PLC.Frequency = value;
                NotifyOfPropertyChange(() => Frequency);
            }
        }

        #region 方法

        /// <summary>
        /// 连接设备
        /// </summary>
        public void Connect()
        {
            PLC?.Connect(0);
            NotifyOfPropertyChange(() => IsConnect);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            PLC?.Disconnect();
            NotifyOfPropertyChange(() => IsConnect);
        }

        #endregion
    }
}
