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
        public event Action<BitmapSource, Rectangle> ImageCaptured;

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

            #region get a copy of the motion mask and enhance its color
            double[] minValues, maxValues;
            System.Drawing.Point[] minLoc, maxLoc;
            _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLoc, out maxLoc); // a motion mask, a min/max lokáció és értékeket adja vissza
            Mat motionMask = new Mat();
            using (ScalarArray sa = new ScalarArray(255.0 / maxValues[0]))
                CvInvoke.Multiply(_motionHistory.Mask, sa, motionMask, 1, DepthType.Cv8U); //a két bemenő , motion history mask és az skaláris tömb bemenő adatokból a motionMask értékét adja meg 8byteos képként skálázva        
            #endregion

            //create the motion image 
            Mat motionImage = new Mat(motionMask.Size.Height, motionMask.Size.Width, DepthType.Cv8U, 3); // motion image a mask horizontális és vertikális összetevőiből áll össze
            motionImage.SetTo(new MCvScalar(0)); // skalárisan beállíja

            CvInvoke.InsertChannel(motionMask, motionImage, 0); // ?????? megfejteni

            double minArea = 100;

            Rectangle[] rects;
            using (VectorOfRect boundingRect = new VectorOfRect())
            {
                _motionHistory.GetMotionComponents(_segMask, boundingRect);
                rects = boundingRect.ToArray();
            } // motion historyból a mozgó részekből csinál egy rectangle tömböt. a Vector of rect a vektorai a rectanglenak

            //iterate through each of the motion component, egyértelmű
            foreach (Rectangle comp in rects)
            {
                int area = comp.Width * comp.Height;
                //reject the components that have small area;
                if (area < minArea) continue;

                // find the angle and motion pixel count of the specific area
                double angle, motionPixelCount;
                _motionHistory.MotionInfo(_forgroundMask, comp, out angle, out motionPixelCount);

                //reject the area that contains too few motion
                if (motionPixelCount < area * 0.05) continue;

                //CropImage(comp);

                //Draw each individual motion
                CvInvoke.Rectangle(motionImage, comp, new MCvScalar(255, 255, 0));

                var originalImageFOrDisplay = image.ToImage<Bgr, byte>();
                var convertOriginalImageToBitmapSource = ToBitmapSource(originalImageFOrDisplay);
                ImageCaptured?.Invoke(convertOriginalImageToBitmapSource, comp);

                //var cropRect = new Int32Rect(
                //    Convert.ToInt32(comp.Left),
                //    Convert.ToInt32(comp.Top),
                //    Convert.ToInt32(comp.Width),
                //    Convert.ToInt32(comp.Height));


                //var originalImageFOrDisplay = image.ToImage<Bgr, byte>();
                //var convertOriginalImageToBitmapSource = ToBitmapSource(originalImageFOrDisplay);

                //var cropped = new CroppedBitmap(convertOriginalImageToBitmapSource, cropRect);
                //cropped.Freeze();
                //ImageCaptured?.Invoke(cropped);
            }

            //var motionImageForDisplay = motionImage.ToImage<Bgr, byte>();
            //var convertedImageFromMotionImage = ToBitmapSource(motionImageForDisplay);
            //ImageCaptured?.Invoke(convertedImageFromMotionImage);

            //var imageForDisplay = _forgroundMask.ToImage<Bgr, byte>();
            //var convertedImage = ToBitmapSource(imageForDisplay);
            //ImageCaptured?.Invoke(convertedImage);
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
