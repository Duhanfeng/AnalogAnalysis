using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace DataAnalysis
{

    public enum DigitEdgeType
    {
        /// <summary>
        /// 临界电平(小于最大阈值而大于最小阈值)
        /// </summary>
        CriticalLevel = 0,

        /// <summary>
        /// 高电平
        /// </summary>
        HeightLevel,

        /// <summary>
        /// 低电平
        /// </summary>
        LowLevel,

        /// <summary>
        /// 第一个边沿为上升沿的边沿
        /// </summary>
        FirstRisingEdge,

        /// <summary>
        /// 第一个边沿为下降沿沿的边沿
        /// </summary>
        FirstFillingEdge,
    }

    public class Analysis
    {
        #region 求导/滤波

        /// <summary>
        /// 数据微分求导
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="destination">求导后的数据</param>
        public static void Derivative(double[] source, out double[] destination)
        {
            destination = new double[0];

            double[] tempX = new double[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                tempX[i] = i;
            }

            var interpolate = MathNet.Numerics.Interpolate.Linear(tempX, source);

            Func<double, double> f1 = (double x) =>
            {
                if (x < source.Length)
                {
                    return interpolate.Interpolate(x);
                }
                return 0;
            };

            var f2 = MathNet.Numerics.Differentiate.DerivativeFunc(f1, 1);
            destination = new double[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = f2(i);
            }

        }

        /// <summary>
        /// 数据微分求导
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="sampleInterval">采样间隔</param>
        /// <param name="destination">求导后的数据</param>
        public static void Derivative(double[] source, int sampleInterval, out double[] destination)
        {
            double[] source2 = new double[source.Length / sampleInterval];

            for (int i = 0; i < source.Length / sampleInterval; i++)
            {
                source2[i] = source[i * sampleInterval];
            }

            Derivative(source2, out destination);

        }

        /// <summary>
        /// 中值滤波
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="destination">结果数据</param>
        /// <param name="kernelSize">内核大小</param>
        public static void MedianFilter(double[] source, int kernelSize, out double[] destination)
        {
            var filter = new MathNet.Filtering.Median.OnlineMedianFilter(kernelSize);

            destination = filter.ProcessSamples(source);

        }

        /// <summary>
        /// 均值滤波
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="destination">结果数据</param>
        /// <param name="kernelSize">内核大小</param>
        public static void MeanFilter(double[] source, int kernelSize, out double[] destination)
        {
            destination = new double[0];
            if (source == null || source.Length == 0)
            {
                throw new ArgumentException($"{nameof(source)}无效");
            }

            if (kernelSize < 3)
            {
                throw new ArgumentException($"{nameof(kernelSize)}必须大于等于3(当前:{kernelSize})");
            }

            int length = source.Length;

            if (length < kernelSize)
            {
                throw new ArgumentException($"数据长度({length})必须大于等于{nameof(kernelSize)}({kernelSize})");
            }

            destination = new double[length];

            int startOffset = 0;
            int endOffset = 0;

            //偶数
            if ((kernelSize % 2) == 0)
            {
                startOffset = kernelSize / 2 - 1;
                endOffset = kernelSize / 2;
            }
            else
            {
                startOffset = endOffset = kernelSize / 2;
            }

            double sum = 0;

            for (int i = 0; i < startOffset; i++)
            {
                destination[i] = source[i];
            }

            for (int i = (length - endOffset); i < length; i++)
            {
                destination[i] = source[i];
            }

            for (int i = startOffset; i < (length - endOffset); i++)
            {
                sum = 0;
                for (int j = 0; j < kernelSize; j++)
                {
                    sum += source[i - startOffset + j];
                }
                destination[i] = sum / kernelSize;
            }

        }

        #endregion

        #region 脉冲解析

        /// <summary>
        /// 通过阈值查找边沿
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="minThreshold">最小阈值</param>
        /// <param name="maxThreshold">最大阈值</param>
        /// <param name="edgeIndexs">边沿索引</param>
        /// <param name="isRisingFirst">首边沿为上升沿标志位</param>
        public static void FindEdgeByThreshold(double[] source, double minThreshold, double maxThreshold, out List<int> edgeIndexs, out DigitEdgeType digitEdgeType)
        {
            if (source == null || source.Length == 0)
            {
                throw new ArgumentException($"{nameof(source)}无效");
            }

            if (minThreshold > maxThreshold)
            {
                throw new ArgumentException($"{nameof(minThreshold)}({minThreshold})不得大于{nameof(maxThreshold)}({maxThreshold})");
            }

            edgeIndexs = new List<int>();
            digitEdgeType = DigitEdgeType.CriticalLevel;

            //当前位置,0-最大阈值与最小阈值之间,-1小于最小阈值,1-大于最大阈值
            int currentLocation = 0;

            if (source[0] >= maxThreshold)
            {
                digitEdgeType = DigitEdgeType.HeightLevel;
                currentLocation = 1;
            }
            else if (source[0] <= minThreshold)
            {
                digitEdgeType = DigitEdgeType.LowLevel;
                currentLocation = -1;
            }
            else
            {
                digitEdgeType = DigitEdgeType.CriticalLevel;
                currentLocation = 0;
            }

            for (int i = 1; i < source.Length; i++)
            {
                switch (currentLocation)
                {
                    case 1:
                        if (digitEdgeType == DigitEdgeType.CriticalLevel)
                        {
                            digitEdgeType = DigitEdgeType.HeightLevel;
                        }

                        //当前在顶部,查找下降沿
                        if (source[i] <= minThreshold)
                        {
                            edgeIndexs.Add(i);
                            currentLocation = -1;

                            if (digitEdgeType == DigitEdgeType.HeightLevel)
                            {
                                digitEdgeType = DigitEdgeType.FirstFillingEdge;
                            }
                        }
                        break;
                    case -1:
                        if (digitEdgeType == DigitEdgeType.CriticalLevel)
                        {
                            digitEdgeType = DigitEdgeType.LowLevel;
                        }

                        //当前在底部,查找上升沿
                        if (source[i] >= maxThreshold)
                        {
                            edgeIndexs.Add(i);
                            currentLocation = 1;

                            if (digitEdgeType == DigitEdgeType.LowLevel)
                            {
                                digitEdgeType = DigitEdgeType.FirstRisingEdge;
                            }
                        }
                        break;
                    default:
                        //查找下降沿
                        if (source[i] <= minThreshold)
                        {
                            currentLocation = -1;
                        }

                        //查找上升沿
                        if (source[i] >= maxThreshold)
                        {
                            currentLocation = 1;
                        }
                        break;
                }
            }

        }

        /// <summary>
        /// 分析脉冲数据
        /// </summary>
        /// <param name="edgeIndexs">边沿索引</param>
        /// <param name="isRisingFirst">首边沿为上升沿标志位</param>
        /// <param name="sampleRate"></param>
        /// <param name="frequencies"></param>
        /// <param name="dutyRatios"></param>
        public static void AnalysePulseData(List<int> edgeIndexs, DigitEdgeType digitEdgeType, int sampleRate, out List<double> frequencies, out List<double> dutyRatios)
        {
            frequencies = new List<double>();
            dutyRatios = new List<double>();

            if ((edgeIndexs == null) || 
                (edgeIndexs.Count == 0) ||
                (digitEdgeType == DigitEdgeType.CriticalLevel) ||
                (digitEdgeType == DigitEdgeType.HeightLevel) || 
                (digitEdgeType == DigitEdgeType.LowLevel))
            {
                return;
            }

            int firstRisingIndex = (digitEdgeType == DigitEdgeType.FirstRisingEdge) ? 0 : 1;
            int tempCount1 = 0;
            int tempCount2 = 0;

            //当前脉冲的频率及占空比分析
            while (firstRisingIndex + 2 < edgeIndexs.Count)
            {
                tempCount1 = edgeIndexs[firstRisingIndex + 2] - edgeIndexs[firstRisingIndex];
                tempCount2 = edgeIndexs[firstRisingIndex + 1] - edgeIndexs[firstRisingIndex];
                frequencies.Add((double)sampleRate / (double)tempCount1);
                dutyRatios.Add((double)(tempCount2) / (double)(tempCount1));
                firstRisingIndex += 2;
            }

        }

        /// <summary>
        /// 检测频率
        /// </summary>
        /// <param name="frequencies">频率列表</param>
        /// <param name="minFrequency">最小频率</param>
        /// <param name="maxFrequency">最大频率</param>
        /// <param name="ignoreCount">忽略数量</param>
        /// <returns>检测结果</returns>
        public static bool CheckFrequency(List<double> frequencies, double minFrequency, double maxFrequency, int ignoreCount = 0)
        {
            if (frequencies == null || frequencies.Count == 0)
            {
                throw new ArgumentException($"{nameof(frequencies)}无效");
            }

            if (minFrequency > maxFrequency)
            {
                throw new ArgumentException($"{nameof(minFrequency)}({minFrequency})不得大于{nameof(maxFrequency)}({maxFrequency})");
            }

            int currentError = 0;

            foreach (var item in frequencies)
            {
                if ((item < minFrequency) || (item > maxFrequency))
                {
                    currentError++;
                }
            }

            return (currentError <= ignoreCount);
        }

        /// <summary>
        /// 检测频率
        /// </summary>
        /// <param name="frequencies">频率列表</param>
        /// <param name="minFrequency">最小频率</param>
        /// <param name="maxFrequency">最大频率</param>
        /// <param name="ignoreCount">忽略数量</param>
        /// <returns>检测结果</returns>
        public static bool CheckDutyRatio(List<double> dutyRatios, double minDutyRatio, double maxDutyRatio, int ignoreCount = 0)
        {
            if (dutyRatios == null || dutyRatios.Count == 0)
            {
                throw new ArgumentException($"{nameof(dutyRatios)}无效");
            }

            if (minDutyRatio > maxDutyRatio)
            {
                throw new ArgumentException($"{nameof(minDutyRatio)}({minDutyRatio})不得大于{nameof(maxDutyRatio)}({maxDutyRatio})");
            }

            int currentError = 0;

            foreach (var item in dutyRatios)
            {
                if ((item < minDutyRatio) || (item > maxDutyRatio))
                {
                    currentError++;
                }
            }

            return (currentError <= ignoreCount);
        }

        /// <summary>
        /// 只有单个上升沿
        /// </summary>
        /// <param name="edgeIndexs">边沿索引</param>
        /// <param name="isRisingFirst">首边沿为上升沿标志位</param>
        /// <returns>结果</returns>
        public static bool IsOnlyRisingEdge(List<int> edgeIndexs, DigitEdgeType digitEdgeType)
        {
            if (edgeIndexs == null || edgeIndexs.Count == 0)
            {
                throw new ArgumentException($"{nameof(edgeIndexs)}无效");
            }

            if ((digitEdgeType == DigitEdgeType.FirstRisingEdge) && (edgeIndexs.Count == 1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 只有单个下降沿
        /// </summary>
        /// <param name="edgeIndexs">边沿索引</param>
        /// <param name="isRisingFirst">首边沿为上升沿标志位</param>
        /// <returns>结果</returns>
        public static bool IsOnlyFillingEdge(List<int> edgeIndexs, DigitEdgeType digitEdgeType)
        {
            if (edgeIndexs == null || edgeIndexs.Count == 0)
            {
                throw new ArgumentException($"{nameof(edgeIndexs)}无效");
            }

            if ((digitEdgeType == DigitEdgeType.FirstFillingEdge) && (edgeIndexs.Count == 1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 测量上升沿边沿时间
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="minThreshold">最小阈值</param>
        /// <param name="maxThreshold">最大阈值</param>
        /// <param name="time">边沿时间</param>
        /// <returns>执行结果</returns>
        public static bool MeasureRisingEdgeTime(double[] source, double minThreshold, double maxThreshold, out double time)
        {
            time = -1;

            if (source == null || source.Length == 0)
            {
                throw new ArgumentException($"{nameof(source)}无效");
            }

            if (minThreshold > maxThreshold)
            {
                throw new ArgumentException($"{nameof(minThreshold)}({minThreshold})不得大于{nameof(maxThreshold)}({maxThreshold})");
            }


            return false;
        }

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static double Median(double[] source)
        {
            return ArrayStatistics.MedianInplace(source);
        }

        #endregion
    }


}
