using AnalogSignalAnalysisWpf.Hardware.PWM;
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
    public class FrequencyMeasurementViewModel : Screen
    {
        public FrequencyMeasurementViewModel()
        {

        }

        public FrequencyMeasurementViewModel(IScopeBase scope, IPWM pwm)
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (pwm == null)
            {
                throw new ArgumentException("plc invalid");
            }

            Scope = scope;
            PWM = pwm;
        }

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPWM PWM { get; set; }

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
        public event EventHandler<FrequencyMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(FrequencyMeasurementCompletedEventArgs e)
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

        #region 折线图



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
            if (Scope?.IsConnect != true)
            {
                throw new Exception("scope invalid");
            }

            //频率列表(单位:Hz)
            int[] frequencies1 = new int[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };
            int[] sampleTime = new int[] { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 200, 200, 200, 200, 200, 200, 200 };

            measureThread = new Thread(() =>
            {
                if (frequencies1.Length != sampleTime.Length)
                {
                    return;
                }

                int lastFrequency = -1;

                for (int i = 0; i < frequencies1.Length; i++)
                {
                    //设置PLC频率
                    PWM.Frequency = frequencies1[i];
                    Thread.Sleep(50);

                    //设置Scope采集时长
                    Scope.SampleTime = sampleTime[i];

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

                    //分析脉冲数据
                    List<double> pulseFrequencies;
                    List<double> dutyRatios;
                    Analysis.AnalysePulseData(edgeIndexs, digitEdgeType, (int)Scope.SampleRate, out pulseFrequencies, out dutyRatios);

                    if (pulseFrequencies.Count > 0)
                    {
                        //检测脉冲是否异常
                        double minFrequency = frequencies1[i] * (1 - FrequencyErrLimit);
                        double maxFrequency = frequencies1[i] * (1 + FrequencyErrLimit);
                        if (!Analysis.CheckFrequency(pulseFrequencies, minFrequency, maxFrequency, 1))
                        {
                            if (lastFrequency != -1)
                            {
                                //测试完成
                                OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                            }
                            else
                            {
                                //测试失败
                                OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                            }
                            return;
                        }
                        else
                        {
                            lastFrequency = frequencies1[i];
                        }
                    }
                    else
                    {
                        if (lastFrequency != -1)
                        {
                            //测试完成
                            OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                        }
                        else
                        {
                            //测试失败
                            OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                        }
                        return;
                    }

                }

                if (lastFrequency != -1)
                {
                    //测试完成
                    OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                }
                else
                {
                    //测试失败
                    OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                }
                return;
            });

            measureThread.Start();
        }


        #endregion
    }
}
