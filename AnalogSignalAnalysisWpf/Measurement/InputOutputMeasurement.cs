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
    /// 输入输出测试
    /// </summary>
    public class InputOutputMeasurement
    {

        #region 构造函数

        /// <summary>
        /// 创建ThroughputMeasured新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="plc">PLC接口</param>
        public InputOutputMeasurement(IScopeBase scope, IPLC plc)
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
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        #endregion

        #region 配置参数

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
        public int SampleTime { get; set; } = 200;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay { get; set; } = 200;

        #endregion

        #region 事件

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<InputOutputMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(InputOutputMeasurementCompletedEventArgs e)
        {
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
                List<double> inputs = new List<double>();
                List<double> outputs = new List<double>();

                if (MinVoltage > MaxVoltage)
                {
                    throw new ArgumentException("MinVoltage > MaxVoltage");
                }

                //设置采样时间
                Scope.SampleTime = SampleTime;

                double currentVoltage = MinVoltage;
                PLC.Voltage = currentVoltage;
                PLC.Enable = true;
                while (currentVoltage <= MaxVoltage)
                {
                    //设置当前电压
                    PLC.Voltage = currentVoltage;
                    inputs.Add(currentVoltage);
                    Thread.Sleep(ComDelay);

                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadDataBlock(0, out originalData);

                    //数据滤波
                    double[] filterData;
                    Analysis.MeanFilter(originalData, 7, out filterData);

                    //获取中值
                    var medianData = Analysis.Median(filterData);
                    if (double.IsNaN(medianData))
                    {
                        OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs());
                    }

                    outputs.Add(medianData);

                    currentVoltage += VoltageInterval;
                }

                //输出结果
                OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs(true, inputs, outputs));

            });

            measureThread.Start();
        }

        #endregion

    }
}
