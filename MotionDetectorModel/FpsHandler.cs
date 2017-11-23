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
        Timer fpsTimer;
        IVideoProcessor processor;
        bool isProcessFinished = false;

        public FpsHandler(IVideoProcessor processor)
        {
            this.processor = processor;
            this.processor.ProcessFinished += Processor_ProcessFinished;
            this.fpsTimer = new Timer();
        }

        private void Processor_ProcessFinished(object sender, EventArgs e)
        {
            isProcessFinished = true;
            SetFpsValue(FpsValue);
        }

        #region Interface implementation
        public int FpsValue { get; private set; }

        public void SetFpsValue(int value)
        {
            if (FpsValue != value)
            {
                fpsTimer.Stop();
            }

            FpsValue = value;

            if (isProcessFinished)
            {
                StartTimer();
            }
        } 
        #endregion

        private void StartTimer()
        {
            fpsTimer.Interval = 1000/FpsValue;
            fpsTimer.Elapsed += fpsTimerElapsed;
            fpsTimer.Start();
        }

        private void fpsTimerElapsed(object sender, ElapsedEventArgs e)
        {
            processor.Capture();

            if (processor.GrabbedFrame >= FpsValue)
            {
                //LOGGER
            }
        }
    }
}
