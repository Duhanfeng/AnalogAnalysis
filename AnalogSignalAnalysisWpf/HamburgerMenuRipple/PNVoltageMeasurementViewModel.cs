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

    public class PNVoltageMeasurementInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 当前状态(检测吸合/释放)
        /// </summary>
        public string CurrentStatus { get; set; }

        /// <summary>
        /// 当前电压(输出电压)
        /// </summary>
        public double CurrentVoltage { get; set; }

        /// <summary>
        /// 当前气压
        /// </summary>
        public double CurrentPressure { get; set; }

        /// <summary>
        /// 测量电平
        /// </summary>
        public string MeasureLevel { get; set; }

        /// <summary>
        /// 测试结果(无/成功)
        /// </summary>
        public string MeasureResult { get; set; }

        /// <summary>
        /// 创建FrequencyMeasurementInfo新实例
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sampleTime"></param>
        /// <param name="currentFrequency"></param>
        public PNVoltageMeasurementInfo(string currentStatus, double currentVoltage, double currentPressure, string measureLevel, string measureResult)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentStatus = currentStatus;
            CurrentVoltage = currentVoltage;
            CurrentPressure = currentPressure;
            MeasureLevel = measureLevel;
            MeasureResult = measureResult;
        }

    }

    public class PNVoltageMeasurementViewModel : Screen
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


        public PNVoltageMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            CriticalPressure = SystemParamManager.SystemParam.PNVoltageMeasureParams.CriticalPressure;
            VoltageInterval = SystemParamManager.SystemParam.PNVoltageMeasureParams.VoltageInterval;
            MinVoltage = SystemParamManager.SystemParam.PNVoltageMeasureParams.MinVoltage;
            MaxVoltage = SystemParamManager.SystemParam.PNVoltageMeasureParams.MaxVoltage;
            SampleTime = SystemParamManager.SystemParam.PNVoltageMeasureParams.SampleTime;
        }

        /// <summary>
        /// 创建PNVoltageMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="power">Power接口</param>
        public PNVoltageMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
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

        #region 配置参数

        private double criticalPressure = 3;

        /// <summary>
        /// 临界气压
        /// </summary>
        public double CriticalPressure
        {
            get
            {
                return criticalPressure;
            }
            set
            {
                criticalPressure = value;
                NotifyOfPropertyChange(() => CriticalPressure);

                SystemParamManager.SystemParam.PNVoltageMeasureParams.CriticalPressure = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.VoltageInterval = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.MinVoltage = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.MaxVoltage = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.SampleTime = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 测量信息

        private ObservableCollection<PNVoltageMeasurementInfo> measurementInfos;

        /// <summary>
        /// 测量信息
        /// </summary>
        public ObservableCollection<PNVoltageMeasurementInfo> MeasurementInfos
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

        private double currentVoltage;

        /// <summary>
        /// 当前电压(输出电压)
        /// </summary>
        public double CurrentVoltage
        {
            get
            {
                return currentVoltage;
            }
            set
            {
                currentVoltage = value;
                NotifyOfPropertyChange(() => CurrentVoltage);
            }
        }

        private double currentPressure;

        /// <summary>
        /// 当前气压
        /// </summary>
        public double CurrentPressure
        {
            get
            {
                return currentPressure;
            }
            set
            {
                currentPressure = value;
                NotifyOfPropertyChange(() => CurrentPressure);
            }
        }

        private double pVoltage;

        /// <summary>
        /// 吸合电压
        /// </summary>
        public double PVoltage
        {
            get
            {
                return pVoltage;
            }
            set
            {
                pVoltage = value;
                NotifyOfPropertyChange(() => PVoltage);
            }
        }

        private double nVoltage;

        /// <summary>
        /// 释放电压
        /// </summary>
        public double NVoltage
        {
            get
            {
                return nVoltage;
            }
            set
            {
                nVoltage = value;
                NotifyOfPropertyChange(() => NVoltage);
            }
        }

        #endregion

        #region 折线图

        private ObservableCollection<Data> pVoltageCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> PVoltageCollection
        {
            get
            {
                return pVoltageCollection;
            }
            set
            {
                pVoltageCollection = value;
                NotifyOfPropertyChange(() => PVoltageCollection);
            }
        }

        private ObservableCollection<Data> pVoltageEdgeCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> PVoltageEdgeCollection
        {
            get
            {
                return pVoltageEdgeCollection;
            }
            set
            {
                pVoltageEdgeCollection = value;
                NotifyOfPropertyChange(() => PVoltageEdgeCollection);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowPEdgeData(double[] data, List<int> edgeIndexs, DigitEdgeType digitEdgeType)
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
                    //collection.Add(new Data() { Value1 = PVoltageCollection[item / SampleInterval].Value1, Value = item * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    collection2.Add(new Data() { Value1 = collection[item / SampleInterval].Value1, Value = collection[item / SampleInterval].Value });
                }
            }

            PVoltageCollection = collection;
        }

        private ObservableCollection<Data> nVoltageCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> NVoltageCollection
        {
            get
            {
                return nVoltageCollection;
            }
            set
            {
                nVoltageCollection = value;
                NotifyOfPropertyChange(() => NVoltageCollection);
            }
        }

        private ObservableCollection<Data> nVoltageEdgeCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> NVoltageEdgeCollection
        {
            get
            {
                return nVoltageEdgeCollection;
            }
            set
            {
                nVoltageEdgeCollection = value;
                NotifyOfPropertyChange(() => NVoltageEdgeCollection);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowNEdgeData(double[] data, List<int> edgeIndexs, DigitEdgeType digitEdgeType)
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
                    //collection.Add(new Data() { Value1 = NVoltageCollection[item / SampleInterval].Value1, Value = item * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    collection2.Add(new Data() { Value1 = collection[item / SampleInterval].Value1, Value = collection[item / SampleInterval].Value });
                }
            }

            NVoltageCollection = collection;
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
        public event EventHandler<PNVoltageMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(PNVoltageMeasurementCompletedEventArgs e)
        {
            RunningStatus = e.IsSuccess ? "成功" : "失败";
            CanMeasure = true;

            if (Power?.IsConnect == true)
            {
                Power.IsEnableOutput = false;
            }

            PVoltage = e.PositiveVoltage;
            NVoltage = e.NegativeVoltage;

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
        /// 显示吸合测试数据
        /// </summary>
        /// <param name="time">当前时间</param>
        /// <param name="currentVoltage">当前电压</param>
        /// <param name="currentPressure">当前气压</param>
        /// <param name="isSuccess">执行结果</param>
        private void ShowPData(double time, double currentVoltage, double currentPressure, bool isSuccess)
        {
            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    SynchronizationContext.Current.Send(pl =>
                    {
                        PVoltageCollection.Add(new Data
                        {
                            Value1 = currentVoltage,
                            Value = time
                        });

                        if (isSuccess)
                        {
                            PVoltageEdgeCollection.Add(new Data
                            {
                                Value1 = currentVoltage,
                                Value = time
                            });
                            MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("已吸合", currentVoltage, currentPressure, "H", "吸合电压"));
                        }
                        else
                        {
                            NVoltageEdgeCollection.Add(new Data
                            {
                                Value1 = currentVoltage,
                                Value = time
                            });
                            MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("测量吸合", currentVoltage, currentPressure, "L", "无效"));
                        }
                    }, null);
                });
            }).Start();
        }

        /// <summary>
        /// 显示释放测试数据
        /// </summary>
        /// <param name="time">当前时间</param>
        /// <param name="currentVoltage">当前电压</param>
        /// <param name="currentPressure">当前气压</param>
        /// <param name="isSuccess">执行结果</param>
        private void ShowNData(double time, double currentVoltage, double currentPressure, bool isSuccess)
        {
            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    SynchronizationContext.Current.Send(pl =>
                    {
                        PVoltageCollection.Add(new Data
                        {
                            Value1 = currentVoltage,
                            Value = time
                        });

                        if (isSuccess)
                        {
                            PVoltageEdgeCollection.Add(new Data
                            {
                                Value1 = currentVoltage,
                                Value = time
                            });
                            MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("已释放", currentVoltage, currentPressure, "L", "释放电压"));
                        }
                        else
                        {
                            NVoltageEdgeCollection.Add(new Data
                            {
                                Value1 = currentVoltage,
                                Value = time
                            });
                            MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("测量释放", currentVoltage, currentPressure, "H", "无效"));
                        }
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

            if (MinVoltage > MaxVoltage)
            {
                throw new ArgumentException("MinVoltage > MaxVoltage");
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            //设置电源模块直通
            PLC.PWMSwitch = false;

            PVoltage = -1;
            NVoltage = -1;

            RunningStatus = "运行中";
            MeasurementInfos = new ObservableCollection<PNVoltageMeasurementInfo>();

            PVoltageCollection = new ObservableCollection<Data>();
            PVoltageEdgeCollection = new ObservableCollection<Data>();
            NVoltageCollection = new ObservableCollection<Data>();
            NVoltageEdgeCollection = new ObservableCollection<Data>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            OnMeasurementStarted();

            measureThread = new Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                bool isSuccess = false;



                double positiveVoltage = 0;
                double negativeVoltage = 0;

                //设置采样时间
                Scope.SampleTime = SampleTime;
                if (SampleTime <= 50)
                {
                    Scope.SampleRate = ESampleRate.Sps_781K;
                }

                //设置PWM
                PWM.Frequency = 0;
                PWM.DutyRatio = 50;

                //设置电源输出
                double currentVoltage = MinVoltage;
                Power.Voltage = currentVoltage;
                Power.IsEnableOutput = true;

                double roughPositiveVoltage = 0;
                double roughNegativeVoltage = 0;
                var roughVoltageInterval = VoltageInterval < 1 ? 1 : VoltageInterval;

                int measureStep = 0;

                while (true)
                {
                    switch (measureStep)
                    {
                        case 0:
                            RunningStatus = "查找吸合电压(粗)";

                            //测量释放电压
                            while (currentVoltage <= MaxVoltage)
                            {
                                //设置当前电压
                                Power.Voltage = currentVoltage;
                                Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);
                                CurrentVoltage = currentVoltage;

                                //读取Scope数据
                                double[] originalData;
                                Scope.ReadDataBlock(0, out originalData);

                                //数据滤波
                                double[] filterData;
                                Analysis.MeanFilter(originalData, 7, out filterData);

                                //电压转气压
                                double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                                //获取平均值
                                CurrentPressure = Analysis.Mean(pressureData);

                                stopwatch.Stop();

                                //假如当前气压值大于等于临界值,则认为测试有效
                                if (CurrentPressure >= CriticalPressure)
                                {
                                    ShowPData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, true);

                                    roughPositiveVoltage = currentVoltage;
                                    isSuccess = true;
                                    break;
                                }
                                else
                                {
                                    ShowPData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, false);
                                }

                                stopwatch.Start();

                                currentVoltage += roughVoltageInterval;
                            }

                            if (isSuccess)
                            {
                                measureStep++;
                            }
                            else
                            {
                                measureStep = -1;
                            }

                            break;

                        case 1:
                            RunningStatus = "查找释放电压(粗)";
                            
                            //测量释放电压
                            while (currentVoltage >= 0)
                            {
                                //设置当前电压
                                Power.Voltage = currentVoltage;
                                Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);
                                CurrentVoltage = currentVoltage;

                                //读取Scope数据
                                double[] originalData;
                                Scope.ReadDataBlock(0, out originalData);

                                //数据滤波
                                double[] filterData;
                                Analysis.MeanFilter(originalData, 7, out filterData);

                                //电压转气压
                                double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                                //获取平均值
                                CurrentPressure = Analysis.Mean(pressureData);

                                stopwatch.Stop();

                                //假如当前气压值小于等于临界值,则认为测试有效
                                if (CurrentPressure <= CriticalPressure)
                                {
                                    ShowNData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, true);

                                    roughNegativeVoltage = currentVoltage;
                                    isSuccess = true;
                                    break;
                                }
                                else
                                {
                                    ShowNData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, false);
                                }

                                stopwatch.Start();

                                currentVoltage -= roughVoltageInterval;
                            }

                            if (isSuccess)
                            {
                                measureStep++;
                            }
                            else
                            {
                                measureStep = -1;
                            }

                            break;

                        case 2:
                            RunningStatus = "查找吸合电压(精细)";
                            var temp = roughPositiveVoltage - roughVoltageInterval * 2;
                            currentVoltage = temp < 0 ? 0 : temp;
                            //currentVoltage = MinVoltage;

                            //测量吸合电压
                            while (currentVoltage <= MaxVoltage)
                            {
                                //设置当前电压
                                Power.Voltage = currentVoltage;
                                Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);
                                CurrentVoltage = currentVoltage;

                                //读取Scope数据
                                double[] originalData;
                                Scope.ReadDataBlock(0, out originalData);

                                //数据滤波
                                double[] filterData;
                                Analysis.MeanFilter(originalData, 7, out filterData);

                                //电压转气压
                                double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                                //获取平均值
                                CurrentPressure = Analysis.Mean(pressureData);

                                stopwatch.Stop();

                                //假如当前气压值大于等于临界值,则认为测试有效
                                if (CurrentPressure >= CriticalPressure)
                                {
                                    ShowPData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, true);

                                    positiveVoltage = currentVoltage;
                                    isSuccess = true;
                                    break;
                                }
                                else
                                {
                                    ShowPData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, false);
                                }

                                stopwatch.Start();

                                currentVoltage += VoltageInterval;
                            }

                            if (isSuccess)
                            {
                                measureStep++;
                                isSuccess = false;
                            }
                            else
                            {
                                measureStep = -1;
                            }

                            break;

                        case 3:
                            RunningStatus = "查找释放电压(精细)";
                            currentVoltage = (roughNegativeVoltage <= 0 ? 0 : roughNegativeVoltage) + roughVoltageInterval * 2;
                            //currentVoltage = positiveVoltage + 1;

                            //测量释放电压
                            while (currentVoltage >= 0)
                            {
                                //设置当前电压
                                Power.Voltage = currentVoltage;
                                Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);
                                CurrentVoltage = currentVoltage;

                                //读取Scope数据
                                double[] originalData;
                                Scope.ReadDataBlock(0, out originalData);

                                //数据滤波
                                double[] filterData;
                                Analysis.MeanFilter(originalData, 7, out filterData);

                                //电压转气压
                                double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                                //获取平均值
                                CurrentPressure = Analysis.Mean(pressureData);

                                stopwatch.Stop();

                                //假如当前气压值小于等于临界值,则认为测试有效
                                if (CurrentPressure <= CriticalPressure)
                                {
                                    ShowNData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, true);

                                    negativeVoltage = currentVoltage;
                                    isSuccess = true;
                                    break;
                                }
                                else
                                {
                                    ShowNData(stopwatch.Elapsed.TotalMilliseconds, CurrentVoltage, CurrentPressure, false);
                                }

                                stopwatch.Start();

                                currentVoltage -= VoltageInterval;
                            }

                            if (isSuccess)
                            {
                                measureStep++;
                            }
                            else
                            {
                                measureStep = -1;
                            }

                            break;

                        case 4:
                            //测试成功
                            OnMeasurementCompleted(new PNVoltageMeasurementCompletedEventArgs(true, positiveVoltage, negativeVoltage));
                            return;

                        default:
                            OnMeasurementCompleted(new PNVoltageMeasurementCompletedEventArgs());
                            return;
                    }
                }
            });

            measureThread.Start();
        }

        #endregion
    }
}
