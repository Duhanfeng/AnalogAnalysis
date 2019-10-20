using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class PNVoltageMeasurementViewModel : Screen
    {
        #region 构造函数

        /// <summary>
        /// 创建PNVoltageMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="plc">PLC接口</param>
        public PNVoltageMeasurementViewModel(IScopeBase scope, IPLC plc)
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (plc == null)
            {
                throw new ArgumentException("plc invalid");
            }

            Scope = scope;
            PLC = plc;
        }

        #endregion

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        #endregion

        #region 配置参数

        private double minVoltageThreshold = 1.5;

        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold
        {
            get
            {
                return minVoltageThreshold;
            }
            set
            {
                minVoltageThreshold = value;
                NotifyOfPropertyChange(() => MinVoltageThreshold);
            }
        }

        private double maxVoltageThreshold = 8.0;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold
        {
            get
            {
                return minVoltageThreshold;
            }
            set
            {
                maxVoltageThreshold = value;
                NotifyOfPropertyChange(() => MaxVoltageThreshold);
            }
        }

        private double frequencyErrLimit = 0.2;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit
        {
            get
            {
                return frequencyErrLimit;
            }
            set
            {
                frequencyErrLimit = value;
                NotifyOfPropertyChange(() => FrequencyErrLimit);
            }
        }

        private double voltageInterval = 0.25;

        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval
        {
            get 
            { 
                return voltageInterval; 
            }
            set 
            { 
                voltageInterval = value;
                NotifyOfPropertyChange(() => VoltageInterval);
            }
        }

        private double minVoltage = 1;

        /// <summary>
        /// 最小电压(单位:V)
        /// </summary>
        public double MinVoltage
        {
            get
            {
                return minVoltage;
            }
            set
            {
                minVoltage = value;
                NotifyOfPropertyChange(() => MinVoltage);
            }
        }

        private double maxVoltage = 15.0;

        /// <summary>
        /// 最大电压(单位:V)
        /// </summary>
        public double MaxVoltage
        {
            get
            {
                return maxVoltage;
            }
            set
            {
                maxVoltage = value;
                NotifyOfPropertyChange(() => MaxVoltage);
            }
        }

        private int sampleTime = 500;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime
        {
            get
            {
                return sampleTime;
            }
            set
            {
                sampleTime = value;
                NotifyOfPropertyChange(() => SampleTime);
            }
        }

        private int comDelay = 200;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay
        {
            get
            {
                return comDelay;
            }
            set
            {
                comDelay = value;
                NotifyOfPropertyChange(() => ComDelay);
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<PNVoltageMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(PNVoltageMeasurementCompletedEventArgs e)
        {
            PLC.EnableOutput = false;
            MeasurementCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// 消息触发事件
        /// </summary>
        public event EventHandler<MessageRaisedEventArgs> MessageRaised;

        /// <summary>
        /// 触发消息触发事件
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected void OnMessageRaised(MessageLevel messageLevel, string message, Exception exception = null)
        {
            MessageRaised?.Invoke(this, new MessageRaisedEventArgs(messageLevel, message, exception));
        }

        #endregion

        #region 方法

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if ((Scope?.IsConnect != true) ||
                (PLC?.IsConnect != true))
            {
                throw new Exception("scope/plc invalid");
            }

            measureThread = new Thread(() =>
            {
                if (MinVoltage > MaxVoltage)
                {
                    throw new ArgumentException("MinVoltage > MaxVoltage");
                }

                bool isSuccess = false;
                double positiveVoltage = 0;
                double negativeVoltage = 0;

                //设置采样时间
                Scope.SampleTime = SampleTime;

                double currentVoltage = MinVoltage;
                PLC.Voltage = currentVoltage;
                PLC.EnableOutput = true;
                while (currentVoltage <= MaxVoltage)
                {
                    //设置当前电压
                    PLC.Voltage = currentVoltage;
                    Thread.Sleep(ComDelay);

                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadDataBlock(0, out originalData);

                    //数据滤波
                    double[] filterData;
                    Analysis.MeanFilter(originalData, 7, out filterData);

                    //阈值查找边沿
                    List<int> edgeIndexs;
                    DigitEdgeType digitEdgeType;
                    Analysis.FindEdgeByThreshold(filterData, MinVoltageThreshold, MaxVoltageThreshold, out edgeIndexs, out digitEdgeType);

                    //如果为上升沿或者是高电平,则有效
                    if ((digitEdgeType == DigitEdgeType.FirstRisingEdge) || (digitEdgeType == DigitEdgeType.HeightLevel))
                    {
                        positiveVoltage = currentVoltage;
                        isSuccess = true;
                        break;
                    }

                    currentVoltage += VoltageInterval;
                }

                if (isSuccess)
                {
                    isSuccess = false;
                    currentVoltage = MaxVoltage;
                    while (currentVoltage > MinVoltage)
                    {
                        //设置当前电压
                        PLC.Voltage = currentVoltage;
                        Thread.Sleep(ComDelay);

                        //读取Scope数据
                        double[] originalData;
                        Scope.ReadDataBlock(0, out originalData);

                        //数据滤波
                        double[] filterData;
                        Analysis.MeanFilter(originalData, 7, out filterData);

                        //阈值查找边沿
                        List<int> edgeIndexs;
                        DigitEdgeType digitEdgeType;
                        Analysis.FindEdgeByThreshold(filterData, MinVoltageThreshold, MaxVoltageThreshold, out edgeIndexs, out digitEdgeType);

                        //如果为上升沿或者是高电平,则有效
                        if ((digitEdgeType == DigitEdgeType.FirstFillingEdge) || (digitEdgeType == DigitEdgeType.LowLevel))
                        {
                            negativeVoltage = currentVoltage;
                            isSuccess = true;
                            break;
                        }

                        currentVoltage -= VoltageInterval;
                    }

                    if (isSuccess)
                    {
                        OnMeasurementCompleted(new PNVoltageMeasurementCompletedEventArgs(true, positiveVoltage, negativeVoltage));
                    }
                }

                OnMeasurementCompleted(new PNVoltageMeasurementCompletedEventArgs());

                return;
            });

            measureThread.Start();
        }

        #endregion


    }
}
