using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Freescale_debug
{
    public static class SugarTransfer
    {
        private static Regex re = new Regex("/d+", RegexOptions.Compiled);
        static public Int32 toInt32(this string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return -1;
            }
        }


    }
}
