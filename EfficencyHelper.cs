using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class EfficiencyHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_POWER_THROTTLING_STATE
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }

    enum PROCESS_INFORMATION_CLASS
    {
        ProcessPowerThrottling = 5
    }

    const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
    const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetProcessInformation(
        IntPtr hProcess,
        PROCESS_INFORMATION_CLASS processInformationClass,
        ref PROCESS_POWER_THROTTLING_STATE processInformation,
        uint processInformationSize);
		
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool GetProcessInformation(
		IntPtr hProcess,
		PROCESS_INFORMATION_CLASS processInformationClass,
		ref PROCESS_POWER_THROTTLING_STATE processInformation,
		uint processInformationSize);

    public static void EnableEfficiencyMode(Process process)
    {
        try
        {
            var state = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED
            };

            SetProcessInformation(
                process.Handle,
                PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                ref state,
                (uint)Marshal.SizeOf(typeof(PROCESS_POWER_THROTTLING_STATE))
            );

            process.PriorityClass = ProcessPriorityClass.BelowNormal;

            Console.WriteLine($"Efficiency Mode enabled for {process.ProcessName}");
        }
        catch
        {
            // Access denied or protected process
        }
    }
	
	public static bool IsEfficiencyModeEnabled(Process process)
	{
		try
		{
			var state = new PROCESS_POWER_THROTTLING_STATE();

			bool success = GetProcessInformation(
				process.Handle,
				PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
				ref state,
				(uint)Marshal.SizeOf(typeof(PROCESS_POWER_THROTTLING_STATE))
			);

			if (!success)
				return false;

			return (state.StateMask & PROCESS_POWER_THROTTLING_EXECUTION_SPEED) != 0;
		}
		catch
		{
			return false;
		}
	}

}
