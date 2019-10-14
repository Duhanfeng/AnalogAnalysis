using AnalogDataAnalysisWpf.LiveData;
using CsvHelper;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DataAnalysis.Analysis;

namespace DataAnalysisDamoWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LiveDataViewModel1 = LiveDataView1.DataContext as LiveDataViewModel;
            LiveDataViewModel2 = LiveDataView2.DataContext as LiveDataViewModel;
        }

        public LiveDataViewModel LiveDataViewModel1 { get; set; }
        public LiveDataViewModel LiveDataViewModel2 { get; set; }

        public double[] FilterData;

        /// <summary>
        /// 数据采样间隔
        /// </summary>
        public int SampleInterval = 200;


        private class CHData
        {
            public string CH { get; set; }
        }

        private static void GetData(string file, out double[] Data)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{nameof(file)}:{file} not found!");
            }

            try
            {
                using (var reader = new StreamReader(file))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Configuration.HasHeaderRecord = false;
                        csv.Configuration.Comment = ';';
                        csv.Configuration.AllowComments = true;
                        //csv.Configuration.RegisterClassMap<FooMap>();
                        var records = csv.GetRecords<CHData>();
                        //deviceData = records.ToArray();
                        var str = records.ToList().ConvertAll(x => x.CH);

                        Data = (
                        from val in str
                        where !val.Contains("CH")
                        select val).ToList().ConvertAll(x => double.Parse(x)).ToArray();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        //800MS
        private static void GetData2(string file, out double[] Data)
        {
            Data = new double[0];

            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{nameof(file)}:{file} not found!");
            }

            try
            {
                List<string> datas2 = new List<string>();

                using (var reader = new StreamReader(file))
                {

                    while (true)
                    {
                        var row = reader.ReadLine(); // row 是个字符串
                        if (row == null)
                        {
                            break;
                        }
                        datas2.Add(row);
                    }

                    Data = (
                    from val in datas2
                    where !val.Contains("CH")
                    select val).ToList().ConvertAll(x => double.Parse(x)).ToArray();

                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        //600MS
        private static void GetData3(string file, out double[] Data)
        {
            Data = new double[0];

            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{nameof(file)}:{file} not found!");
            }

            try
            {
                using (var reader = new StreamReader(file))
                {
                    var allString = reader.ReadToEnd();
                    var d1 = allString.Split('\n').ToList();
                    var d2 = d1.GetRange(d1.Count / 2 + 2, d1.Count / 2 - 4);
                    Data = d2.ConvertAll(x => double.Parse(x)).ToArray();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadDataButton_Click(object sender, RoutedEventArgs e)
        {
            //选择CSV文件
            DirectoryInfo directoryInfo = new DirectoryInfo("./data");
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "csv文件|*.csv",
                InitialDirectory = directoryInfo.FullName,
            };

            if (ofd.ShowDialog() != true)
            {
                return;
            }

            //从CSV中获取数据
            double[] deviceData;
            GetData(ofd.FileName, out deviceData);
            //using (var reader = new StreamReader(ofd.FileName))
            //{
            //    using (var csv = new CsvReader(reader))
            //    {
            //        csv.Configuration.HasHeaderRecord = false;
            //        var records = csv.GetRecords<double>();
            //        deviceData = records.ToArray();
            //    }
            //}
            int sampleRatio = 8 * 1000;

            //数据滤波
            Analysis.MeanFilter(deviceData, 15, out FilterData);

            //显示数据
            var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            for (int i = 0; i < FilterData.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = FilterData[i * SampleInterval], Value = i * 1000.0 / sampleRatio * SampleInterval });
            }
            LiveDataViewModel1.Collection = collection;

        }

        /// <summary>
        /// 微分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DerivativeDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterData?.Length > 0)
            {
                int K = 20;
                int sampleRatio = 8 * 1000;
                int sampleCount = sampleRatio / K;
                Analysis.Derivative(FilterData, sampleCount, out var derivativeData);

                ObservableCollection<Data> collection = new ObservableCollection<Data>();

                for (int i = 0; i < derivativeData.Length; i++)
                {
                    collection.Add(new Data() { Value1 = derivativeData[i] + 3.0, Value = i * sampleCount });
                }

                LiveDataViewModel1.Collection3 = collection;
            }
                
        }

        public List<int> EdgeIndexs = new List<int>();
        public DigitEdgeType DigitEdgeType = DigitEdgeType.CriticalLevel;

        /// <summary>
        /// 查找边沿点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterData?.Length > 0)
            {
                LiveDataViewModel2.Collection = new ObservableCollection<Data>(LiveDataViewModel1.Collection);

                Analysis.FindEdgeByThreshold(FilterData, 2, 3, out EdgeIndexs, out DigitEdgeType);

                if ((DigitEdgeType == DigitEdgeType.FirstFillingEdge) || (DigitEdgeType == DigitEdgeType.FirstRisingEdge))
                {
                    ObservableCollection<Data> collection = new ObservableCollection<Data>();

                    foreach (var item in EdgeIndexs)
                    {
                        collection.Add(new Data() { Value1 = LiveDataViewModel2.Collection[item / SampleInterval].Value1, Value = item });

                    }
                    LiveDataViewModel2.Collection2 = collection;
                }

            }


        }

