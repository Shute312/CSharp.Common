using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            DateTime time = DateTime.Now;
            object obj = time;
            time.ToString("yyyy-MM-dd");
        }
    }
}
