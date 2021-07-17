using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;

namespace ForxAudio
{
    public class DeviceEnforcer
    {
        public static readonly DeviceEnforcer Instance = new DeviceEnforcer(); 
        public AudioController<CoreAudioDevice> Controller { get; } = new CoreAudioController();

        public DeviceEnforcer()
        {
            Controller.AudioDeviceChanged.Subscribe(a =>
            {
                if (a.ChangedType == DeviceChangedType.DefaultChanged)
                {
                    Ensure();
                }

                if (a.ChangedType == DeviceChangedType.StateChanged)
                {
                    NotifyVoicemeeter();
                }
            });
        }
        
        public bool Ensure()
        {
            var worked = false;
            
            if (!Config.Instance.Enforce)
                return false;
            
            if (Config.Instance.DefaultPlayback != Guid.Empty && Config.Instance.DefaultPlayback != Controller.DefaultPlaybackDevice?.Id)
                worked = Apply(Config.Instance.DefaultPlayback, Role.Multimedia);
            
            if (Config.Instance.DefaultPlaybackComm != Guid.Empty && Config.Instance.DefaultPlaybackComm != Controller.DefaultPlaybackCommunicationsDevice?.Id)
                worked = worked || Apply(Config.Instance.DefaultPlaybackComm, Role.Communications);
            
            if (Config.Instance.DefaultCapture != Guid.Empty && Config.Instance.DefaultCapture != Controller.DefaultCaptureDevice?.Id)
                worked = worked || Apply(Config.Instance.DefaultCapture, Role.Multimedia);
            
            if (Config.Instance.DefaultCaptureComm != Guid.Empty && Config.Instance.DefaultCaptureComm != Controller.DefaultCaptureCommunicationsDevice?.Id)
                worked = worked || Apply(Config.Instance.DefaultCaptureComm, Role.Communications);

            return worked;
        }

        private bool IsDefaultDevice(IDevice dev, Role role) => (role & Role.Communications) != 0 ? dev.IsDefaultCommunicationsDevice : dev.IsDefaultDevice;
        
        private bool Apply(Guid id, Role role)
        {
            var dev = Controller.GetDevice(id);
            if (dev != null)
            {
                var attempts = 5;
                while (attempts-- > 0)
                {
                    if (role == Role.Communications)
                        dev.SetAsDefaultCommunications();
                    else
                        dev.SetAsDefault();
                    Thread.Sleep(500);

                    if (IsDefaultDevice(dev, role))
                        return true;
                }
            }

            return false;
        }

        private static readonly string[] ProcessNames =
        {
            "voicemeeter8x64",
            "voicemeeter8",
            "voicemeeterprox64",
            "voicemeeterpro",
            "voicemeeterx64",
            "voicemeeter"
        };
        private void NotifyVoicemeeter()
        {
            foreach (var pName in ProcessNames)
            {
                var p = Process.GetProcessesByName(pName);
                if (p.Length > 0)
                {
                    var path = p[0].GetMainModuleFilePath();
                    Process.Start(path, "-R");
                    break;
                }
            }
        }
        
    }
    
    internal static class NativeExtensions {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFilePath(this Process process, int buffer = 1024) {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    }
}