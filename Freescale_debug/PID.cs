namespace Freescale_debug
{
    internal class FourWheelPID
    {
        public PID motorPID { get; set; }
        public PID steerPID { get; set; }
    }

    internal class BalancePID
    {
        public PID speedPID { get; set; }
        public PID directionPID { get; set; }
        public PID standPID { get; set; }
    }

    internal class PID
    {
        public int P { get; set; }
        public int I { get; set; }
        public int D { get; set; }
    }
}