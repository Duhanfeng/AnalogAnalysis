using AnalogSignalAnalysisWpf.Hardware;
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
    public class FrequencyMeasurement
    {
        public FrequencyMeasurement(IScope scope, IPLC plc)
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
        /// 示波器
        /// </summary>
        public IScope Scope { get; set; }

        /// <summary>
        /// PLC
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// 最小阈值
        /// </summary>
        public double MinThreshold { get; set; }

        /// <summary>
        /// 最大阈值
        /// </summary>
        public double MaxThreshold { get; set; }

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit { get; set; } = 0.2;

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

            int[] frequencies1 = new int[] { 1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100};

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
                    Analysis.FindEdgeByThreshold(filterData, MinThreshold, MaxThreshold, out edgeIndexs, out digitEdgeType);

                    //分析脉冲数据
                    List<double> pulseFrequencies;
                    List<double> dutyRatios;
                    Analysis.AnalysePulseData(edgeIndexs, digitEdgeType, (int)Scope.SampleRate, out pulseFrequencies, out dutyRatios);

                    //检测脉冲是否异常
                    double minFrequency = trueFrequencies[i] * (1 - FrequencyErrLimit);
                    double maxFrequency = trueFrequencies[i] * (1 + FrequencyErrLimit);

                    if (Analysis.CheckFrequency(pulseFrequencies, minFrequency, maxFrequency) == false)
                    {
                        //失败

                    }

                }


            });

            measureThread.Start();

        }

    }
}
