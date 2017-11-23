using MotionDetectorInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace MotionDetectorModel
{
    public class FpsHandler : IFpsHandler
    {
        int fpsCount = 0;
        Timer fpsTimer = new Timer();
        IVideoProcessor processor;

        public FpsHandler(IVideoProcessor processor)
        {
            this.processor = processor;
        }

        #region Interface implementation
        public int FpsValue { get; private set; }

        public void SetFpsValue(int value)
        {
            FpsValue = value;
            StartTimer();
        } 
        #endregion

        private void StartTimer()
        {
            fpsTimer.Interval = 1000/FpsValue;
            fpsTimer.Elapsed += fpsTimerElapsed;
            fpsTimer.Enabled = true;
        }

        private void fpsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            processor.Capture();
        }
    }
}
