using System.Collections.Generic;
using ZedGraph;

namespace Freescale_debug
{
    internal class ZedGrpahName
    {
        public PointPairList listZed = new PointPairList();
        public bool IsSingleWindowShowed { get; set; }
        public double ValueZed { get; set; }
        public int x { get; set; }

        public PointPairList ListZed
        {
            get { return listZed; }
            set { ListZed = value; }
        }

        public ZedGraphPoint zedPoint { get; set; }
    }

    internal class ZedGraphPoint
    {
        public List<double> ZedListX = new List<double>();
        public List<double> ZedListY = new List<double>();

        public List<double> zedListX
        {
            get { return ZedListX; }
            set { ZedListX = value; }
        }

        public List<double> zedListY
        {
            get { return ZedListY; }
            set { ZedListY = value; }
        }
    }
}