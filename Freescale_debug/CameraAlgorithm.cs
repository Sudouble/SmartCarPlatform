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
    class CameraAlgorithm
    {
        private string originCameraStr = "";
        private string cameraStr = "";
        private int width = 0;
        private int height = 0;
        public CameraAlgorithm(string orginStr)
        {
            originCameraStr = orginStr;
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
        }

        public void DrawCameraPicture(PictureBox pictureBoxCamera)
        {
            if (isValidData())
            {
                var bitmap = new Bitmap(width, height);

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
                        int grey = Convert.ToInt16(cameraStr.ElementAt(Convert.ToInt16(y + x / 4)));

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

                pictureBoxCamera.Image = bitmap;
            }
        }

        private bool isValidData()
        {
            if (cameraStr.Length == width*height)
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
