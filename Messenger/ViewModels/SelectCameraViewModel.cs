using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class SelectCameraViewModel : BaseViewModel
    {
        public RelayCommand EnableCameraCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public string[] Devices { get; set; }
        public string SelectedDevice { get; set; }

        private readonly VideoHelper _videoHelper;
        private readonly FilterInfoCollection _devices;

        public SelectCameraViewModel(VideoHelper videoHelper)
        {
            EnableCameraCommand = new RelayCommand(EnableCamera);
            CancelCommand = new RelayCommand(Cancel);

            _videoHelper = videoHelper;
            _devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            Devices = _devices.Cast<FilterInfo>().Select(fi => fi.Name).ToArray();
        }

        private void EnableCamera(object obj)
        {
            _videoHelper.StartVideoRecord(_devices.Cast<FilterInfo>().FirstOrDefault(fi => fi.Name == SelectedDevice));
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(SelectCameraViewModel));
        }

        private void Cancel(object obj)
        {
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(SelectCameraViewModel));
        }
    }
}
