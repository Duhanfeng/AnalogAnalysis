using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Measurement
{
    /// <summary>
    /// 吸合电压与释放电压测试
    /// </summary>
    public class PositiveAndNegativeVoltageMeasurement
    {
        #region 构造函数

        /// <summary>
        /// 创建频率测量新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="plc">PLC接口</param>
        public PositiveAndNegativeVoltageMeasurement(IScope scope, IPLC plc)
        {
            if (scope?.IsConnect != true)
            {
                throw new ArgumentException("scope invalid");
            }

            if (plc?.IsConnect != true)
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
        public IScope Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        #endregion

        #region 配置参数

        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit { get; set; } = 0.2;

        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval { get; set; } = 0.25;

        /// <summary>
        /// 最小电压(V)
        /// </summary>
        public double MinVoltage { get; set; } = 1.0;

        /// <summary>
        /// 最大电压(V)
        /// </summary>
        public double MaxVoltage { get; set; } = 15.0;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 500;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay { get; set; } = 200;

        #endregion

        #region 事件

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<PositiveAndNegativeVoltageMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(PositiveAndNegativeVoltageMeasurementCompletedEventArgs e)
        {
            PLC.Enable = false;
            MeasurementCompleted?.Invoke(this, e);
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
                PLC.Enable = true;
                while (currentVoltage <= MaxVoltage)
                {
                    //设置当前电压
                    PLC.Voltage = currentVoltage;
                    Thread.Sleep(ComDelay);

                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadData(0, out originalData);

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
                        Scope.ReadData(0, out originalData);

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
                        OnMeasurementCompleted(new PositiveAndNegativeVoltageMeasurementCompletedEventArgs(true, positiveVoltage, negativeVoltage));
                    }
                }

                OnMeasurementCompleted(new PositiveAndNegativeVoltageMeasurementCompletedEventArgs());

                return;
            });

            measureThread.Start();
        }

        #endregion
    }
}