#if false
        
        private double[] deviceData = new double[0];
        private double[] derivativeData = new double[0];

        /// <summary>
        /// 滤波后的数据
        /// </summary>
        private IntPtr FilterData = IntPtr.Zero;
        private int FilterDataCount = 0;

        /// <summary>
        /// 数据采样数量
        /// </summary>
        private int sampleCount = 200;

        private void ReadDataButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("./data");
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "csv文件|*.csv",
                InitialDirectory = directoryInfo.FullName,
            };

            if (ofd.ShowDialog() != true)
            {
                return;
            }

            //从CSV中获取数据
            using (var reader = new StreamReader(ofd.FileName))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<double>();
                deviceData = records.ToArray();
            }

            var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            for (int i = 0; i < deviceData.Length / sampleCount; i++)
            {
                collection.Add(new Data() { Value1 = deviceData[i * sampleCount], Value = i * sampleCount });
            }

            LiveDataViewModel1.Collection = collection;

            if (FilterData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(FilterData);
                FilterData = IntPtr.Zero;
                FilterDataCount = 0;
            }

            //数据滤波
            IntPtr source = IntPtr.Zero;
            source = Marshal.AllocHGlobal(sizeof(double) * deviceData.Length);
            FilterData = Marshal.AllocHGlobal(sizeof(double) * deviceData.Length);
            FilterDataCount = deviceData.Length;

            //将原始数据从数组转成指针
            Marshal.Copy(deviceData, 0, source, deviceData.Length);

            //均值滤波,得到滤波后的数据FilterData
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            DataFilter.MeanFilter(source, FilterData, deviceData.Length, 15);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);

            var collection2 = new System.Collections.ObjectModel.ObservableCollection<Data>();
            unsafe
            {
                double* destinationIntPtr = (double*)FilterData.ToPointer();

                for (int i = 0; i < deviceData.Length / sampleCount; i++)
                {
                    collection2.Add(new Data() { Value1 = destinationIntPtr[i * sampleCount], Value = i * sampleCount });
                }
            }

            if (source != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(source);
                source = IntPtr.Zero;
            }

            LiveDataViewModel2.Collection = collection2;
            LiveDataViewModel2.Collection2 = new ObservableCollection<Data>();
        }

        private void DerivativeDataButton_Click(object sender, RoutedEventArgs e)
        {
            //微分求导
            Analysis.Derivative(deviceData, out derivativeData);

            var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
            for (int i = 1; i < derivativeData.Length / sampleCount; i++)
            {
                collection.Add(new Data() { Value1 = derivativeData[i * sampleCount], Value = i * sampleCount });
            }

            LiveDataViewModel2.Collection = collection;

        }

        private void FindEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterData != IntPtr.Zero)
            {
                //IntPtr topLocation = Marshal.AllocHGlobal(sizeof(double) * FilterDataCount);
                //IntPtr buttomLocation = Marshal.AllocHGlobal(sizeof(double) * FilterDataCount);

                IntPtr topLocation = IntPtr.Zero;
                IntPtr buttomLocation = IntPtr.Zero;
                int risingCount = 0;
                int fallingCount = 0;

                DataFilter.FindEdge(FilterData, FilterDataCount, 1.5, 3, out topLocation, out buttomLocation, ref risingCount, ref fallingCount);

                unsafe
                {
                    ObservableCollection<Data> collection = new ObservableCollection<Data>();
                    int * topLocationPtr = (int*)topLocation;
                    //上升沿位置
                    for (int i = 0; i < risingCount; i++)
                    {
                        Console.WriteLine(topLocationPtr[i]);
                        collection.Add(new Data() { Value1 = LiveDataViewModel2.Collection[topLocationPtr[i] / sampleCount].Value1, Value = topLocationPtr[i] });
                    }

                    int* buttomLocationPtr = (int*)buttomLocation;
                    //上升沿位置
                    for (int i = 0; i < fallingCount; i++)
                    {
                        Console.WriteLine(buttomLocationPtr[i]);
                        collection.Add(new Data() { Value1 = LiveDataViewModel2.Collection[buttomLocationPtr[i] / sampleCount].Value1, Value = buttomLocationPtr[i] });
                    }

                    LiveDataViewModel2.Collection2 = collection;
                }

                //释放内存
                DataFilter.FreeIntPtr(topLocation);
                DataFilter.FreeIntPtr(buttomLocation);

            }
        }

#endif




    }
}
