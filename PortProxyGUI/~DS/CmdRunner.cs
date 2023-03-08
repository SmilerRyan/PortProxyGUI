﻿using System;
using System.Diagnostics;

namespace PortProxyGUI
{
    [Obsolete("The method of creating a new process is no longer used.")]
    public static class CmdRunner
    {
        public static string Execute(string cmd)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            proc.Start();

            proc.StandardInput.WriteLine($"{cmd} & exit");
            var output = proc.StandardOutput.ReadToEnd();

            return output;
        }
    }
}
