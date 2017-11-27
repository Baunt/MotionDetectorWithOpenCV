using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MotionDetectorInterfaces
{
    public interface IVideoProcessor
    {
        event Action<BitmapSource> ImageCaptured;

        void LoadVideo(OpenFileDialog path);

        void CloseVideo();

        void Start();

        void Stop();

        void Pause();

        VideoMethod CurrentState { get; set; }
    }
}
