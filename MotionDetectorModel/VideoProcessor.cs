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
        private bool playstate = false;

        private VideoCapture _capture;
        private MotionHistory _motionHistory;
        private BackgroundSubtractorMOG2 _forgroundDetector;
        private Mat _segMask;
        private Mat _forgroundMask;
        private List<BitmapSource> frames;
        private object lockObject;
        private Rectangle[] rects;
        private bool _isProcessFinished;
        private int _frameWidth { get; set; }
        private int _frameHeight { get; set; }
        private string _videoPath;

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
                                2.0, //in second, the duration of motion history you wants to keep
                                0.05, //in second, maxDelta for cvCalcMotionGradient
                                0.5); //in second, minDelta for cvCalcMotionGradient
            frames = new List<BitmapSource>();
            lockObject = new object();
            _isProcessFinished = false;
        }

        public void LoadVideo(OpenFileDialog path)
        {
            if (_capture == null)
            {
                try
                {
                    _videoPath = path.SafeFileName;
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
                    _frameWidth = (int)_capture.GetCaptureProperty(CapProp.FrameWidth);
                    _frameHeight = (int)_capture.GetCaptureProperty(CapProp.FrameHeight);
                    var size = new System.Drawing.Size(_frameWidth,_frameHeight);
                    FrameRate = 15; //Set the framerate manually as a camera would retun 0 if we use GetCaptureProperty()
                    
                    var VW = new VideoWriter($"../Temporary/{_videoPath}", -1, (int)FrameRate, size , true);

                    playstate = !playstate; //change playstate to the opposite
                    if (playstate)
                    {
                        _capture.Start();
                    }
                    else
                    {
                        _capture.Pause();
                    }
                }
            }
        }

        public void Stop()
        {
            if (_capture != null && CurrentState != VideoMethod.Stopped)
            {
                _capture.Stop();
                CurrentState = VideoMethod.Stopped;
            }
            else
            {
                //call the process frame to update the picturebox
                ProcessFrame(null, null);
            }
        }

        public void Pause()
        {
            if (_capture != null)
            {
                _capture.Pause();
            }
            else
            {
                //call the process frame to update the picturebox
                ProcessFrame(null, null);
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
            double framenumber = _capture.GetCaptureProperty(CapProp.PosFrames);
            try
            {
                _capture.Retrieve(frame);// az imaget feltölti a grabbed frammel
                if (frame != null && _isProcessFinished == false)
                {
                    _capture.Retrieve(frame);
                    if (_forgroundDetector == null)
                    {
                        _forgroundDetector = new BackgroundSubtractorMOG2(); // létrehozza egyszer a BGR maskot
                    }
                    _forgroundDetector.Apply(frame, _forgroundMask); // updateli az image bemenő byte arrayal, a foreground maskot, ami a végleges kép
                                                                     //update the motion history
                    _motionHistory.Update(_forgroundMask); // a motion historyt updateli

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
                        CvInvoke.Rectangle(frame, comp, new MCvScalar(255, 255, 0), 4, LineType.EightConnected);
                    }

                    Thread.Sleep((int)(1000.0 / FrameRate)); //This may result in fast playback if the codec does not tell the truth

                    //Lets check to see if we have reached the end of the video
                    //If we have lets stop the capture and video as in pause button was pressed
                    //and reset the video back to start
                    if (framenumber == TotalFrames)
                    {
                        framenumber = 0;
                        _capture.SetCaptureProperty(CapProp.PosFrames, framenumber);
                        //call the process frame to update the picturebox
                        ProcessFrame(null, null);
                        _isProcessFinished = true;
                    }
                }
                else
                {
                    var originalImageFOrDisplay = frame.ToImage<Bgr, byte>();
                    var convertOriginalImageToBitmapSource = ToBitmapSource(originalImageFOrDisplay.PyrDown());
                    ImageCaptured?.Invoke(convertOriginalImageToBitmapSource);
                }

            }
            catch (Exception ex)
            {
                //LOGGER
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
