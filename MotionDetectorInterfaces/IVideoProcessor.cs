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

        event EventHandler ProcessFinished;

        void Capture();

        int GrabbedFrame { get; }

        void LoadVideo(string path);

        void CloseVideo();
    }
}
