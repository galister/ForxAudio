using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ForxAudio
{
    public class Config
    {
        private static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);
        private static readonly string ConfigFile = Path.Combine(ConfigDir, $"{Application.ProductName}.json");
        
        private static Config _instance;
        public static Config Instance => _instance ?? (_instance = ReadFromFile());

        public bool Init;
        public bool Enforce;
        public Guid DefaultPlayback;
        public Guid DefaultPlaybackComm;
        public Guid DefaultCapture;
        public Guid DefaultCaptureComm;

        public void SaveToFile()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this));
        }

        private static Config ReadFromFile()
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);
            
            return File.Exists(ConfigFile) 
                ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile)) 
                : new Config();
        }
    }
}