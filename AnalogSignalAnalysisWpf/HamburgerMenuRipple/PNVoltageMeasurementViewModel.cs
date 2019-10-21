using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public PNVoltageMeasurementInfo(string currentStatus, double currentVoltage, string measureLevel, string measureResult)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentStatus = currentStatus;
            CurrentVoltage = currentVoltage;
            MeasureLevel = measureLevel;
            MeasureResult = measureResult;
        }

    }

    public class PNVoltageMeasurementViewModel : Screen
    {
        #region 构造函数

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }


        public PNVoltageMeasurementViewModel()
        {
            ScopeScale = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EScale>());
            ScopeSampleRateCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ESampleRate>());
            ScopeVoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());


            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            MinVoltageThreshold = SystemParamManager.SystemParam.PNVoltageMeasureParams.MinVoltageThreshold;
            MaxVoltageThreshold = SystemParamManager.SystemParam.PNVoltageMeasureParams.MaxVoltageThreshold;
            FrequencyErrLimit = SystemParamManager.SystemParam.PNVoltageMeasureParams.FrequencyErrLimit;
            VoltageInterval = SystemParamManager.SystemParam.PNVoltageMeasureParams.VoltageInterval;
            MinVoltage = SystemParamManager.SystemParam.PNVoltageMeasureParams.MinVoltage;
            MaxVoltage = SystemParamManager.SystemParam.PNVoltageMeasureParams.MaxVoltage;
            SampleTime = SystemParamManager.SystemParam.PNVoltageMeasureParams.SampleTime;

            ComDelay = SystemParamManager.SystemParam.PNVoltageMeasureParams.ComDelay;
            CHAScale = SystemParamManager.SystemParam.PNVoltageMeasureParams.CHAScale;
            CHAVoltageDIV = SystemParamManager.SystemParam.PNVoltageMeasureParams.CHAVoltageDIV;
            SampleRate = SystemParamManager.SystemParam.PNVoltageMeasureParams.SampleRate;

        }

        /// <summary>
        /// 创建PNVoltageMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="plc">PLC接口</param>
        public PNVoltageMeasurementViewModel(IScopeBase scope, IPLC plc) : this()
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

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        {
            get
            {
                if ((Scope?.IsConnect == true) && (PLC?.IsConnect == true))
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
            NotifyOfPropertyChange(() => IsHardwareValid);
        }

        /// <summary>
        /// 放大倍数
        /// </summary>
        public ObservableCollection<string> ScopeScale { get; set; }

        /// <summary>
        /// 电压档位
        /// </summary>
        public ObservableCollection<string> ScopeVoltageDIVCollection { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ObservableCollection<string> ScopeSampleRateCollection { get; set; }

        private EScale CHAScale = EScale.x10;

        /// <summary>
        /// CHA探头衰变
        /// </summary>
        public string ScopeCHAScale
        {
            get
            {
                return EnumHelper.GetDescription(CHAScale);
            }
            set
            {
                CHAScale = EnumHelper.GetEnum<EScale>(value);
                NotifyOfPropertyChange(() => ScopeCHAScale);

                SystemParamManager.SystemParam.PNVoltageMeasureParams.CHAScale = CHAScale;
                SystemParamManager.SaveParams();
            }
        }

        private EVoltageDIV CHAVoltageDIV = EVoltageDIV.DIV_2V5;

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public string ScopeCHAVoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(CHAVoltageDIV);
            }
            set
            {
                CHAVoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);

                SystemParamManager.SystemParam.PNVoltageMeasureParams.CHAVoltageDIV = CHAVoltageDIV;
                SystemParamManager.SaveParams();
            }
        }

        private ESampleRate SampleRate = ESampleRate.Sps_49K;

        /// <summary>
        /// 采样率
        /// </summary>
        public string ScopeSampleRate
        {
            get
            {
                return EnumHelper.GetDescription(SampleRate);
            }
            set
            {
                SampleRate = EnumHelper.GetEnum<ESampleRate>(value);
                NotifyOfPropertyChange(() => ScopeSampleRate);

                SystemParamManager.SystemParam.PNVoltageMeasureParams.SampleRate = SampleRate;
                SystemParamManager.SaveParams();
            }
        }

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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.MinVoltageThreshold = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.MaxVoltageThreshold = value;
                SystemParamManager.SaveParams();
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.FrequencyErrLimit = value;
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

                SystemParamManager.SystemParam.PNVoltageMeasureParams.ComDelay = value;
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

            if (PLC?.IsConnect == true)
            {
                PLC.EnableOutput = false;
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

        #region 方法

        private object lockObject = new object();

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

            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = CHAScale;
            Scope.SampleRate = SampleRate;
            Scope.CHAVoltageDIV = CHAVoltageDIV;
            Scope.SampleTime = SampleTime;

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

            measureThread = new Thread(() =>
            {
                if (MinVoltage > MaxVoltage)
                {
                    throw new ArgumentException("MinVoltage > MaxVoltage");
                }

                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                bool isSuccess = false;
                double positiveVoltage = 0;
                double negativeVoltage = 0;

                RunningStatus = "查找吸合电压";

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

                    CurrentVoltage = currentVoltage;
                    
                    //如果为上升沿或者是高电平,则有效
                    if ((digitEdgeType == DigitEdgeType.FirstRisingEdge) || (digitEdgeType == DigitEdgeType.HeightLevel))
                    {
                        PVoltage = CurrentVoltage;

                        new Thread(delegate ()
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                System.Threading.SynchronizationContext.Current.Send(pl =>
                                {
                                    stopwatch.Stop();

                                    PVoltageCollection.Add(new Data
                                    {
                                        Value1 = CurrentVoltage,
                                        Value = stopwatch.Elapsed.TotalMilliseconds
                                    });
                                    PVoltageEdgeCollection.Add(new Data
                                    {
                                        Value1 = CurrentVoltage,
                                        Value = stopwatch.Elapsed.TotalMilliseconds
                                    });

                                    stopwatch.Start();

                                    //MeasurementInfos.Add(new PNVoltageMeasurementInfo("吸合", CurrentVoltage, "H", "吸合电压"));
                                    MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("吸合", CurrentVoltage, "H", "吸合电压"));
                                }, null);
                            });
                        }).Start();

                        positiveVoltage = currentVoltage;
                        isSuccess = true;
                        break;
                    }
                    else
                    {
                        new Thread(delegate ()
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                System.Threading.SynchronizationContext.Current.Send(pl =>
                                {
                                    stopwatch.Stop();

                                    PVoltageCollection.Add(new Data
                                    {
                                        Value1 = CurrentVoltage,
                                        Value = stopwatch.Elapsed.TotalMilliseconds
                                    });

                                    NVoltageEdgeCollection.Add(new Data
                                    {
                                        Value1 = CurrentVoltage,
                                        Value = stopwatch.Elapsed.TotalMilliseconds
                                    });

                                    stopwatch.Start();

                                    MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("测量吸合", CurrentVoltage, "L", "无效"));
                                }, null);
                            });
                        }).Start();
                    }

                    currentVoltage += VoltageInterval;
                }

                if (isSuccess)
                {
                    RunningStatus = "查找释放电压";

                    isSuccess = false;
                    currentVoltage = positiveVoltage + 2;
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

                        CurrentVoltage = currentVoltage;

                        //如果为上升沿或者是高电平,则有效
                        if ((digitEdgeType == DigitEdgeType.FirstFillingEdge) || (digitEdgeType == DigitEdgeType.LowLevel))
                        {
                            NVoltage = CurrentVoltage;

                            new Thread(delegate ()
                            {
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                    System.Threading.SynchronizationContext.Current.Send(pl =>
                                    {
                                        stopwatch.Stop();

                                        PVoltageCollection.Add(new Data
                                        {
                                            Value1 = CurrentVoltage,
                                            Value = stopwatch.Elapsed.TotalMilliseconds
                                        });

                                        PVoltageCollection.Add(new Data
                                        {
                                            Value1 = 0,
                                            Value = stopwatch.Elapsed.TotalMilliseconds + 4000
                                        });

                                        PVoltageEdgeCollection.Add(new Data
                                        {
                                            Value1 = CurrentVoltage,
                                            Value = stopwatch.Elapsed.TotalMilliseconds
                                        });

                                        stopwatch.Start();

                                        MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("释放", CurrentVoltage, "L", "释放电压"));
                                    }, null);
                                });
                            }).Start();

                            negativeVoltage = currentVoltage;
                            isSuccess = true;
                            break;
                        }
                        else
                        {
                            new Thread(delegate ()
                            {
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                    System.Threading.SynchronizationContext.Current.Send(pl =>
                                    {
                                        stopwatch.Stop();

                                        PVoltageCollection.Add(new Data
                                        {
                                            Value1 = CurrentVoltage,
                                            Value = stopwatch.Elapsed.TotalMilliseconds
                                        });

                                        NVoltageEdgeCollection.Add(new Data
                                        {
                                            Value1 = CurrentVoltage,
                                            Value = stopwatch.Elapsed.TotalMilliseconds
                                        });

                                        stopwatch.Start();

                                        MeasurementInfos.Insert(0, new PNVoltageMeasurementInfo("测量释放", CurrentVoltage, "H", "无效"));
                                    }, null);
                                });
                            }).Start();
                        }

                        currentVoltage -= VoltageInterval;
                    }

                    if (isSuccess)
                    {
                        OnMeasurementCompleted(new PNVoltageMeasurementCompletedEventArgs(true, positiveVoltage, negativeVoltage));
                        return;
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
