using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.VideoSurveillance;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private BackgroundSubtractor _forgroundDetector;
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

            _capture.Retrieve(image);
            if (_forgroundDetector == null)
            {
                _forgroundDetector = new BackgroundSubtractorMOG2();
            }
                _forgroundDetector.Apply(image, _forgroundMask);
            //update the motion history
            _motionHistory.Update(_forgroundMask);

            #region get a copy of the motion mask and enhance its color
            double[] minValues, maxValues;
            System.Drawing.Point[] minLoc, maxLoc;
            _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLoc, out maxLoc);
            Mat motionMask = new Mat();
            using (ScalarArray sa = new ScalarArray(255.0 / maxValues[0]))
                CvInvoke.Multiply(_motionHistory.Mask, sa, motionMask, 1, DepthType.Cv8U);
            //Image<Gray, Byte> motionMask = _motionHistory.Mask.Mul(255.0 / maxValues[0]);
            #endregion

            //create the motion image 
            Mat motionImage = new Mat(motionMask.Size.Height, motionMask.Size.Width, DepthType.Cv8U, 3);
            motionImage.SetTo(new MCvScalar(0));

            var motionImageForDisplay = motionImage.ToImage<Bgr, byte>();
            var convertedImageFromMotionImage = ToBitmapSource(motionImageForDisplay);

            //var imageForDisplay =_forgroundMask.ToImage<Bgr,byte>();
            //var convertedImage = ToBitmapSource(imageForDisplay);
            ImageCaptured?.Invoke(convertedImageFromMotionImage);
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
            using (System.Drawing.Bitmap source = image.Bitmap)
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
