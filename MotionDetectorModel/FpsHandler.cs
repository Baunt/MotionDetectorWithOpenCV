using MotionDetectorInterfaces;
using System;

namespace MotionDetectorModel
{
    public class FpsHandler : IFpsHandler
    {
        public event Action<double> FrameRateChanged;

        private IVideoProcessor processor;

        public FpsHandler(IVideoProcessor processor)
        {
            this.processor = processor;
        }

        #region Interface implementation
        private double _frameRate;


        public double FrameRate
        {
            get { return _frameRate; }
            set
            {
                _frameRate = value;
                FrameRateChanged?.Invoke(_frameRate);
            }
        }

        public double TotalFrames { get; set; }
        #endregion


    }
}
