using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using ForxAudio.Properties;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace ForxAudio
{
    public class AppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly MenuItem _enabled;
        private readonly MenuItem _defaultPlayback;
        private readonly MenuItem _defaultPlaybackComm;
        private readonly MenuItem _defaultCapture;
        private readonly MenuItem _defaultCaptureComm;
        private readonly MenuItem _runOnBoot;
        private readonly Timer _timer;

        private readonly string _startupFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{Application.ProductName}.lnk");
        
        public AppContext()
        {
            if (!Config.Instance.Init)
            {
                Config.Instance.Init = true;
                Config.Instance.Enforce = true;
                Config.Instance.DefaultPlayback = DeviceEnforcer.Instance.Controller.DefaultPlaybackDevice.Id;
                Config.Instance.DefaultPlaybackComm = DeviceEnforcer.Instance.Controller.DefaultPlaybackCommunicationsDevice.Id;
                Config.Instance.DefaultCapture = DeviceEnforcer.Instance.Controller.DefaultCaptureDevice.Id;
                Config.Instance.DefaultCaptureComm = DeviceEnforcer.Instance.Controller.DefaultCaptureCommunicationsDevice.Id;
                Config.Instance.SaveToFile();
            }
            
            _trayIcon = new NotifyIcon()
            {
                Icon = Resources.Icon,
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "Audio Device Changed",
                BalloonTipText = "Your selected audio devices have been re-applied!",
                ContextMenu = new ContextMenu(new[] {
                    _defaultPlayback = new MenuItem("Playback Device: Primary"),
                    _defaultPlaybackComm = new MenuItem("Playback Device: Communications"),
                    _defaultCapture = new MenuItem("Capture Device: Primary"),
                    _defaultCaptureComm = new MenuItem("Capture Device: Communications"),
                    _enabled = new MenuItem("Ensure These Devices at All Times", Enabled) { Checked = Config.Instance.Enforce },
                    _runOnBoot = new MenuItem("Run on Boot", RunOnStartup) {Checked = File.Exists(_startupFile)},
                    new MenuItem("Shut Down", Exit)
                }),
                Visible = true,
            };
            _trayIcon.ContextMenu.Popup += PopulateDevices;

            _timer = new Timer
            {
                Enabled = true,
                Interval = 30000
            };
            _timer.Tick += (sender, args) =>
            {
                if (DeviceEnforcer.Instance.Ensure())
                {
                    _trayIcon.ShowBalloonTip(15000);
                }
            };
            _timer.Start();
        }

        private void PopulateDevices(object o, EventArgs e)
        {
            var playBackDevices = DeviceEnforcer.Instance.Controller.GetDevices(DeviceType.Playback);
            var captureDevices = DeviceEnforcer.Instance.Controller.GetDevices(DeviceType.Capture);
            
            _defaultPlayback.MenuItems.Clear();
            _defaultPlaybackComm.MenuItems.Clear();
            _defaultCapture.MenuItems.Clear();
            _defaultCaptureComm.MenuItems.Clear();
            
            foreach (var dev in playBackDevices)
            {
                _defaultPlayback.MenuItems.Add(new MenuItem(dev.InterfaceName, SetDefault)
                {
                    Checked = dev.IsDefaultDevice,
                    Tag = dev
                });
                
                _defaultPlaybackComm.MenuItems.Add(new MenuItem(dev.InterfaceName, SetDefaultComm)
                {
                    Checked = dev.IsDefaultCommunicationsDevice,
                    Tag = dev
                });
            }
            
            foreach (var dev in captureDevices)
            {
                _defaultCapture.MenuItems.Add(new MenuItem(dev.InterfaceName, SetDefault)
                {
                    Checked = dev.IsDefaultDevice,
                    Tag = dev
                });
                
                _defaultCaptureComm.MenuItems.Add(new MenuItem(dev.InterfaceName, SetDefaultComm)
                {
                    Checked = dev.IsDefaultCommunicationsDevice,
                    Tag = dev
                });
            }
        }

        private void SetDefault(object o, EventArgs e)
        {
            if (o is MenuItem m && m.Tag is CoreAudioDevice d)
            {
                if ((d.DeviceType & DeviceType.Playback) != 0)
                    Config.Instance.DefaultPlayback = d.Id;
                else
                    Config.Instance.DefaultCapture = d.Id;
                
                d.SetAsDefault();
                Config.Instance.SaveToFile();
            }
        }
        
        private void SetDefaultComm(object o, EventArgs e)
        {
            if (o is MenuItem m && m.Tag is CoreAudioDevice d)
            {
                if ((d.DeviceType & DeviceType.Playback) != 0)
                    Config.Instance.DefaultPlaybackComm = d.Id;
                else
                    Config.Instance.DefaultCaptureComm = d.Id;
                
                d.SetAsDefaultCommunications();
                Config.Instance.SaveToFile();
            }
        }
        
        private void Enabled(object o, EventArgs e)
        {
            Config.Instance.Enforce = !Config.Instance.Enforce;
            _enabled.Checked = Config.Instance.Enforce;
            Config.Instance.SaveToFile();
        }
        
        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Application.Exit();
        }
        
        private void RunOnStartup(object sender, EventArgs e)
        {
            if (File.Exists(_startupFile))
            {
                File.Delete(_startupFile);
                _runOnBoot.Checked = false;
            }
            else
            {
                var srcFolder = Path.GetDirectoryName(Application.ExecutablePath);
                var tgtFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);
                
                
                var filesToCopy = new[] { $"{Application.ProductName}.exe", "AudioSwitcher.AudioApi.dll", "AudioSwitcher.AudioApi.CoreAudio.dll", "Newtonsoft.Json.dll" };
                foreach (var s in filesToCopy)
                {
                    File.Copy(Path.Combine(srcFolder, s), Path.Combine(tgtFolder, s), true);
                }

                var wshShell = new WshShell();
                var shortcut = (IWshShortcut) wshShell.CreateShortcut(_startupFile);

                shortcut.TargetPath = Path.Combine(tgtFolder, filesToCopy[0]);
                shortcut.WorkingDirectory = tgtFolder;
                shortcut.Description = $"Launch {Application.ProductName}";
                shortcut.Save();
                _runOnBoot.Checked = true;
            }
        }
    }
}