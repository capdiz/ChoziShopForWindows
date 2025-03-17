using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Services
{
    public static class ServiceManager
    {
        private const string SERVICE_NAME = "ChoziShopWorkerService";

        public static bool IsServiceInstalled()
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == SERVICE_NAME);
        }

        public static bool IsServiceRunning()
        {
            using var sc = new ServiceController(SERVICE_NAME);
            return sc.Status == ServiceControllerStatus.Running;
        }

        public static void InstallService()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"create {SERVICE_NAME} binPath= \"{GetServicePath()}\" start=auto",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public static void StartService()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"start {SERVICE_NAME}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static string GetServicePath()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.Combine(Path.GetDirectoryName(assembly), "ChoziShopWorkerService.exe");
        }
    }
}
