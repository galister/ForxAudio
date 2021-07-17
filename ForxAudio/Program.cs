using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForxAudio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var myPid = Process.GetCurrentProcess().Id;
            if (Process.GetProcessesByName(Application.ProductName).Any(x => x.Id != myPid))
            {
                MessageBox.Show($"{Application.ProductName} is already running!");
                Environment.Exit(12);
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }
    }
}