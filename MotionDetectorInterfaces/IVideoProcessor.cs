using Microsoft.Win32;
using System;
using System.Windows.Media.Imaging;

namespace MotionDetectorInterfaces
{
    public interface IVideoProcessor
    {
        event Action<BitmapSource> ImageCaptured;
        event Action<double> FrameRateChanged;
        event Action<double> TotalFramesChanged;

        void LoadVideo(OpenFileDialog path);

        void CloseVideo();

        void Start();

        void Stop();

        void Pause();

        VideoMethod CurrentState { get; set; }
    }
}
