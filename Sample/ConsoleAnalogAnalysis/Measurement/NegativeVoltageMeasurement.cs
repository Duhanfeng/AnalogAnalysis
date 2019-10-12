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
    /// 无效电压测量(从开启到关闭的电压)
    /// </summary>
    public class NegativeVoltageMeasurement
    {/// <summary>
     /// 创建频率测量新实例
     /// </summary>
     /// <param name="scope">示波器接口</param>
     /// <param name="plc">PLC接口</param>
        public NegativeVoltageMeasurement(IScope scope, IPLC plc)
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

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScope Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

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

            int[] frequencies1 = new int[] { 1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };

            int[] trueFrequencies = frequencies1.ToList().ConvertAll(x => x * 1000).ToArray();

            measureThread = new Thread(() =>
            {
                for (int i = 0; i < trueFrequencies.Length; i++)
                {
                    double[] originalData;
                    double[] filterData;
                    List<int> edgeIndexs;
                    DigitEdgeType digitEdgeType;

                    PLC.Frequency = trueFrequencies[i];
                    Thread.Sleep(500);

                    //读取scope数据
                    Scope.ReadData(0, out originalData);
                    Analysis.MeanFilter(originalData, 7, out filterData);

                    //阈值查找边沿
                    Analysis.FindEdgeByThreshold(filterData, MinVoltageThreshold, MaxVoltageThreshold, out edgeIndexs, out digitEdgeType);

                    //分析脉冲数据
                    List<double> pulseFrequencies;
                    List<double> dutyRatios;
                    Analysis.AnalysePulseData(edgeIndexs, digitEdgeType, (int)Scope.SampleRate, out pulseFrequencies, out dutyRatios);

                }

            });

            measureThread.Start();
        }
    }
}
