using Microsoft.Win32;
using MotionDetectorUI.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MotionDetectorUI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public MainWindowViewModel()
        {
            VideoDisplayViewModel videoDisplayViewModel = new VideoDisplayViewModel();
            CurrentView = videoDisplayViewModel;
        }
    }
}
