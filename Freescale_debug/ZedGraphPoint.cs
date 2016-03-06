using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace Freescale_debug
{
    class ZedGrpahName
    {
        public bool isSingleWindowShowed { get; set; }
        public double ValueZed { get; set; }
        public int x { get; set; }

        public PointPairList listZed = new PointPairList();
        public PointPairList ListZed
        {
            get { return listZed;}
            set { ListZed = value; }
        }
        public ZedGraphPoint zedPoint { get; set; }
    }
    class ZedGraphPoint
    {
        public List<double> ZedListX = new List<double>();
        public List<double> zedListX 
        {
            get { return ZedListX; }
            set { ZedListX = value; }
        }
        public List<double> ZedListY = new List<double>();
        public List<double> zedListY
        {
            get { return ZedListY; }
            set { ZedListY = value; }
        }
    }
}
