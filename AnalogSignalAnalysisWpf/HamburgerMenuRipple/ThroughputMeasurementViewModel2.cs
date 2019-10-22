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
    public class ThroughputMeasurementViewModel2 : Screen
    {

        #region 构造函数

        /// <summary>
        /// 创建ThroughputMeasured新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="plc">PLC接口</param>
        public ThroughputMeasurementViewModel2(IScopeBase scope, IPLC plc)
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
                return maxVoltageThreshold;
            }
            set
            {
                maxVoltageThreshold = value;
                NotifyOfPropertyChange(() => MaxVoltageThreshold);
            }
        }

        private double outputVoltage = 12.0;

        /// <summary>
        /// 输出电压(V)
        /// </summary>
        public double OutputVoltage
        {
            get
            {
                return outputVoltage;
            }
            set
            {
                outputVoltage = value;
                NotifyOfPropertyChange(() => OutputVoltage);
            }
        }

        private int outputDelay = 300;

        /// <summary>
        /// 输出延时(MS)
        /// </summary>
        public int OutputDelay
        {
            get
            {
                return outputDelay;
            }
            set
            {
                outputDelay = value;
                NotifyOfPropertyChange(() => OutputDelay);
            }
        }

        private int sampleTime = 1000;

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

        #endregion

        #region 事件

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<ThroughputMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(ThroughputMeasurementCompletedEventArgs e)
        {
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
                //设置电压
                PLC.EnableOutput = false;
                PLC.Voltage = OutputVoltage;

                //设置示波器采集时间
                Scope.SampleTime = SampleTime;

                //启动线程
                new Thread(() =>
                {
                    Thread.Sleep(OutputDelay);

                    //设置电压
                    PLC.EnableOutput = true;
                }).Start();

                //读取Scope数据
                double[] originalData;
                Scope.ReadDataBlock(0, out originalData);
                Thread.Sleep(OutputDelay);
                PLC.EnableOutput = false;

                //数据滤波
                double[] filterData;
                Analysis.MeanFilter(originalData, 7, out filterData);

                //阈值查找边沿
                List<int> edgeIndexs;
                DigitEdgeType digitEdgeType;
                Analysis.FindEdgeByThreshold(filterData, MinVoltageThreshold, MaxVoltageThreshold, out edgeIndexs, out digitEdgeType);

                if (Analysis.IsOnlyRisingEdge(edgeIndexs, digitEdgeType))
                {
                    //查找时间
                }
                else
                {
                    OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs());
                }

                return;
            });

            measureThread.Start();
        }

        #endregion
    }
}
