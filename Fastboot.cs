using System;
using System.Diagnostics;
using System.IO;

namespace FastbootEnhance
{
    class Fastboot : IDisposable
    {
        Process process;
        public StreamReader stdout;
        public StreamReader stderr;

        public Fastboot(string serial, string action)
        {
            process = new Process();
            process.StartInfo.FileName = ".\\fastboot.exe";
            process.StartInfo.Arguments = serial == null ? action :
                "\"-s\" \"" + serial + "\" " + action;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            stdout = process.StandardOutput;
            stderr = process.StandardError;
        }

        public void Dispose()
        {
            process.Close();
            process = null;
        }

        ~Fastboot()
        {
            if (process != null)
                Dispose();
        }
    }
}
