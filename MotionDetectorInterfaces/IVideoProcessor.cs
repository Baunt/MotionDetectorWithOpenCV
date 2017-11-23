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

        void Capture();

        string VideoPath { get; set; }
    }
}
