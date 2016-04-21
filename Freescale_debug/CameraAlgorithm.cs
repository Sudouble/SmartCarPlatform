using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Freescale_debug
{
    internal class CameraAlgorithm
    {
        private readonly List<int> originBuff;
        private  List<List<int>> cameraBuff = new List<List<int>>();

        private string cameraStr = "";
        private int height;
        private readonly string originCameraStr = "";
        private int width;

        public CameraAlgorithm(string orginStr, List<int> recvBuff)
        {
            originCameraStr = orginStr;
            originBuff = recvBuff;
        }

        public void ApartMessage()
        {
            var firstRightIndex = originCameraStr.IndexOf('(');
            var indexEnd = originCameraStr.LastIndexOf('|');
            var cameraMessgageAdd = originCameraStr.Substring(firstRightIndex,
                indexEnd - firstRightIndex);

            var leftIndex = cameraMessgageAdd.IndexOf('(');
            var middleIndex = cameraMessgageAdd.IndexOf('+');
            var rightIndex = cameraMessgageAdd.IndexOf(')');
            int widthCamera =
                Convert.ToInt16(cameraMessgageAdd.Substring(leftIndex + 1, middleIndex - leftIndex - 1));
            int heightCamera =
                Convert.ToInt16(cameraMessgageAdd.Substring(middleIndex + 1,
                    rightIndex - middleIndex - 1));
            var cameraData = cameraMessgageAdd.Substring(rightIndex + 1);

            height = heightCamera;
            width = widthCamera;
            cameraStr = cameraData;

            //找到recvBuff中的相应段
            for (var i = 0; i < originBuff.Count - 12; i++)
            {
                if (originBuff.ElementAt(i) == '#' &&
                    originBuff.ElementAt(i + 1) == '|' &&
                    originBuff.ElementAt(i + 2) == '1' &&
                    originBuff.ElementAt(i + 3) == '|' &&
                    originBuff.ElementAt(i + 5) == '|' &&
                    originBuff.ElementAt(i + 6) == '(' &&
                    originBuff.ElementAt(i + 7) == '8' &&
                    originBuff.ElementAt(i + 8) == '0' &&
                    originBuff.ElementAt(i + 9) == '+' &&
                    originBuff.ElementAt(i + 10) == '6' &&
                    originBuff.ElementAt(i + 11) == '0' &&
                    originBuff.ElementAt(i + 12) == ')')
                {
                    List<int> cameraBuffOneLine = new List<int>();
                    for (var j = i + 13; j < width*height/8 + i + 13; j++)
                    {
                        cameraBuffOneLine.Add(originBuff[j]);
                    }

                    List<int> cameraBuffColumn = new List<int>();
                    for (int j = 0; j < width*height/8; j++)
                    {
                        int currentBuff = cameraBuffOneLine.ElementAt(j);
                        for (int k = 7; k >= 0; k--)
                            cameraBuffColumn.Add(currentBuff>>k & 0x01);
                        if ((j+1)%10 == 0)
                        {
                            cameraBuff.Add(cameraBuffColumn);
                            cameraBuffColumn = new List<int>();
                        }
                    }

                    break;
                }
            }
        }

        public void DrawCameraPicture(PictureBox pictureBoxCamera)
        {
            if (isValidData())
            {
                int amplify = 5;
                var bitmap = new Bitmap(width*amplify, height*amplify);

                var bitmapData =
                    bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadWrite, bitmap.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
                var byteCount = bitmapData.Stride*bitmap.Height;
                var pixels = new byte[byteCount];
                var ptrFirstPixel = bitmapData.Scan0;
                Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width*bytesPerPixel;

                for (var y = 0; y < heightInPixels; y++)
                {
                    var currentLine = y*bitmapData.Stride;

                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int grey = cameraBuff.ElementAt(y/amplify).ElementAt(x/4/amplify);

                        // calculate new pixel value
                        pixels[currentLine + x] = (byte)(grey == 1 ? 0 : 255);
                        pixels[currentLine + x + 1] = (byte)(grey == 1 ? 0 : 255);
                        pixels[currentLine + x + 2] = (byte)(grey == 1 ? 0 : 255);
                        pixels[currentLine + x + 3] = 255;
                    }
                }
                // copy modified bytes back
                Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
                bitmap.UnlockBits(bitmapData);

                pictureBoxCamera.Image = bitmap;
            }
        }

        private bool isValidData()
        {
            if (cameraBuff.Count*cameraBuff.First().Count == width*height)
                return true;
            return false;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetWidth()
        {
            return width;
        }

        public string GetCameraStr()
        {
            return cameraStr;
        }
    }
}