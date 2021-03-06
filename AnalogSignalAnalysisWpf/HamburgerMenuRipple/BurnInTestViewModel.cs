﻿using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AnalogSignalAnalysisWpf
{
    public class BurnInTestInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 输入频率
        /// </summary>
        public int InputFrequency { get; set; }

        /// <summary>
        /// 采样时间
        /// </summary>
        public int SampleTime { get; set; }

        /// <summary>
        /// 当前频率
        /// </summary>
        public string CurrentFrequency { get; set; }

        /// <summary>
        /// 创建FrequencyMeasurementInfo新实例
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sampleTime"></param>
        /// <param name="currentFrequency"></param>
        public BurnInTestInfo(int input, int sampleTime, List<double> pulseFrequencies)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            InputFrequency = input;
            SampleTime = sampleTime;

            string pulseMessage = "";

            if (pulseFrequencies != null)
            {
                foreach (var item in pulseFrequencies)
                {
                    pulseMessage += item.ToString("F2") + ", ";
                }
            }

            CurrentFrequency = pulseMessage;
        }

    }

    public class BurnInTestViewModel : Screen
    {
        #region 构造函数

        private bool isAdmin = false;

        /// <summary>
        /// 管理员权限
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                isAdmin = value;
                NotifyOfPropertyChange(() => IsAdmin);
            }
        }

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        public BurnInTestViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            Frequency = SystemParamManager.SystemParam.BurnInTestParams.Frequency;
            PLCCount = SystemParamManager.SystemParam.BurnInTestParams.PLCCount;

            MeasurementInfos = new ObservableCollection<BurnInTestInfo>();

            UpdateHardware();

            IsAdmin = false;
        }

        public BurnInTestViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (power == null)
            {
                throw new ArgumentException("power invalid");
            }

            if (plc == null)
            {
                throw new ArgumentException("plc invalid");
            }

            if (pwm == null)
            {
                throw new ArgumentException("pwm invalid");
            }

            Scope = scope;
            Power = power;
            PLC = plc;
            PWM = pwm;

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
            }
            else
            {
                RunningStatus = "就绪";
            }
        }

        #endregion

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// Power接口
        /// </summary>
        public IPower Power { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// PWM接口
        /// </summary>
        public IPWM PWM { get; set; }

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        {
            get
            {
                if ((Scope?.IsConnect == true) && 
                    (Power?.IsConnect == true) && 
                    (PLC?.IsConnect == true) && 
                    (PWM?.IsConnect == true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 更新硬件
        /// </summary>
        public void UpdateHardware()
        {
            if (Scope?.IsConnect != true)
            {
                Scope?.Connect();
            }

            if (Power?.IsConnect != true)
            {
                Power?.Connect();
            }

            if (PLC?.IsConnect != true)
            {
                PLC?.Connect();
            }

            if (PWM?.IsConnect != true)
            {
                PWM?.Connect();
            }

            NotifyOfPropertyChange(() => IsHardwareValid);
            NotifyOfPropertyChange(() => CanMeasure);
        }

        #endregion

        #region 测量信息

        private ObservableCollection<BurnInTestInfo> measurementInfos;

        /// <summary>
        /// 测量信息
        /// </summary>
        public ObservableCollection<BurnInTestInfo> MeasurementInfos
        {
            get
            {
                return measurementInfos;
            }
            set
            {
                measurementInfos = value;
                NotifyOfPropertyChange(() => MeasurementInfos);
            }
        }

        private bool isMeasuring;

        /// <summary>
        /// 测量标志
        /// </summary>
        public bool IsMeasuring
        {
            get
            {
                return isMeasuring;
            }
            set
            {
                isMeasuring = value;
                NotifyOfPropertyChange(() => IsMeasuring);
            }
        }

        private bool canMeasure = true;

        /// <summary>
        /// 允许测量
        /// </summary>
        public bool CanMeasure
        {
            get
            {
                if (IsHardwareValid)
                {
                    return canMeasure;
                }

                return false;
            }
            set
            {
                canMeasure = value;
                NotifyOfPropertyChange(() => CanMeasure);
            }
        }


        private string runningStatus = "就绪";

        /// <summary>
        /// 运行状态
        /// </summary>
        public string RunningStatus
        {
            get
            {
                return runningStatus;
            }
            set
            {
                runningStatus = value;
                NotifyOfPropertyChange(() => RunningStatus);
            }
        }

        private int currentSampleTime;

        /// <summary>
        /// 当前采样时间
        /// </summary>
        public int CurrentSampleTime
        {
            get
            {
                return currentSampleTime;
            }
            set
            {
                currentSampleTime = value;
                NotifyOfPropertyChange(() => CurrentSampleTime);
            }
        }

        private int currentInputFrequency;

        /// <summary>
        /// 当前输入频率
        /// </summary>
        public int CurrentInputFrequency
        {
            get
            {
                return currentInputFrequency;
            }
            set
            {
                currentInputFrequency = value;
                NotifyOfPropertyChange(() => CurrentInputFrequency);
            }
        }

        #endregion

        #region 配置参数

        private int frequency;

        /// <summary>
        /// 测试频率(Hz)
        /// </summary>
        public int Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                NotifyOfPropertyChange(() => Frequency);
                SystemParamManager.SystemParam.BurnInTestParams.Frequency = value;
                SystemParamManager.SaveParams();
            }
        }

        private int plcCount;

        /// <summary>
        /// 测试次数
        /// </summary>
        public int PLCCount
        {
            get
            {
                return plcCount;
            }
            set
            {
                plcCount = value;
                NotifyOfPropertyChange(() => PLCCount);
                SystemParamManager.SystemParam.BurnInTestParams.PLCCount = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 测量开始事件
        /// </summary>
        public event EventHandler<EventArgs> MeasurementStarted;

        /// <summary>
        /// 测量开始事件
        /// </summary>
        protected void OnMeasurementStarted()
        {
            CanMeasure = false;
            MeasurementStarted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<BurnInTestCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(BurnInTestCompletedEventArgs e)
        {
            CanMeasure = true;

            Power.IsEnableOutput = false;
            PLC.PWMSwitch = false;
            PWM.Frequency = 0;
            if (e.IsSuccess == true)
            {
                RunningStatus = "成功";
            }
            else
            {
                RunningStatus = "失败";
            }

            lock (lockObject)
            {
                IsMeasuring = false;
            }

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

        private ObservableCollection<Data> scopeChACollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHACollection
        {
            get
            {
                return scopeChACollection;
            }
            set
            {
                scopeChACollection = value;
                NotifyOfPropertyChange(() => ScopeCHACollection);
            }
        }

        private ObservableCollection<Data> scopeChAEdgeCollection;

        /// <summary>
        /// 通道A边沿数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHAEdgeCollection
        {
            get
            {
                return scopeChAEdgeCollection;
            }
            set
            {
                scopeChAEdgeCollection = value;
                NotifyOfPropertyChange(() => ScopeCHAEdgeCollection);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowEdgeData(double[] data, List<int> edgeIndexs, DigitEdgeType digitEdgeType)
        {
            int SampleInterval = 1;

            var collection = new ObservableCollection<Data>();
            for (int i = 0; i < data.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = data[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
            }
            var collection2 = new ObservableCollection<Data>();
            if ((digitEdgeType == DigitEdgeType.FirstFillingEdge) || (digitEdgeType == DigitEdgeType.FirstRisingEdge))
            {
                foreach (var item in edgeIndexs)
                {
                    //collection.Add(new Data() { Value1 = ScopeCHACollection[item / SampleInterval].Value1, Value = item * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    collection2.Add(new Data() { Value1 = collection[item / SampleInterval].Value1, Value = collection[item / SampleInterval].Value });
                }
            }

            ScopeCHACollection = collection;
            ScopeCHAEdgeCollection = collection2;
        }

        #endregion

        #region 方法

        private object lockObject = new object();

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 电压转气压
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        private double VoltageToPressure(double voltage)
        {

            return (voltage - SystemParamManager.SystemParam.GlobalParam.PressureZeroVoltage) * SystemParamManager.SystemParam.GlobalParam.PressureK;
        }

        /// <summary>
        /// 显示测量信息
        /// </summary>
        /// <param name="currentInputFrequency"></param>
        /// <param name="currentSampleTime"></param>
        /// <param name="pulseFrequencies"></param>
        /// <param name="scopeCHACollection"></param>
        /// <param name="scopeCHAEdgeCollection"></param>
        private void ShowMeasureInfos(int currentInputFrequency, int currentSampleTime, List<double> pulseFrequencies)
        {
            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    SynchronizationContext.Current.Send(pl =>
                    {
                        MeasurementInfos.Insert(0, new BurnInTestInfo(currentInputFrequency, currentSampleTime, pulseFrequencies));
                    }, null);
                });
            }).Start();
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            //设置PLC开关模式
            PLC.PWMSwitch = true;
            Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);
            MeasurementInfos = new ObservableCollection<BurnInTestInfo>();

            OnMeasurementStarted();

            measureThread = new Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                //获取运行时长(单位:MS)
                double testTime = (double)PLCCount * 1000.0 / Frequency;

                //设置频率
                PWM.Frequency = 0;
                PWM.DutyRatio = SystemParamManager.SystemParam.FrequencyMeasureParams.DutyRatio;

                //使能Power输出
                Power.Voltage = SystemParamManager.SystemParam.FrequencyMeasureParams.OutputVoltage;
                Power.IsEnableOutput = true;

                //设置实际输出频率
                PWM.Frequency = Frequency;

                //设置Scope采集时长
                Scope.SampleTime = 1000;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.Elapsed.TotalMilliseconds < testTime)
                {
                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadDataBlock(0, out originalData);

                    //假如读取不到示波器数据,则报测量失败
                    if ((originalData == null) || (originalData.Length == 0))
                    {
                        //测试失败
                        OnMeasurementCompleted(new BurnInTestCompletedEventArgs());
                        return;
                    }

                    //数据滤波
                    double[] filterData;
                    Analysis.MeanFilter(originalData, SystemParamManager.SystemParam.FrequencyMeasureParams.VoltageFilterCount, out filterData);

                    //电压转气压
                    double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                    //阈值查找边沿
                    List<int> edgeIndexs;
                    DigitEdgeType digitEdgeType;
                    Analysis.FindEdgeByThreshold(pressureData, SystemParamManager.SystemParam.FrequencyMeasureParams.MinPressure,
                        SystemParamManager.SystemParam.FrequencyMeasureParams.MaxPressure,
                        out edgeIndexs, out digitEdgeType);

                    //显示波形
                    ShowEdgeData(pressureData, edgeIndexs, digitEdgeType);

                    //分析脉冲数据
                    List<double> pulseFrequencies;
                    List<double> dutyRatios;
                    Analysis.AnalysePulseData(edgeIndexs, digitEdgeType, (int)Scope.SampleRate, out pulseFrequencies, out dutyRatios);

                    //显示测量信息
                    ShowMeasureInfos(CurrentInputFrequency, CurrentSampleTime, pulseFrequencies);

                    if (pulseFrequencies.Count > 0)
                    {
                        //检测脉冲是否异常
                        double minFrequency = Frequency * (1 - SystemParamManager.SystemParam.FrequencyMeasureParams.FrequencyErrLimit);
                        double maxFrequency = Frequency * (1 + SystemParamManager.SystemParam.FrequencyMeasureParams.FrequencyErrLimit);

                        //若波形不符,则退出测试
                        if (!Analysis.CheckFrequency(pulseFrequencies, minFrequency, maxFrequency, 0))
                        {
                            //保存错误信息
                            errPressureData = new double[pressureData.Length];
                            Array.Copy(pressureData, errPressureData, pressureData.Length);
                            errEdgeIndexs = new List<int>(edgeIndexs);
                            errDigitEdgeType = digitEdgeType;

                            //测试失败
                            OnMeasurementCompleted(new BurnInTestCompletedEventArgs());
                            return;
                        }
                    }
                    else
                    {
                        //保存错误信息
                        errPressureData = new double[pressureData.Length];
                        Array.Copy(pressureData, errPressureData, pressureData.Length);
                        errEdgeIndexs = new List<int>(edgeIndexs);
                        errDigitEdgeType = digitEdgeType;

                        //测试失败
                        OnMeasurementCompleted(new BurnInTestCompletedEventArgs());
                        return;
                    }

                    Thread.Sleep(200);
                }

                //测试成功
                OnMeasurementCompleted(new BurnInTestCompletedEventArgs(true));

                stopwatch.Stop();
            });

            measureThread.Start();
        }

        private double[] errPressureData;
        private List<int> errEdgeIndexs = new List<int>();
        private DigitEdgeType errDigitEdgeType;

        /// <summary>
        /// 显示上次异常的错误图形
        /// </summary>
        public void ShowErrorFigure()
        {
            if (!IsMeasuring)
            {
                if ((errPressureData?.Length > 0))
                {
                    //显示波形
                    ShowEdgeData(errPressureData, errEdgeIndexs, errDigitEdgeType);
                }
                else
                {
                    OnMessageRaised(MessageLevel.Warning, "无异常");
                }
            }

        }

        #endregion
    }
}
