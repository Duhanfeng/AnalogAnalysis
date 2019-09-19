using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering;

namespace DataAnalysis
{
    public class Analysis
    {
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

            for (int i = 0; i < source.Length / 100; i++)
            {
                destination[i] = f2(i * 100);
            }

        }

        /// <summary>
        /// 中值滤波
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="destination">结果数据</param>
        /// <param name="kernelSize">内核大小</param>
        public static void MedianFilter(double[] source, out double[] destination, int kernelSize)
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
        public static void MeanFilter(double[] source, out double[] destination, int kernelSize)
        {
            var filter = new MathNet.Filtering.Median.OnlineMedianFilter(kernelSize);
            //MathNet.Filtering.OnlineFilter

            //destination = filter.ProcessSamples(source);

            throw new InvalidOperationException();
        }


    }
}
