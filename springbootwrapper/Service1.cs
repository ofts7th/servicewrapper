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
        public void logMessage(string m)
        {
            if ("log" == ConfigurationManager.AppSettings["log"])
            {
                m = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + m + Environment.NewLine;
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), m, Encoding.UTF8);
            }
        }

        string javaPath = ConfigurationManager.AppSettings["javaPath"];
        string appName = ConfigurationManager.AppSettings["appName"];
        string classPath = ConfigurationManager.AppSettings["classPath"];
        string jarDir = ConfigurationManager.AppSettings["jarDir"];
        string pidFile = ConfigurationManager.AppSettings["pidFile"];
        string javaArgs = "";

        public Service1()
        {
            InitializeComponent();
            List<string> cpList = new List<string>();
            cpList.Add(classPath);
            foreach (string jar in Directory.GetFiles(jarDir))
            {
                if (jar.EndsWith(".jar"))
                {
                    cpList.Add(jar);
                }
            }
            cpList.RemoveAll(x => string.IsNullOrEmpty(x));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cpList.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(";");
                }
                sb.Append(cpList[i]);
            }
            sb.Append(" ");
            javaArgs = " -classpath " + sb.ToString() + " " + appName;
        }

        protected override void OnStart(string[] args)
        {
            callExe(javaPath, javaArgs).Start();
        }

        protected override void OnStop()
        {
            var p = Process.GetProcessById(int.Parse(File.ReadAllText(pidFile)));
            if (p != null)
            {
                p.Kill();
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
            return p;
        }
    }
}