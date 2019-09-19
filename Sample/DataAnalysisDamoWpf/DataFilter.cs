using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalysisDamoWpf
{
    public static class DataFilter
    {
        [DllImport("DataFilter.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MeanFilter(IntPtr source, IntPtr destination, int length, int kernelSize);

        /// <summary>
        /// 查找边沿
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="length">数据长度</param>
        /// <param name="minThreshold">最小阈值</param>
        /// <param name="maxThreshold">最大阈值</param>
        /// <param name="topLocation">顶点位置(数据长度与数据源保持一致)</param>
        /// <param name="buttomLocation">底部位置(数据长度与数据源保持一致)</param>
        /// <param name="risingCount">实际上升沿数量</param>
        /// <param name="fallingCount">实际下降沿数量</param>
        /// <returns></returns>
        [DllImport("DataFilter.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FindEdge(IntPtr source, int length, double minThreshold, double maxThreshold, IntPtr topLocation, IntPtr buttomLocation, ref int risingCount, ref int fallingCount);

    }
}
