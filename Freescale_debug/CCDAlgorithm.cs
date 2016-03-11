using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freescale_debug
{
    class CCDAlgorithm
    {
        private string testCCD_Image()
        {
            var randInts = new int[128];
            var result = "";
            var rand = new Random();
            for (var i = 0; i < 128; i++)
            {
                randInts[i] = rand.Next(1, 255);
                result += Convert.ToChar(randInts[i]).ToString();
                //result += Convert.ToChar(0).ToString();
            }
            return result;
        }
        private int Average_ccd(List<int> p)
        {
            var sum = p.Select((t, i) => p.ElementAt(i)).Sum();

            var thresold = sum / p.Count;

            if (thresold > 230)
                thresold = 100;
            else if (thresold < 150)
                thresold = 200;
            else
            {
                thresold = Otsu(p);
            }

            return thresold;
        }
        private int Otsu(List<int> p)
        {
            //处理全是黑色时候的情况

            var threshold = 0;
            int g = 0, max = 0;
            int total = 0, total_low = 0;
            int u0 = 0, u1 = 0, count = 0, cnt = 0;
            var tmpData = new int[256];
            var j = 0;
            for (j = 5; j <= 122; j++)
            {
                tmpData[p.ElementAt(j)]++;
                total += p.ElementAt(j);
            }
            for (j = 0; j <= 254; j++)
            {
                cnt = tmpData[j];
                if (cnt == 0) continue; // 优化加速
                count += tmpData[j];
                total_low += cnt * j;
                u0 = total_low / count;
                if (count >= 118) break; // 优化加速 122 - 5+1
                u1 = (total - total_low) / (118 - count);
                g = ((u0 - u1) * (u0 - u1)) * ((count * (118 - count))) / 16384;
                if (g > max)
                {
                    max = g;
                    threshold = j;
                }
            }

            return threshold;
        }
        private string CCD_FindBoard(List<int> p)
        {
            var black = "Border";
            for (var i = 0; i < p.Count - 1; i++)
            {
                if ((p.ElementAt(i) == 255 && p.ElementAt(i + 1) == 0) ||
                    (p.ElementAt(i) == 0 && p.ElementAt(i + 1) == 255))
                {
                    black += i + " ";
                }
            }
            return black;
        }
    }
}
