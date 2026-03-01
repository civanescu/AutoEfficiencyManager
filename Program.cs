using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    static Dictionary<int, TimeSpan> previousCpuTimes = new();
	static HashSet<string> excludedProcesses = new(StringComparer.OrdinalIgnoreCase);
	static HashSet<int> alreadyThrottled = new();

    static void Main()
    {
		LoadExclusions();
        Console.WriteLine("Auto Efficiency Manager started...");
        Console.WriteLine("Monitoring processes...\n");

        while (true)
        {
            MonitorProcesses();
            Thread.Sleep(1000);
        }
    }
	
	static void LoadExclusions()
	{
		string filePath = "exclusions.txt";
		if (!File.Exists(filePath))
		{
			File.WriteAllText(filePath, "explorer\n");
		}
		
		foreach (var line in File.ReadAllLines(filePath))
		{
			if (!string.IsNullOrWhiteSpace(line))
				excludedProcesses.Add(line.Trim());
		}
		
		Console.WriteLine("Loaded exclusions:");
		foreach (var item in excludedProcesses) Console.WriteLine($" - {item}");
	}

    static void MonitorProcesses()
    {
		// Optional — Clean Exited Processes
		alreadyThrottled.RemoveWhere(id => 
		{
			try { Process.GetProcessById(id); return false; }
			catch { return true; }
		});
		
		
        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
				if (excludedProcesses.Contains(process.ProcessName)) continue;
				
                if (process.HasExited)
                    continue;

                if (IsProtectedProcess(process))
                    continue;

                if (!previousCpuTimes.ContainsKey(process.Id))
                {
                    previousCpuTimes[process.Id] = process.TotalProcessorTime;
                    continue;
                }

                var oldCpuTime = previousCpuTimes[process.Id];
                var newCpuTime = process.TotalProcessorTime;

                previousCpuTimes[process.Id] = newCpuTime;

                double cpuUsedMs = (newCpuTime - oldCpuTime).TotalMilliseconds;
                double cpuPercent = cpuUsedMs / (1000 * Environment.ProcessorCount) * 100;

                if (cpuPercent > 20)
                {
					if (!alreadyThrottled.Contains(process.Id) && 
						!EfficiencyHelper.IsEfficiencyModeEnabled(process))
					{
						Console.WriteLine($"High CPU detected: {process.ProcessName} ({cpuPercent:F2}%)");

						EfficiencyHelper.EnableEfficiencyMode(process);
						alreadyThrottled.Add(process.Id);
					}
                }
            }
            catch
            {
                // Ignore inaccessible processes
            }
        }
    }

    static bool IsProtectedProcess(Process process)
    {
        string[] protectedNames =
        {
            "System",
            "Idle",
            "explorer",
            "winlogon",
            "csrss",
            "services"
        };

        return protectedNames.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase);
    }
}
