using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Messenger
{
    public static class AsyncExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return task.Result;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }
    }


    public class VoiceHelper
    {
        private WaveInEvent _waveIn;
        private readonly RecieveThread _recieveVoiceThread;
        private readonly UdpClient _udpVoice;
        private const int _udpVoicePort = 40015;
        private readonly string _currentEmail;
        private readonly ObservableCollection<VoiceChatMember> _members;
        private bool _isTimerStopped = false;
        private Timer _timer;

        public VoiceHelper(string currentEmail, ObservableCollection<VoiceChatMember> members)
        {
            _members = members;
            _currentEmail = currentEmail;
            _recieveVoiceThread = new RecieveThread(WaitSound);
            _udpVoice = new UdpClient(_udpVoicePort);

            _waveIn = new WaveInEvent
            {
                BufferMilliseconds = 100,
                NumberOfBuffers = 10,

                DeviceNumber = 0
            };

            _waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(SendData);
            _waveIn.WaveFormat = new WaveFormat(8000, 16, 1/*44200, 2*/);
            _recieveVoiceThread.StartThread();
        }

        private void SupportConnection(object obj)
        {
            if (!_isTimerStopped)
            {
                SendData(null, null);
                return;
            }

            _timer.Dispose();
        }

        private void StartTimer()
        {
            TimerCallback timerCallback = new(SupportConnection);
            _timer = new(timerCallback, null, 0, 10000);
        }

        public void StartVoiceRecord()
        {
            _isTimerStopped = true;
            _waveIn.StartRecording();
        }

        public void StopVoiceRecord()
        {
            _waveIn.StopRecording();
            _isTimerStopped = false;
            StartTimer();
        }

        private void SendData(object sender, WaveInEventArgs e)
        {
            foreach (var user in _members)
            {
                if (user.IPUser.Email == _currentEmail)
                    continue;

                var ipep = new IPEndPoint(IPAddress.Parse(user.IPUser.IP), 40015);
                if (e is not null)
                {
                    _udpVoice.Send(e.Buffer, e.Buffer.Length, ipep);
                    continue;
                }

                _udpVoice.Send(Array.Empty<byte>(), Array.Empty<byte>().Length, ipep);
            }
        }

        private async void WaitSound(CancellationToken token)
        {
            BufferedWaveProvider _playBuffer = new(new WaveFormat(8000, 16, 1/*44200, 2*/));

            WaveOutEvent _waveOut = new();
            _waveOut.Init(_playBuffer);
            _waveOut.Play();

            try
            {
                while (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var incoming = await _udpVoice.ReceiveAsync().WithCancellation(token);
                    _playBuffer.AddSamples(incoming.Buffer, 0, incoming.Buffer.Length);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Поток вывода звука завершился");
            }
            finally
            {
                _waveOut.Dispose();
            }

        }

        public void Exit()
        {
            _isTimerStopped = true;
            _waveIn.Dispose();
            _waveIn = null;

            _recieveVoiceThread.CancelThread();

            _udpVoice.Close();
            _udpVoice.Dispose();
        }
    }
}
