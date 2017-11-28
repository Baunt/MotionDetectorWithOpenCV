using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetectorInterfaces
{

    public delegate void FRCHandler(double frameRate);

    public interface IFpsHandler
    {
        
        event FRCHandler FrameRateChanged;

        double FrameRate { get; set; }

        double TotalFrames { get; set; }

    }
}
