using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace serviceWrapper
{
    public partial class Service1 : ServiceBase
    {
        Process process = null;
        string exePath = null;
        string processName = null;
        string identifidBy = null;
        string pidFile = null;

        public Service1()
        {
            InitializeComponent();
            exePath = ConfigurationManager.AppSettings["exePath"];
            processName = Path.GetFileNameWithoutExtension(exePath);
            identifidBy = ConfigurationManager.AppSettings["identifidBy"];
            pidFile = ConfigurationManager.AppSettings["pidFile"];
        }

        protected override void OnStart(string[] args)
        {
            process = callExe(exePath, ConfigurationManager.AppSettings["startPara"]);
            process.Start();
        }

        protected override void OnStop()
        {
            if (identifidBy == "name")
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["stopPara"]))
                {
                    if (process != null)
                    {
                        process.Kill();
                        process = null;
                    }
                    else
                    {
                        foreach (var p in Process.GetProcessesByName(processName))
                        {
                            p.Kill();
                        }
                    }
                }
                else
                {
                    var p = callExe(exePath, ConfigurationManager.AppSettings["stopPara"]);
                    p.Start();
                    p.WaitForExit();
                }
            }
            else if (identifidBy == "pid")
            {
                var p = Process.GetProcessById(int.Parse(File.ReadAllText(pidFile)));
                if (p != null)
                {
                    p.Kill();
                }
            }
        }

        static Process callExe(string exepath, string para)
        {
            Process p = new Process();
            p.StartInfo.FileName = exepath;
            if (!string.IsNullOrEmpty(para))
            {
                p.StartInfo.Arguments = para;
            }
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = exepath.Substring(0, exepath.LastIndexOf('\\'));
            return p;
        }
    }
}