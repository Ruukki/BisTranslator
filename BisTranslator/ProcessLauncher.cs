using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator
{
    public static class ProcessLauncher
    {
        public static void StartExecutable(string exePath)
        {
            try
            {
                if (CheckIfRunning("FFoverlay"))
                {
                    return;
                }
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true, // Set to true if the executable requires elevated privileges or uses file associations
                };
                Process process = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static bool CheckIfRunning(string processName)
        {
            // Get the list of all processes with the specified name
            Process[] processes = Process.GetProcessesByName(processName);

            // Check if any process with the specified name is found
            return processes.Length > 0;
        }
    }
}
