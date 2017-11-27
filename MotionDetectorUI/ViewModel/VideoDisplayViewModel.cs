using Microsoft.Win32;
using MotionDetectorInterfaces;
using MotionDetectorModel;
using MotionDetectorUI.Command;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MotionDetectorUI.ViewModel
{
    public class VideoDisplayViewModel : ViewModelBase
    {
        IVideoProcessor processor;
        IFpsHandler fpsHandler;
        readonly int defaultSliderValue = 30;

        public VideoDisplayViewModel()
        {
            processor = new VideoProcessor();
            fpsHandler = new FpsHandler(processor);
            processor.ImageCaptured += Processor_ImageCaptured;
            LoadCommand = new SimpleCommand { ExecuteDelegate = Load };
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

        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get { return _loadCommand; }
            set { _loadCommand = value; OnPropertyChanged(nameof(LoadCommand)); }
        }

        private int _sliderValue;

        public int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = value;
                fpsHandler.SetFpsValue(_sliderValue);
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
            processor.LoadVideo(op.FileName);
            SliderValue = defaultSliderValue;
        }
    }
}
