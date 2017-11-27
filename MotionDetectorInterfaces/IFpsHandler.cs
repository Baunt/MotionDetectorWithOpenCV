using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionDetectorInterfaces
{
    public interface IFpsHandler
    {
        int FpsValue { get;}

        void SetFpsValue(int value);
    }
}
