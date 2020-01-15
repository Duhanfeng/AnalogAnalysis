using AnalogSignalAnalysisWpf.Hardware;
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
using Framework.Infrastructure.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;
using CsvHelper;
using Sparrow.Chart;

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// 外部记录的数据
    /// </summary>
    public class ExternRecordData
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 最大电压
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public double MaxTime { get; set; }

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;

        /// <summary>
        /// 模板
        /// </summary>
        public ObservableCollection<DoublePoint> Template { get; set; }
    }

    public class NewIOMeasurementViewModel : Screen
    {

        /// <summary>
        /// 内部锁
        /// </summary>
        private object lockObject = new object();

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

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        public NewIOMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            //更新硬件状态
            UpdateHardware();
        }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器</param>
        /// <param name="power">Power</param>
        public NewIOMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
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

        #region 运行状态

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
                if (IsHardwareValid && IsConfigParamValid)
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

        private double currentInput;

        /// <summary>
        /// 当前输入电压(电源输出电压)
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

        #region 输入参数

        private string importFile;

        /// <summary>
        /// 导入的配置文件
        /// </summary>
        public string ImportFile
        {
            get { return importFile; }
            set { importFile = value; NotifyOfPropertyChange(() => ImportFile); }
        }

        /// <summary>
        /// 模板数据
        /// </summary>
        public ObservableCollection<Data> Template { get; set; } = new ObservableCollection<Data>();

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
#if true
                if (Scope?.IsConnect == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#else
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
#endif

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

#if false
        #region 显示参数

        private ObservableCollection<Data> powerCollection;

        /// <summary>
        /// 输出参数
        /// </summary>
        public ObservableCollection<Data> PowerCollection
        {
            get
            {
                return powerCollection;
            }
            set
            {
                powerCollection = value;
                NotifyOfPropertyChange(() => PowerCollection);
            }
        }

        private ObservableCollection<Data> powerCollection2;

        /// <summary>
        /// 输出参数
        /// </summary>
        public ObservableCollection<Data> PowerCollection2
        {
            get
            {
                return powerCollection2;
            }
            set
            {
                powerCollection2 = value;
                NotifyOfPropertyChange(() => PowerCollection2);
            }
        }

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

        private ObservableCollection<Data> scopeChBCollection;

        /// <summary>
        /// 通道B数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHBCollection
        {
            get
            {
                return scopeChBCollection;
            }
            set
            {
                scopeChBCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBCollection);
            }
        }

        private ObservableCollection<Data> templateDataCollection;

        /// <summary>
        /// 模板数据集合
        /// </summary>
        public ObservableCollection<Data> TemplateDataCollection
        {
            get { return templateDataCollection; }
            set { templateDataCollection = value; NotifyOfPropertyChange(() => TemplateDataCollection); }
        }

        private ObservableCollection<Data> tempPowerCollection = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempPowerCollection2 = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempCHACollection = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempCHBCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempPowerCollection.Add(item);
            }

            PowerCollection = new ObservableCollection<Data>(tempPowerCollection);
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection(Data data)
        {
            tempPowerCollection.Add(data);

            PowerCollection = new ObservableCollection<Data>(tempPowerCollection);

        }

        /// <summary>
        /// 清除电源集合
        /// </summary>
        private void ClearPowerCollection()
        {
            tempPowerCollection  = new ObservableCollection<Data>();
            PowerCollection = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection2(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempPowerCollection2.Add(item);
            }

            PowerCollection2 = new ObservableCollection<Data>(tempPowerCollection2);
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection2(Data data)
        {
            tempPowerCollection2.Add(data);

            PowerCollection2 = new ObservableCollection<Data>(tempPowerCollection2);

        }

        /// <summary>
        /// 清除电源集合
        /// </summary>
        private void ClearPowerCollection2()
        {
            tempPowerCollection2 = new ObservableCollection<Data>();
            PowerCollection2 = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加CHA
        /// </summary>
        /// <param name="datas"></param>
        private void AppendScopeCHACollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempCHACollection.Add(item);
            }

            ScopeCHACollection = new ObservableCollection<Data>(tempCHACollection);

        }
        
        /// <summary>
        /// 清除CHA集合
        /// </summary>
        private void ClearScopeCHACollection()
        {
            tempCHACollection = new ObservableCollection<Data>();
            ScopeCHACollection = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加CHB
        /// </summary>
        /// <param name="datas"></param>
        private void AppendScopeCHBCollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempCHBCollection.Add(item);
            }

            ScopeCHBCollection = new ObservableCollection<Data>(tempCHBCollection);

        }

        /// <summary>
        /// 清除CHB集合
        /// </summary>
        private void ClearScopeCHBCollection()
        {
            tempCHBCollection = new ObservableCollection<Data>();
            ScopeCHBCollection = new ObservableCollection<Data>();
        }

        #endregion
