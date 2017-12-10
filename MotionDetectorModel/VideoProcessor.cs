using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.VideoSurveillance;
using Microsoft.Win32;
using MotionDetectorInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MotionDetectorModel
{
    public class VideoProcessor : IVideoProcessor
    {
        private VideoCapture _capture;
        private MotionHistory _motionHistory;
        private BackgroundSubtractorMOG2 _forgroundDetector;
        private Mat _segMask;
        private Mat _forgroundMask;
        private List<BitmapSource> frames;
        private object lockObject;
        private Rectangle[] rects;
        private int _frameWidth { get; set; }
        private int _frameHeight { get; set; }

        public VideoMethod CurrentState { get; set; }

        private double _frameRate;
        public double FrameRate
        {
            get
            {
                return _frameRate;
            }

            set
            {
                _frameRate = value;
                FrameRateChanged?.Invoke(_frameRate);
            }
        }

        private double _totalFrames;
        public double TotalFrames
        {
            get
            {
                return _totalFrames;
            }

            set
            {
                _totalFrames = value;
                TotalFramesChanged?.Invoke(_totalFrames);
            }
        }

        public event Action<double> FrameRateChanged;
        public event Action<double> TotalFramesChanged;
        public event Action<BitmapSource> ImageCaptured;

        public VideoProcessor()
        {
            _segMask = new Mat();
            _forgroundMask = new Mat();
            _motionHistory = new MotionHistory(
                                0.1, //in second, the duration of motion history you wants to keep
                                0.05, //in second, maxDelta for cvCalcMotionGradient
                                0.5); //in second, minDelta for cvCalcMotionGradient
            frames = new List<BitmapSource>();
            lockObject = new object();
        }

        public void LoadVideo(OpenFileDialog path)
        {
            if (_capture == null)
            {
                try
                {
                    _capture = new VideoCapture(path.FileName);
                    _capture.ImageGrabbed += ProcessFrame;
                    TotalFrames = _capture.GetCaptureProperty(CapProp.FrameCount);
                    FrameRate = _capture.GetCaptureProperty(CapProp.Fps);
                    CurrentState = VideoMethod.Viewing;
                }
                catch (NullReferenceException excpt)
                {
                    throw new NullReferenceException("Rossz", excpt);
                }
            }
        }

        public void Start()
        {
            if (_capture != null)
            {
                if (CurrentState == VideoMethod.Viewing)
                {
                    _capture.Start();
                }
            }
        }

        public void Stop()
        {
            if (_capture != null && CurrentState != VideoMethod.Stopped)
            {
                CloseVideo();
                CurrentState = VideoMethod.Stopped;
            }
        }

        public void Pause()
        {
            if (_capture != null)
            {
                _capture.Pause();
            }
        }

        public void CloseVideo()
        {
            if (_capture != null)
            {
                _capture.Stop();
                _capture.Dispose();
                _capture = null;
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            Mat resizedFrame = new Mat();
            try
            {
                _capture.Retrieve(frame);

                if (frame.Size.Height > 768 || frame.Size.Width > 1024)
                {
                    CvInvoke.Resize(frame, resizedFrame, new System.Drawing.Size(), 0.3, 0.3, Inter.Linear); 
                }

                if (resizedFrame != null)
                {
                    if (_forgroundDetector == null)
                    {
                        _forgroundDetector = new BackgroundSubtractorMOG2(); 
                    }
                    _forgroundDetector.Apply(resizedFrame, _forgroundMask); 
                    _motionHistory.Update(_forgroundMask); 

                    using (VectorOfRect boundingRect = new VectorOfRect())
                    {
                        _motionHistory.GetMotionComponents(_segMask, boundingRect);
                        rects = boundingRect.ToArray();
                    } 
                    double minArea = 3000;
                    foreach (Rectangle comp in rects)
                    {
                        int area = comp.Width * comp.Height;
                        if (area < minArea) continue;
                        
                        CvInvoke.Rectangle(resizedFrame, comp, new MCvScalar(255, 255, 0), 2, LineType.EightConnected);
                    }

                    var originalImageFOrDisplay = resizedFrame.ToImage<Bgr, byte>();
                    var convertOriginalImageToBitmapSource = ToBitmapSource(originalImageFOrDisplay);
                    ImageCaptured?.Invoke(convertOriginalImageToBitmapSource);
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        #region Converter
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
        #endregion
    }
}
