using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Messenger.Native;
using Forms = System.Windows.Forms;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool IsWindows11 => Environment.OSVersion.Version.Build >= 22000;
        private readonly Forms.NotifyIcon _trayIcon;
        private IntPtr _handle;
        private Taskbar _taskbar = new();

        public MainWindow()
        {
            InitializeComponent();
            CheckState();
            DataContext = new MainWindowViewModel();

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("Images\\Chat.ico"),
                ContextMenuStrip = new Forms.ContextMenuStrip()
            };
            _trayIcon.ContextMenuStrip.Items.Add("Открыть", null, OnOpenClicked);
            _trayIcon.ContextMenuStrip.Items.Add("Закрыть", null, OnCloseClicked);

            _trayIcon.Visible = true;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    {
                        WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight, _taskbar);
                        //MainGrid.Margin = new Thickness(0, 7, 0, 0);
                        handled = true;

                        break;
                    }
            }

            return (IntPtr)0;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            if (ServerContext.IsConnected())
            {
                Package package = new(Command.Exit, "Exit");
                ServerContext.SendRequest(package);
            }
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void OnOpenClicked(object sender, EventArgs e)
        {
            Show();
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Minimized;
        }

        private void OnChangeSizeClick(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                return;
            }

            WindowState = WindowState.Maximized;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e) =>
            Close();

        private void OnActivated(object sender, EventArgs e) =>
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));

        private void OnStateChanged(object sender, EventArgs e) =>
            CheckState();

        private void CheckState()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;

            if (WindowState == WindowState.Maximized)
            {
                ChangeSize.Content = "\uE923";
                return;
            }

            ChangeSize.Content = "\uE739";
        }
    }
}