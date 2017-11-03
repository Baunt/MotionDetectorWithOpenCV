using Microsoft.Win32;
using MotionDetectorModel;
using MotionDetectorUI.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MotionDetectorUI.ViewModel
{
    public class VideoDisplayViewModel : ViewModelBase
    {
        VideoProcessor processor;

        public VideoDisplayViewModel()
        {
            processor = new VideoProcessor();
            processor.ImageCaptured += Processor_ImageCaptured;
            LoadCommand = new SimpleCommand { ExecuteDelegate = Load };
        }

        private void Processor_ImageCaptured(BitmapSource obj, Rectangle rect)
        {
            VideoSourcePath = obj;
            RectendgleOnVideoSource = rect;
        }

        private BitmapSource _videoSourcePath;

        public BitmapSource VideoSourcePath
        {
            get { return _videoSourcePath; }
            set { _videoSourcePath = value; OnPropertyChanged(nameof(VideoSourcePath)); }
        }

        private Rectangle _rectangleOnVideoSource;

        public Rectangle RectendgleOnVideoSource
        {
            get { return _rectangleOnVideoSource; }
            set { _rectangleOnVideoSource = value; OnPropertyChanged(nameof(RectendgleOnVideoSource)); }
        }

        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get { return _loadCommand; }
            set { _loadCommand = value; OnPropertyChanged(nameof(LoadCommand)); }
        }

        private void Load(object obj)
        {
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
            processor.Capture(op.FileName);
        }
    }
}
