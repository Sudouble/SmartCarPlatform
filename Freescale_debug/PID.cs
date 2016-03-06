using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freescale_debug
{
    class FourWheelPID
    {
        public PID motorPID { get; set; }
        public PID steerPID { get; set; }
    }
    class  BalancePID
    {
        public PID speedPID { get; set; }
        public PID directionPID { get; set; }
        public PID standPID { get; set; }
    }

    class PID
    {
        public int P { get; set; }
        public int I { get; set; }
        public int D { get; set; }
    }
}
