using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Messenger
{
    public class VideoHelper
    {
        public event Action<string, ImageSource> NewFrameRecieved;

        private IVideoSource _videoSource;
        private readonly RecieveThread _recieveVideoThread;
        private readonly UdpClient _udpVideo;
        private const int _udpVideoPort = 40016;
        private readonly string _currentEmail;
        private readonly ObservableCollection<VoiceChatMember> _members;
        private bool _isTimerStopped = false;
        private Timer _timer;

        public VideoHelper(string currentEmail, ObservableCollection<VoiceChatMember> members)
        {
            _members = members;
            _currentEmail = currentEmail;
            _recieveVideoThread = new RecieveThread(WaitVideo);
            _udpVideo = new UdpClient(_udpVideoPort);

            _recieveVideoThread.StartThread();

            StartTimer();
        }

        private void SupportConnection(object obj)
        {
            if (!_isTimerStopped)
            {
                OnNewFrame(null, null);
                return;
            }

            _timer.Dispose();
        }

        private void StartTimer()
        {
            TimerCallback timerCallback = new(SupportConnection);
            _timer = new(timerCallback, null, 0, 10000);
        }

        public void StartVideoRecord(FilterInfo currentDevice)
        {
            if (currentDevice != null)
            {
                _isTimerStopped = true;
                _videoSource = new VideoCaptureDevice(currentDevice.MonikerString);
                _videoSource.NewFrame += OnNewFrame;
                _videoSource.Start();
            }
        }

        public async Task StopVideoRecord()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= OnNewFrame;
                for (int i = 0; i < 5; ++i)
                {
                    NewFrameRecieved?.Invoke(_currentEmail, null);
                    OnNewFrame(null, null);
                    await Task.Delay(500);
                }

                _isTimerStopped = false;
                StartTimer();
            }
        }

        private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            FrameInfo frameInfo = new(_currentEmail);
            if (eventArgs is not null)
            {
                using var bitmap = (Bitmap)eventArgs.Frame.Clone();
                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                BitmapImage bi;
                bi = bitmap.ToBitmapImage();
                bi.Freeze();

                NewFrameRecieved?.Invoke(_currentEmail, bi);
                frameInfo.Frame = ms.ToArray();
            }
            else
                frameInfo.Frame = null;

            try
            {
                foreach (var user in _members)
                {
                    if (user.IPUser.Email == _currentEmail)
                        continue;

                    var ipep = new IPEndPoint(IPAddress.Parse(user.IPUser.IP), 40016);
                    string messageJson = JsonSerializer.Serialize(frameInfo);
                    byte[] messageByte = Encoding.UTF8.GetBytes(messageJson);
                    _udpVideo?.Send(messageByte, messageByte.Length, ipep);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Поток вывода видео завершился");
            }
        }

        private async void WaitVideo(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    byte[] messageByte = (await _udpVideo.ReceiveAsync().WithCancellation(token)).Buffer;
                    string messageJson = Encoding.UTF8.GetString(messageByte);
                    FrameInfo frameInfo = JsonSerializer.Deserialize<FrameInfo>(messageJson);
                    if (frameInfo.Frame is not null)
                    {
                        using var ms = new MemoryStream(frameInfo.Frame);
                        BitmapImage bi;
                        using var bitmap = new Bitmap(ms);
                        bi = bitmap.ToBitmapImage();
                        bi.Freeze();
                        NewFrameRecieved?.Invoke(frameInfo.Email, bi);
                    }
                    else
                        NewFrameRecieved?.Invoke(frameInfo.Email, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async void Exit()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= OnNewFrame;
                for (int i = 0; i < 5; ++i)
                {
                    NewFrameRecieved?.Invoke(_currentEmail, null);
                    OnNewFrame(null, null);
                    await Task.Delay(500);
                }
            }
            _videoSource = null;
            _isTimerStopped = true;

            _recieveVideoThread.CancelThread();

            _udpVideo.Close();
            _udpVideo.Dispose();
        }
    }
}
