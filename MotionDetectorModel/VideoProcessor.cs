using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.VideoSurveillance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MotionDetectorModel
{
    public class VideoProcessor
    {
        private VideoCapture _capture;
        private MotionHistory _motionHistory;
        private BackgroundSubtractorMOG2 _forgroundDetector;
        private Mat _segMask = new Mat();
        private Mat _forgroundMask = new Mat();
        public event Action<BitmapSource> ImageCaptured;

        public void Capture(string dataPath)
        {
            if (_capture == null)
            {
                try
                {
                    _capture = new VideoCapture(dataPath);
                }
                catch (NullReferenceException excpt)
                {
                    throw new NullReferenceException("Rossz", excpt);
                }
            }

            if (_capture != null) //if camera capture has been successfully created
            {
                _motionHistory = new MotionHistory(
                    1.0, //in second, the duration of motion history you wants to keep
                    0.05, //in second, maxDelta for cvCalcMotionGradient
                    0.5); //in second, minDelta for cvCalcMotionGradient

                _capture.ImageGrabbed += ProcessFrame;
                _capture.Start();
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            Mat image = new Mat();

            _capture.Retrieve(image);// az imaget feltölti a grabbed frammel
            if (_forgroundDetector == null)
            {
                _forgroundDetector = new BackgroundSubtractorMOG2(); // létrehozza egyszer a BGR maskot
            }
            _forgroundDetector.Apply(image, _forgroundMask); // updateli az image bemenő byte arrayal, a foreground maskot, ami a végleges kép
            //update the motion history
            _motionHistory.Update(_forgroundMask); // a motion historyt updateli

            Rectangle[] rects;
            using (VectorOfRect boundingRect = new VectorOfRect())
            {
                _motionHistory.GetMotionComponents(_segMask, boundingRect);
                rects = boundingRect.ToArray();
            } // motion historyból a mozgó részekből csinál egy rectangle tömböt. a Vector of rect a vektorai a rectanglenak

            double minArea = 50000;
            //iterate through each of the motion component, egyértelmű
            foreach (Rectangle comp in rects)
            {
                int area = comp.Width * comp.Height;
                //reject the components that have small area;
                if (area < minArea) continue;

                //Draw each individual motion
                CvInvoke.Rectangle(image, comp, new MCvScalar(255, 255, 0), 4, LineType.EightConnected);

                var originalImageFOrDisplay = image.ToImage<Bgr, byte>();
                var convertOriginalImageToBitmapSource = ToBitmapSource(originalImageFOrDisplay);
                ImageCaptured?.Invoke(convertOriginalImageToBitmapSource);
            }
        }

        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                bs.Freeze();
                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
    }
}
