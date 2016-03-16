using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Freescale_debug
{
    class CCDAlgorithm
    {
        private string originCCDStr = "";
        private List<int> originCCDBuff = new List<int>();

        private string ccdStr = "";
        private List<int> CCDBuff = new List<int>();
        private int ccdLength = 0;
        private int height = 0;

        public CCDAlgorithm(string orginStr, List<int> recvBuff)
        {
            originCCDStr = orginStr;
            originCCDBuff = recvBuff;
        }

        public void ApartMessage()
        {
            ApartMessageList();
            ApartMessageString();
        }

        private void ApartMessageList()
        {
            //找到recvBuff中的相应段
            for (var i = 0; i < originCCDBuff.Count - 6; i++)
            {
                if (originCCDBuff.ElementAt(i) == '#' &&
                    originCCDBuff.ElementAt(i + 1) == '|' &&
                    originCCDBuff.ElementAt(i + 2) == '2' &&
                    originCCDBuff.ElementAt(i + 3) == '|' &&
                    originCCDBuff.ElementAt(i + 5) == '|' &&
                    originCCDBuff.ElementAt(i + 6) == '(')
                {
                    for (var j = i + 11; j < ccdStr.Length + i + 11; j++)
                    {
                        CCDBuff.Add(originCCDBuff[j]);
                    }
                    break;
                }
            }
        }

        private void ApartMessageString()
        {
            var firstRightIndex = originCCDStr.IndexOf('(');
            var indexEnd = originCCDStr.LastIndexOf('|');
            var ccdMessgageAdd = originCCDStr.Substring(firstRightIndex,
                indexEnd - firstRightIndex);

            var leftIndex = ccdMessgageAdd.IndexOf('(');
            var rightIndex = ccdMessgageAdd.IndexOf(')');
            var length = Convert.ToInt16(ccdMessgageAdd.Substring(leftIndex + 1, rightIndex - leftIndex - 1));
            var ccdData = ccdMessgageAdd.Substring(rightIndex + 1);

            ccdStr = ccdData;
            ccdLength = length;
        }
        public int GetCCDLength()
        {
            return ccdLength;
        }

        public string GetCCDStr()
        {
            return ccdStr;
        }
        public List<int> GetCCDGreyValue()
        {
            return CCDBuff;
        }

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

        #region 6.CCD图像
        public void DrawCCDPicture(PictureBox pictureBoxImage)
        {
            if (ccdStr.Length != ccdLength)
                return;

            var bitmap = new Bitmap(ccdStr.Length, pictureBoxImage.Height);
            var bitmapData =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width * bytesPerPixel;

            for (var y = 0; y < heightInPixels; y++)
            {
                var currentLine = y * bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var grey = CCDBuff.ElementAt(Convert.ToInt16(x / 4));

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte)grey;
                    pixels[currentLine + x + 1] = (byte)grey;
                    pixels[currentLine + x + 2] = (byte)grey;
                    pixels[currentLine + x + 3] = 255;
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            pictureBoxImage.Image = bitmap;
        }

        public void DrawCCDPath(Bitmap bitmap, PictureBox pictureBoxImage)
        {
            if (ccdStr.Length != ccdLength)
                return;

            var bitmapData =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            var byteCount = bitmapData.Stride * bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width * bytesPerPixel;

            for (var y = 1; y < heightInPixels; y++)
            {
                var currentLine = y * bitmapData.Stride;
                var formerLine = (y - 1) * bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    pixels[formerLine + x] = (byte)oldBlue;
                    pixels[formerLine + x + 1] = (byte)oldGreen;
                    pixels[formerLine + x + 2] = (byte)oldRed;
                    pixels[formerLine + x + 3] = 255;
                }
            }

            for (var y = heightInPixels - 1; y < heightInPixels; y++)
            {
                var currentLine = y * bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var leng = CCDBuff.Count;
                    var grey = CCDBuff.ElementAt(Convert.ToInt16(x / 4));

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte)grey;
                    pixels[currentLine + x + 1] = (byte)grey;
                    pixels[currentLine + x + 2] = (byte)grey;
                    pixels[currentLine + x + 3] = 255;
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            pictureBoxImage.Image = bitmap;
        }
        #endregion
    }
}
