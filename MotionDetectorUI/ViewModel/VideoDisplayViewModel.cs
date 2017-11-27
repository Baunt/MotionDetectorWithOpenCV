using Microsoft.Win32;
using MotionDetectorInterfaces;
using MotionDetectorModel;
using MotionDetectorUI.Command;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;

namespace MotionDetectorUI.ViewModel
{
    public class VideoDisplayViewModel : ViewModelBase
    {
        IVideoProcessor processor;
        IFpsHandler fpsHandler;

        public VideoDisplayViewModel()
        {
            processor = new VideoProcessor();
            fpsHandler = new FpsHandler(processor);
            processor.ImageCaptured += Processor_ImageCaptured;
            fpsHandler.FrameRateChanged += FpsHandler_FrameRateChanged;
            LoadCommand = new SimpleCommand { ExecuteDelegate = Load };
            StartCommand = new SimpleCommand { ExecuteDelegate = Start };
            StopCommand = new SimpleCommand { ExecuteDelegate = Stop };
            PauseCommand = new SimpleCommand { ExecuteDelegate = Pause };
        }

        private void FpsHandler_FrameRateChanged(double obj)
        {
            SliderValue = obj;
        }

        private void Processor_ImageCaptured(BitmapSource obj)
        {
            VideoSourcePath = obj;
        }

        private BitmapSource _videoSourcePath;

        public BitmapSource VideoSourcePath
        {
            get { return _videoSourcePath; }
            set { _videoSourcePath = value; OnPropertyChanged(nameof(VideoSourcePath)); }
        }

        #region Commands
        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get { return _loadCommand; }
            set { _loadCommand = value; OnPropertyChanged(nameof(LoadCommand)); }
        }

        private ICommand _stopCommand;

        public ICommand StopCommand
        {
            get { return _stopCommand; }
            set { _stopCommand = value; OnPropertyChanged(nameof(StopCommand)); }
        }

        private ICommand _startCommand;

        public ICommand StartCommand
        {
            get { return _startCommand; }
            set { _startCommand = value; OnPropertyChanged(nameof(StartCommand)); }
        }

        private ICommand _pauseCommand;

        public ICommand PauseCommand
        {
            get { return _pauseCommand; }
            set { _pauseCommand = value; OnPropertyChanged(nameof(PauseCommand)); }
        }
        #endregion

        private double _sliderValue;

        public double SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                OnPropertyChanged(nameof(SliderValue));
            }
        }

        private void Load(object obj)
        {
            processor.CloseVideo();

            OpenFileDialog op = new OpenFileDialog
            {
                Title = "Select a video",
                Filter = "All supported graphics|*.mp4;*.wmv;*.avi;*.mov|" +
                         "AVI (*.avi;)|*.avi;|" +
                         "WMV (*.wmv;)|*.wmv;|" +
                         "MOV (*.mov;)|*.mov;|" +
                         "MP4 (*.mp4)|*.mp4",
                Multiselect = true
            };
            if (op.ShowDialog() != true) return;
            processor.LoadVideo(op);
        }

        private void Stop(object obj)
        {
            processor.Stop();
        }

        private void Start(object obj)
        {
            processor.Start();
        }

        private void Pause(object obj)
        {
            processor.Pause();
        }
    }
}
