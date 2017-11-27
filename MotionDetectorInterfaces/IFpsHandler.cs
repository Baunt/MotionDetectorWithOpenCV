using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetectorInterfaces
{
    public interface IFpsHandler
    {
        event Action<double> FrameRateChanged;

        double FrameRate { get; set; }

        double TotalFrames { get; set; }

    }
}
