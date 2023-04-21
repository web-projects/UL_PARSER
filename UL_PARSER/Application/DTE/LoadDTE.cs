using EnvDTE;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UL_PARSER.DTE
{
    public static class LoadDTE
    {
        public static System.Diagnostics.Process LoadIfNotRunning(string processName, string exe = null, string args = null, bool hidden = false)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);

            if (processes.Any()) 
            { 
                return processes.First(); 
            }
            
            if (exe == null)
            {
                exe = processName;
            }

            System.Diagnostics.Process p = null;
            
            if (!exe.ToLowerInvariant().EndsWith(".exe"))
            {
                exe += ".exe";
            }

            if (!exe.Contains(@":\"))
            {
                exe = Path.Combine(Environment.CurrentDirectory, exe);
            }

            try
            {
                ProcessStartInfo pi = new ProcessStartInfo(exe, args);

                if (hidden)
                {
                    pi.UseShellExecute = false;
                    pi.CreateNoWindow = true;
                    pi.WindowStyle = ProcessWindowStyle.Hidden;
                }
                p = System.Diagnostics.Process.Start(pi);

                Console.WriteLine("Starting " + p.ProcessName + "...");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Exception is: {ex.Message}");
            }

            return p;
        }

        /// <summary>
        ///     Attaches Visual Studio to the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="maxTries">The number of tries to get the process.</param>
        public static void Attach(this System.Diagnostics.Process process, int maxTries = 5)
        {
            // Reference visual studio core
            EnvDTE.DTE dte = null;
            int version = 9;
            while (version < 17 && dte == null)
            {
                try
                {
                    version++;
                    dte = (EnvDTE.DTE)Marshal2.GetActiveObject(String.Format("VisualStudio.DTE.{0}.0", version));
                }
                catch (COMException)
                {
                    //Console.WriteLine(String.Format("Visual studio {0} not found.", version));
                }
            }

            if (dte == null)
            {
                Console.WriteLine("No debugger found, nothing attached...");
                return;
            }

            // Try loop - visual studio may not respond the first time.
            // We also don't want it to stall the main thread
            new System.Threading.Thread(() =>
            {
                while (maxTries-- > 0)
                {
                    try
                    {
                        Processes processes = dte.Debugger.LocalProcesses;
                        foreach (EnvDTE.Process proc in processes)
                        {
                            try
                            {
                                if (proc.Name.Contains(process.ProcessName))
                                {
                                    proc.Attach();
                                    Console.WriteLine(String.Format("Attatched to process {0} successfully.", process.ProcessName));
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception is: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception is: {ex.Message}");
                    }

                    // Wait for debugger and application and debugger to find application
                    System.Threading.Thread.Sleep(1500);
                }
            }).Start();
        }
    }
}
