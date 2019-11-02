using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class BurnInTestCompletedEventArgs : EventArgs
    {
        public BurnInTestCompletedEventArgs()
        {
            IsSuccess = false;
        }

        public BurnInTestCompletedEventArgs(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

    }
}
