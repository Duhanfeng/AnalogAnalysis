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

    public class InputOutputMeasurementInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 输入电压
        /// </summary>
        public double Input { get; set; }

        /// <summary>
        /// 输出电压(检测电压)
        /// </summary>
        public double Output { get; set; }

        /// <summary>
        /// 创建InputOutputMeasurementInfo新实例
        /// </summary>
        public InputOutputMeasurementInfo()
        {

        }

        /// <summary>
        /// 创建InputOutputMeasurementInfo新实例
        /// </summary>
        /// <param name="input">输入电压</param>
        /// <param name="output">输出电压</param>
        public InputOutputMeasurementInfo(double input, double output)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Input = input;
            Output = output;
        }
    }

    public class InputOutputMeasurementViewModel : Screen
    {
        #region 构造函数

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        public InputOutputMeasurementViewModel()
        {
            ScopeScale = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EScale>());
            ScopeSampleRateCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ESampleRate>());
            ScopeVoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());

            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            VoltageInterval = SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageInterval;
            MinVoltage = SystemParamManager.SystemParam.InputOutputMeasureParams.MinVoltage;
            MaxVoltage = SystemParamManager.SystemParam.InputOutputMeasureParams.MaxVoltage;
            SampleTime = SystemParamManager.SystemParam.InputOutputMeasureParams.SampleTime;
            ComDelay = SystemParamManager.SystemParam.InputOutputMeasureParams.ComDelay;
            CHAScale = SystemParamManager.SystemParam.FrequencyMeasureParams.CHAScale;
            CHAVoltageDIV = SystemParamManager.SystemParam.FrequencyMeasureParams.CHAVoltageDIV;
            SampleRate = SystemParamManager.SystemParam.FrequencyMeasureParams.SampleRate;

            NotifyOfPropertyChange(() => ScopeCHAScale);
            NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            NotifyOfPropertyChange(() => ScopeSampleRate);

            //更新硬件状态
            UpdateHardware();
        }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器</param>
        /// <param name="plc">PLC</param>
        public InputOutputMeasurementViewModel(IScopeBase scope, IPLC plc) : this()
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

                SystemParamManager.SystemParam.FrequencyMeasureParams.CHAScale = CHAScale;
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

                SystemParamManager.SystemParam.FrequencyMeasureParams.CHAVoltageDIV = CHAVoltageDIV;
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

                SystemParamManager.SystemParam.FrequencyMeasureParams.SampleRate = SampleRate;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 配置参数

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

                SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageInterval = value;
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

                SystemParamManager.SystemParam.InputOutputMeasureParams.MinVoltage = value;
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

                SystemParamManager.SystemParam.InputOutputMeasureParams.MaxVoltage = value;
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

                SystemParamManager.SystemParam.InputOutputMeasureParams.SampleTime = value;
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

                SystemParamManager.SystemParam.InputOutputMeasureParams.ComDelay = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 测量信息

        private ObservableCollection<InputOutputMeasurementInfo> measurementInfos;

        /// <summary>
        /// 测量信息
        /// </summary>
        public ObservableCollection<InputOutputMeasurementInfo> MeasurementInfos
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

        private double currentInput;

        /// <summary>
        /// 当前输入电压
        /// </summary>
        public double CurrentInput
        {
            get
            {
                return currentInput;
            }
            set
            {
                currentInput = value;
                NotifyOfPropertyChange(() => CurrentInput);
            }
        }

        private double currentOutput;

        /// <summary>
        /// 当前输出电压(测量电压)
        /// </summary>
        public double CurrentOutput
        {
            get
            {
                return currentOutput;
            }
            set
            {
                currentOutput = value;
                NotifyOfPropertyChange(() => CurrentOutput);
            }
        }

        #endregion

        #region 折线图

        private ObservableCollection<Data> scopeCHACollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHACollection
        {
            get
            {
                return scopeCHACollection;
            }
            set
            {
                scopeCHACollection = value;
                NotifyOfPropertyChange(() => ScopeCHACollection);
            }
        }

        private ObservableCollection<Data> scopeCHBCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHBCollection
        {
            get
            {
                return scopeCHBCollection;
            }
            set
            {
                scopeCHBCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBCollection);
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
        }

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
            lock (lockObject)
            {
                IsMeasuring = false;
            }

            if (PLC?.IsConnect == true)
            {
                PLC.EnableOutput = false;
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

            ScopeCHACollection = new ObservableCollection<Data>();
            ScopeCHBCollection = new ObservableCollection<Data>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            measureThread = new System.Threading.Thread(() =>
            {
                List<double> inputs = new List<double>();
                List<double> outputs = new List<double>();

                if (MinVoltage > MaxVoltage)
                {
                    throw new ArgumentException("MinVoltage > MaxVoltage");
                }

                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                MeasurementInfos = new ObservableCollection<InputOutputMeasurementInfo>();

                double currentVoltage = MinVoltage;
                PLC.Voltage = currentVoltage;
                PLC.EnableOutput = true;

                int count = 0;
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
                    CurrentInput = currentVoltage;
                    CurrentOutput = medianData;

                    new Thread(delegate ()
                    {
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                            System.Threading.SynchronizationContext.Current.Send(pl =>
                            {
                                stopwatch.Stop();

                                ScopeCHACollection.Add(new Data
                                {
                                    Value1 = CurrentInput,
                                    Value = stopwatch.Elapsed.TotalMilliseconds
                                });

                                ScopeCHBCollection.Add(new Data
                                {
                                    Value1 = CurrentOutput,
                                    Value = stopwatch.Elapsed.TotalMilliseconds
                                });

                                stopwatch.Start();

                                MeasurementInfos.Add(new InputOutputMeasurementInfo(CurrentInput, CurrentOutput));
                            }, null);
                        });
                    }).Start();

                    currentVoltage += VoltageInterval;
                    count++;
                }

                //输出结果
                OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs(true, inputs, outputs));

            });

            measureThread.Start();
        }

        #endregion

    }
}
