using Shadowsocks.Controller;
using Shadowsocks.Properties;
using Shadowsocks.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Shadowsocks.Model;

namespace Shadowsocks
{
    static class Program
    {
        private static TestConfig _config;
        private static bool _needSave = false;

        public static TestConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = TestConfig.Load(TestConfig.File);
                    if (_config == null)
                    {
                        _config = new TestConfig { killPolipo = false, standalonePolipo = true };
                        _needSave = true;
                    }
                }
                return _config;
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Util.Utils.ReleaseMemory();


            if(Config.killPolipo)
            {
                Process[] proc = Process.GetProcessesByName("ss_polipo");
                if (proc.Length > 0)
                {
                    proc[0].Kill();
                }
            }

            using (Mutex mutex = new Mutex(false, "Global\\" + "71981632-A427-497F-AB91-241CD227EC1F"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName("Shadowsocks");
                    if (oldProcesses.Length > 0)
                    {
                        Process oldProcess = oldProcesses[0];
                    }
                    MessageBox.Show("Shadowsocks is already running.\n\nFind Shadowsocks icon in your notify tray.");
                    return;
                }
                Directory.SetCurrentDirectory(Application.StartupPath);
#if !DEBUG
                Logging.OpenLogFile();
#endif
                ShadowsocksController controller = new ShadowsocksController();

                MenuViewController viewController = new MenuViewController(controller);

                controller.Start();

                Application.Run();

                if(_needSave) TestConfig.Save(Config, TestConfig.File);
            }
        }
    }
}
