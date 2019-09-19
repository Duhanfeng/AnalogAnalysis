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
    }
}