#endif

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
        public event EventHandler<EventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted()
        {
            CanMeasure = true;

            RunningStatus = "成功";

            PLC.PWMSwitch = false;
            PLC.FlowSwitch = false;

            if (Power?.IsConnect == true)
            {
                Power.IsEnableOutput = false;
            }

            lock (lockObject)
            {
                IsMeasuring = false;
            }
            MeasurementCompleted?.Invoke(this, new EventArgs());
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

        public event EventHandler<EventArgs> Compared;

        protected void OnCompared()
        {
            Compared?.Invoke(this, new EventArgs());
        }

        #endregion

#if false

        #region 应用

        /// <summary>
        /// 外部记录的数据
        /// </summary>
        public ExternRecordData ExternRecordData { get; set; }

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 采用间隔(界面)
        /// </summary>
        private readonly int SampleInterval = 10;

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

            //配置文件校验
            if (string.IsNullOrWhiteSpace(ImportFile) || !File.Exists(ImportFile))
            {
                OnMessageRaised(MessageLevel.Err, "配置文件无效");
                return;
            }

            //加载文件
            ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(ImportFile);

            if (!(ExternRecordData?.RecordVoltages?.Values?.Count > 0))
            {
                OnMessageRaised(MessageLevel.Err, "配置文件无效");
                return;
            }

            int totalTime = (ExternRecordData.RecordVoltages.Values.Count) * ExternRecordData.TimeInterval + 3000;

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.IsCHBEnable = true;
            Scope.CHBScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHBVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.SampleRate = ESampleRate.Sps_96K;

            //设置电源模块直通
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = true;

            //开启测量线程
            measureThread = new System.Threading.Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                OnMeasurementStarted();

                Stopwatch totalStopwatch = new Stopwatch();

                //设置示波器采集
                Scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                Scope.ScopeReadDataCompleted += Scope_ScopeReadDataCompleted;

                //清除界面数据
                ClearPowerCollection();
                ClearPowerCollection2();
                ClearScopeCHACollection();
                ClearScopeCHBCollection();
                TemplateDataCollection = new ObservableCollection<Data>();

                Power.Voltage = 0;
                Power.IsEnableOutput = true;

                //开始连续采集
                Scope.StartSerialSampple(totalTime);

                var collection = new ObservableCollection<Data>();

                int totalCount = ExternRecordData.RecordVoltages.Values.Count;

                int index = 0;
                double lastVol = 0;
                totalStopwatch.Start();
                foreach (var item in ExternRecordData.RecordVoltages)
                {
                    while (true)
                    {
                        if (totalStopwatch.Elapsed.TotalMilliseconds >= item.Key)
                        {
                            if (item.Value >= 0)
                            {
                                //设置当前电压
                                Power.Voltage = item.Value / 1000;

                                if (lastVol != item.Value)
                                {
                                    collection.Add(new Data() { Value1 = lastVol / 1000, Value = totalStopwatch.Elapsed.TotalMilliseconds });
                                }
                                collection.Add(new Data() { Value1 = item.Value / 1000, Value = totalStopwatch.Elapsed.TotalMilliseconds });
                                lastVol = item.Value;

                            }

                            index++;
                            break;
                        }
                        //Thread.Sleep(2);
                    }

                    AppendPowerCollection(collection);
                    collection = new ObservableCollection<Data>();

                    //if ((index % (totalCount / 1)) == 0)
                    //{
                        
                    //}
                }

                //AppendPowerCollection(new Data() { Value1 = lastVol / 1000, Value = index * ExternRecordData.TimeInterval });

                //输出完成电压后,设置相关标志位
                OnMeasurementCompleted();

            });

            measureThread.Start();
        }

        /// <summary>
        /// 比较
        /// </summary>
        public void Compare()
        {
            ClearPowerCollection2();
            AppendPowerCollection2(tempPowerCollection);

            new Thread(() => { Thread.Sleep(1000); OnCompared(); }).Start();

        }

        /// <summary>
        /// 显示完整的图表
        /// </summary>
        /// <param name="globleChannel1"></param>
        /// <param name="globleChannel2"></param>
        public void ShowCompleteGraph(double[] globleChannel1, double[] globleChannel2)
        {
            double lastValue = 0;
            ObservableCollection<Data> collectionA = new ObservableCollection<Data>();
            ObservableCollection<Data> collectionB = new ObservableCollection<Data>();

            double[] pressureChA = globleChannel1.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();
            double[] pressureChB = globleChannel2.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

            //double[] pressureChA = globleChannel1;
            //double[] pressureChB = globleChannel2;

            double[] filterData;
            Analysis.MeanFilter(pressureChA, 11, out filterData);

            for (int i = 0; i < pressureChA.Length; i++)
            {
                if (lastValue != filterData[i])
                {
                    collectionA.Add(new Data() { Value1 = lastValue, Value = i * 1000.0 / ((int)Scope.SampleRate) });
                }
                collectionA.Add(new Data() { Value1 = filterData[i], Value = i * 1000.0 / ((int)Scope.SampleRate) });
                lastValue = filterData[i];
            }

            lastValue = 0;
            Analysis.MeanFilter(pressureChB, 11, out filterData);

            for (int i = 0; i < pressureChB.Length; i++)
            {
                if (lastValue != filterData[i])
                {
                    collectionB.Add(new Data() { Value1 = lastValue, Value = i * 1000.0 / ((int)Scope.SampleRate) });
                }
                collectionB.Add(new Data() { Value1 = filterData[i], Value = i * 1000.0 / ((int)Scope.SampleRate) });
                lastValue = filterData[i];
            }

            ClearScopeCHACollection();
            ClearScopeCHBCollection();

            AppendScopeCHACollection(collectionA);
            AppendScopeCHBCollection(collectionB);

            //显示模板数据
            TemplateDataCollection = new ObservableCollection<Data>(Template);

            //Compare();
        }

        /// <summary>
        /// 示波器读取数据完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scope_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {

#if true
            double[] globleChannel1;
            double[] globleChannel2;
            double[] currentChannel1;
            double[] currentChannel2;

            e.GetData(out globleChannel1, out globleChannel2, out currentChannel1, out currentChannel2);

            double[] pressureChA = currentChannel1.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();
            double[] pressureChB = currentChannel2.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

            //设置通道A数据
            //数据滤波
            double[] filterData;
            Analysis.MeanFilter(pressureChA, 11, out filterData);

            //显示数据
            ObservableCollection<Data> collectionA = new ObservableCollection<Data>();

            for (int i = 0; i < pressureChA.Length / SampleInterval; i++)
            {
                collectionA.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel1.Length + i) * 1000.0 / ((int)Scope.SampleRate) });
            }

            ObservableCollection<Data> collectionB = new ObservableCollection<Data>();

            Analysis.MeanFilter(pressureChB, 11, out filterData);

            //显示通道B数据
            for (int i = 0; i < pressureChB.Length / SampleInterval; i++)
            {
                collectionB.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel2.Length + i) * 1000.0 / ((int)Scope.SampleRate) });
            }

            //显示图像
            AppendScopeCHACollection(collectionA);
            AppendScopeCHBCollection(collectionB);
#endif

            //当最后一包数据,则注销事件,并在2S后启动下一次测量任务
            if (e.CurrentPacket == (e.TotalPacket - 1))
            {
                var scope = sender as IScopeBase;
                scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;

                ShowCompleteGraph(globleChannel1, globleChannel2);

                //继续老化(若有)
                new Thread(() =>
                {
                    if ((currentBurnInTime % BackupInterval) == 0)
                    {
                        //备份截图
                        new Thread(() => { Thread.Sleep(1000); OnCompared(); }).Start();
                    }

                    currentBurnInTime++;
                    if (currentBurnInTime < TestTime)
                    {
                        Thread.Sleep(2000);
                        Start();
                    }
                }).Start();
                
            }

        }

        #region 模板设置


        /// <summary>
        /// 导入配置信息文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportConfigFile(string file)
        {
            ImportFile = file;
            ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(file);
        }

        /// <summary>
        /// 导入模板文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportTemplate(string file)
        {
            //导入模板
            try
            {
                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = true;
                    Template = new ObservableCollection<Data>(csv.GetRecords<Data>().ToList());
                }
                OnMessageRaised(MessageLevel.Message, "导入模板成功");
            }
            catch (Exception)
            {
                OnMessageRaised(MessageLevel.Message, "导入模板失败");
            }

        }

        /// <summary>
        /// 导出模板文件
        /// </summary>
        /// <param name="file"></param>
        public void ExportTemplate(string file)
        {
            try
            {
                using (var writer = new StreamWriter(file))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(Template);
                }
                OnMessageRaised(MessageLevel.Message, "导出模板成功");
            }
            catch (Exception)
            {
                OnMessageRaised(MessageLevel.Message, "导出模板失败");
            }

        }

        /// <summary>
        /// 设置当前为模板
        /// </summary>
        public void SetTemplate()
        {
            if (ScopeCHACollection != null)
            {
                var tempDataCollection = new ObservableCollection<Data>();

                for (int i = 0; i < ScopeCHACollection.Count / 10; i++)
                {
                    tempDataCollection.Add(ScopeCHACollection[i * 10]);
                }

                //设置当前结果为模板
                Template = new ObservableCollection<Data>(tempDataCollection);
                OnMessageRaised(MessageLevel.Err, "设置模板成功");
            }
            else
            {
                //Template = new ObservableCollection<Data>();
                OnMessageRaised(MessageLevel.Err, "设置模板失败");
            }
        }

        #endregion

        #region 老化测试

        private int backupInterval = 5;

        /// <summary>
        /// 备份间隔
        /// </summary>
        public int BackupInterval
        {
            get { return backupInterval; }
            set { backupInterval = value; }
        }

        private int testTime;

        /// <summary>
        /// 测试次数
        /// </summary>
        public int TestTime
        {
            get { return testTime; }
            set { testTime = value; NotifyOfPropertyChange(() => TestTime); }
        }

        /// <summary>
        /// 当前老化测试次数
        /// </summary>
        private int currentBurnInTime = 0;

        /// <summary>
        /// 老化测试
        /// </summary>
        public void BurnIn()
        {
            currentBurnInTime = 0;
            Start();

            new Thread(() =>
            {


            }).Start();

        }

        #endregion

        #endregion

#endif

        #region 数据模型

        #region 配置参数

        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate { get; set; } = 96 * 1000;

        /// <summary>
        /// 显示率(点/S)
        /// </summary>
        public int DisplayRate { get; set; } = 100;

        /// <summary>
        /// 显示间隔(忽略此间隔中的其他数)
        /// </summary>
        public int DisplayInterval
        {
            get
            {
                return SampleRate / DisplayRate;
            }
        }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public double TimeInterval
        {
            get
            {
                return 1.0 / DisplayRate;
            }
        }

        /// <summary>
        /// 总时间(S)
        /// </summary>
        public int TotalTime { get; set; } = 10;

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount
        {
            get
            {

                return IsShowLastValue ? TotalTime * DisplayRate * 2 : TotalTime * DisplayRate;
            }
        }

        /// <summary>
        /// 显示上一次的数据
        /// </summary>
        public bool IsShowLastValue { get; set; } = false;

        #endregion

        #region 示波器通道

        #region 通道A

        private ObservableCollection<DoublePoint> scopeChACollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<DoublePoint> ScopeCHACollection
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

        #endregion

        #region 通道B

        private ObservableCollection<DoublePoint> scopeChBCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 通道B数据
        /// </summary>
        public ObservableCollection<DoublePoint> ScopeCHBCollection
        {
            get
            {
                return scopeChBCollection;
            }
            set
            {
                scopeChBCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBCollection);
            }
        }

        #endregion

        /// <summary>
        /// 清除示波器数据
        /// </summary>
        public void ClearScopeData()
        {
            ScopeCHACollection = new ObservableCollection<DoublePoint>();
            ScopeCHBCollection = new ObservableCollection<DoublePoint>();
        }

        /// <summary>
        /// 追加示波器数据
        /// </summary>
        /// <param name="channel1">通道1数据</param>
        /// <param name="channel2">通道2数据</param>
        public void AppendScopeData(double[] channel1, double[] channel2, bool isLimit = true)
        {
            ObservableCollection<DoublePoint> scopeCHACollection;
            ObservableCollection<DoublePoint> scopeCHBCollection;

            {
                double time = 0;
                double lastValue = 0;
                var tempCHA = new ObservableCollection<DoublePoint>();
                var tempCHA2 = new ObservableCollection<DoublePoint>();

                //将之前的数据转移到临时变量中
                foreach (var item in ScopeCHACollection)
                {
                    tempCHA.Add(item);
                }

                if (tempCHA.Count > 0)
                {
                    time = tempCHA[tempCHA.Count - 1].Data;
                    lastValue = tempCHA[tempCHA.Count - 1].Value;
                }

                //将数据追加在结尾
                for (int i = 0; i < channel1.Length / DisplayInterval; i++)
                {
                    if (IsShowLastValue)
                    {
                        tempCHA.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                    }
                    lastValue = channel1[i * DisplayInterval];
                    tempCHA.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                }

                //如果数据过长,则截掉前面的
                if ((isLimit) && (tempCHA.Count > TotalCount))
                {
                    int difference = tempCHA.Count - TotalCount;

                    for (int i = difference; i < tempCHA.Count; i++)
                    {
                        tempCHA2.Add(tempCHA[i]);
                    }
                    scopeCHACollection = tempCHA2;
                }
                else
                {
                    scopeCHACollection = tempCHA;
                }
            }

            {
                double time = 0;
                double lastValue = 0;
                var tempCHB = new ObservableCollection<DoublePoint>();
                var tempCHB2 = new ObservableCollection<DoublePoint>();

                //将之前的数据转移到临时变量中
                foreach (var item in ScopeCHBCollection)
                {
                    tempCHB.Add(item);
                }

                if (tempCHB.Count > 0)
                {
                    time = tempCHB[tempCHB.Count - 1].Data;
                    lastValue = tempCHB[tempCHB.Count - 1].Value;
                }

                //将数据追加在结尾
                for (int i = 0; i < channel2.Length / DisplayInterval; i++)
                {
                    if (IsShowLastValue)
                    {
                        tempCHB.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                    }
                    lastValue = channel2[i * DisplayInterval];
                    tempCHB.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                }

                //如果数据过长,则截掉前面的
                if ((isLimit) && (tempCHB.Count > TotalCount))
                {
                    int difference = tempCHB.Count - TotalCount;
                    for (int i = difference; i < tempCHB.Count; i++)
                    {
                        tempCHB2.Add(tempCHB[i]);
                    }
                    scopeCHBCollection = tempCHB2;
                }
                else
                {
                    scopeCHBCollection = tempCHB;
                }
            }

            //最后才将数据赋值给绑定的变量
            ScopeCHACollection = scopeCHACollection;
            ScopeCHBCollection = scopeCHBCollection;
        }

        #endregion

        #region 模板

        private ObservableCollection<DoublePoint> templateCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 模板
        /// </summary>
        public ObservableCollection<DoublePoint> TemplateCollection
        {
            get
            {
                return templateCollection;
            }
            set
            {
                templateCollection = value;
                NotifyOfPropertyChange(() => TemplateCollection);
            }
        }

        /// <summary>
        /// 显示模板
        /// </summary>
        /// <param name="baseTime"></param>
        public void ShowTemplate(ObservableCollection<DoublePoint> template, double baseTime = 0)
        {
            if (template?.Count > 0)
            {
                var tempTemplate = new ObservableCollection<DoublePoint>();
                foreach (var item in template)
                {
                    var temp = new DoublePoint();
                    temp.Data = item.Data + baseTime;
                    temp.Value = item.Value;
                    tempTemplate.Add(temp);
                }
                TemplateCollection = tempTemplate;
            }

        }

        /// <summary>
        /// 显示模板
        /// </summary>
        /// <param name="baseTime"></param>
        public void ShowTemplate(double baseTime = 0)
        {
            ShowTemplate(ExternRecordData?.Template, baseTime);
        }

        /// <summary>
        /// 清除模板
        /// </summary>
        public void ClearTemplate()
        {
            TemplateCollection = new ObservableCollection<DoublePoint>();
        }

        #endregion

        #region 电源模块输出数据

        private ObservableCollection<DoublePoint> powerCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 电源模块输出信息
        /// </summary>
        public ObservableCollection<DoublePoint> PowerCollection
        {
            get
            {
                return powerCollection;
            }
            set
            {
                powerCollection = value;
                NotifyOfPropertyChange(() => PowerCollection);
            }
        }

        /// <summary>
        /// 清除电源数据
        /// </summary>
        public void ClearPowerData()
        {
            PowerCollection = new ObservableCollection<DoublePoint>();
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        public void AppendPowerData(ObservableCollection<DoublePoint> datas)
        {
            //将之前的数据转移到临时变量中
            var tempPower = new ObservableCollection<DoublePoint>();
            foreach (var item in PowerCollection)
            {
                tempPower.Add(item);
            }

            //追加数据到后面
            foreach (var item in datas)
            {
                tempPower.Add(item);
            }

            PowerCollection = tempPower;
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="point"></param>
        public void AppendPowerData(DoublePoint point)
        {
            //将之前的数据转移到临时变量中
            var tempPower = new ObservableCollection<DoublePoint>();
            foreach (var item in PowerCollection)
            {
                tempPower.Add(item);
            }

            //追加数据到后面
            tempPower.Add(point);

            PowerCollection = tempPower;
        }

        #endregion

        #endregion

        #region 应用

        #region 配置文件

        private string configFile;

        /// <summary>
        /// 配置文件
        /// </summary>
        public string ConfigFile
        {
            get { return configFile; }
            set { configFile = value; NotifyOfPropertyChange(() => ConfigFile); }
        }

        private bool isConfigParamValid;

        /// <summary>
        /// 配置参数有效标志
        /// </summary>
        public bool IsConfigParamValid
        {
            get { return isConfigParamValid; }
            set { isConfigParamValid = value; NotifyOfPropertyChange(() => IsConfigParamValid); }
        }

        private bool isTemplateValid;

        /// <summary>
        /// 模板有效标识
        /// </summary>
        public bool IsTemplateValid
        {
            get { return isTemplateValid; }
            set { isTemplateValid = value; NotifyOfPropertyChange(() => IsTemplateValid); }
        }

        /// <summary>
        /// 外部记录的数据
        /// </summary>
        public ExternRecordData ExternRecordData { get; set; } = new ExternRecordData();

        /// <summary>
        /// 鼠标记录软件路径
        /// </summary>
        private string mouseRecordToolPath = "./Tools/MouseRecordTool/MouseRecordTool.exe";

        /// <summary>
        /// 执行配置工具
        /// </summary>
        public void ExecuteConfigTool()
        {
            var fileInfo = new FileInfo(mouseRecordToolPath);

            //如果文件不存在,则直接退出
            if (!fileInfo.Exists)
            {
                return;
            }
            
            //如果没启动鼠标记录软件,则启动软件
            var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(fileInfo.Name));
            if (process.Length == 0)
            {
                Process.Start(fileInfo.FullName);
            }

        }

        /// <summary>
        /// 导入配置文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportConfigFile(string file)
        {
            //如果文件存在,则设置配置参数
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
            {
                ConfigFile = file;
                ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(file);

                IsConfigParamValid = (ExternRecordData != null);
                IsTemplateValid = (ExternRecordData?.Template?.Count > 0);
                CanMeasure = IsConfigParamValid;
            }

        }

        /// <summary>
        /// 保存配置参数
        /// </summary>
        public void SaveConfigParam()
        {
            //保存配置参数
            if (!string.IsNullOrWhiteSpace(ConfigFile))
            {
                JsonSerialization.SerializeObjectToFile(ExternRecordData, ConfigFile);
            }

        }

        /// <summary>
        /// 将当前数据导出为模板
        /// </summary>
        public void ExportTemplate()
        {
            if (IsConfigParamValid)
            {
                //将当前结果设置为模板
                ExternRecordData.Template = new ObservableCollection<DoublePoint>(ScopeCHACollection);
                IsTemplateValid = (ExternRecordData?.Template?.Count > 0);

                //保存到本地
                SaveConfigParam();
            }

        }

        /// <summary>
        /// 预览模板
        /// </summary>
        public void ReviewTemplate()
        {
            //判断是否在测量中
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (IsTemplateValid)
            {
                //清除界面数据
                ClearScopeData();
                ClearPowerData();
                ClearTemplate();

                //显示模板
                ShowTemplate();
            }
        }

        #endregion

        #region 运行

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            //判断是否在测量中
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            //检测硬件有效性
            if (!CanMeasure)
            {
                RunningStatus = "硬件无效/配置文件无效";
                return;
            }

            if (!(ExternRecordData?.RecordVoltages?.Values?.Count > 0))
            {
                RunningStatus = "输出波形无效";
                return;
            }

            //总时间为记录的时间+3S
            TotalTime = (ExternRecordData.RecordVoltages.Values.Count) * ExternRecordData.TimeInterval + 3000;
            
            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.IsCHBEnable = true;
            Scope.CHBScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHBVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.SampleRate = ESampleRate.Sps_96K;

            //设置电源模块直通
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = true;

            RunningStatus = "运行中";

            //开启测量线程
            measureThread = new System.Threading.Thread(() =>
            {
                //设置测量状态
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                //出发开始测量事件
                OnMeasurementStarted();

                Stopwatch totalStopwatch = new Stopwatch();

                //设置示波器采集
                Scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                Scope.ScopeReadDataCompleted += Scope_ScopeReadDataCompleted;

                //清除界面数据
                ClearScopeData();
                ClearPowerData();
                ClearTemplate();

                Power.Voltage = 0;
                Power.IsEnableOutput = true;

                //开始连续采集
                Scope.StartSerialSampple(TotalTime);

                //设置电源模块输出数据
                var collection = new ObservableCollection<DoublePoint>();
                int totalCount = ExternRecordData.RecordVoltages.Values.Count;
                int index = 0;
                double lastVol = 0;
                totalStopwatch.Start();

                foreach (var item in ExternRecordData.RecordVoltages)
                {
                    //等待时间间隔达到,并输出对应的波形
                    while (true)
                    {
                        if (totalStopwatch.Elapsed.TotalMilliseconds >= item.Key)
                        {
                            if (item.Value >= 0)
                            {
                                //设置当前电压
                                Power.Voltage = item.Value / 1000;

                                if (lastVol != item.Value)
                                {
                                    collection.Add(new DoublePoint() { Value = lastVol / 1000, Data = totalStopwatch.Elapsed.TotalMilliseconds / 1000.0 });
                                }
                                collection.Add(new DoublePoint() { Value = item.Value / 1000, Data = totalStopwatch.Elapsed.TotalMilliseconds / 1000.0 });
                                lastVol = item.Value;

                            }

                            index++;
                            break;
                        }
                    }

                    //追加显示电源输出波形
                    AppendPowerData(collection);
                    collection = new ObservableCollection<DoublePoint>();
                }

                //完成电压波形的输出后,触发相关事件
                OnMeasurementCompleted();

            });

            measureThread.Start();

        }

        /// <summary>
        /// 示波器采集完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scope_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {
            double[] globalChannel1;
            double[] globalChannel2;
            double[] currentCHannel1;
            double[] currentCHannel2;

            //获取示波器数据
            e.GetData(out globalChannel1, out globalChannel2, out currentCHannel1, out currentCHannel2);

            //将数据追加在末尾
            AppendScopeData(currentCHannel1, currentCHannel2, false);

            //最后一帧
            if (e.CurrentPacket == e.TotalPacket)
            {
                //显示模板
                ShowTemplate();
            }

        }

        #endregion

        #region 老化

        private int testTime = 5;

        /// <summary>
        /// 测试次数
        /// </summary>
        public int TestTime
        {
            get { return testTime = 5; }
            set { testTime = value; NotifyOfPropertyChange(() => TestTime); }
        }

        private int backupInterval = 20;

        /// <summary>
        /// 备份间隔
        /// </summary>
        public int BackupInterval
        {
            get { return backupInterval; }
            set { backupInterval = value; NotifyOfPropertyChange(() => BackupInterval); }
        }

        #endregion

        #endregion

    }
}
