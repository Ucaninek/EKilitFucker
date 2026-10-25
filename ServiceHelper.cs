using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class ServiceHelper
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern IntPtr OpenService(IntPtr hSCManager, string serviceName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool ControlService(IntPtr hService, uint controlCode, ref SERVICE_STATUS lpServiceStatus);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CloseServiceHandle(IntPtr hSCObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

    [DllImport("kernel32.dll")]
    public static extern bool SetProcessInformation(IntPtr hProcess, int processInformationClass, ref bool processInformation, int processInformationSize);

    public const int ProcessInformationClass = 0x1D;

    public static void SetProcessCritical(IntPtr hProcess, bool isCritical)
    {
        bool processInformation = isCritical;
        SetProcessInformation(hProcess, ProcessInformationClass, ref processInformation, sizeof(bool));
    }

    private const uint SC_MANAGER_CONNECT = 0x0001;
    private const uint SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
    private const uint SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;
    private const uint SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020;
    private const uint SC_MANAGER_ALL_ACCESS = 0xF003F;

    private const uint SERVICE_STOP = 0x0010;
    private const uint SERVICE_CONTROL_STOP = 0x00000001;

    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS
    {
        public int dwServiceType;
        public int dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }

    public static bool StopService(string serviceName)
    {
        IntPtr scmHandle = OpenSCManager(null, null, SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS);
        if (scmHandle == IntPtr.Zero)
        {
            throw new Exception("Failed to open service control manager.");
        }

        IntPtr serviceHandle = OpenService(scmHandle, serviceName, SERVICE_STOP);
        if (serviceHandle == IntPtr.Zero)
        {
            CloseServiceHandle(scmHandle);
            throw new Exception("Failed to open service.");
        }

        SERVICE_STATUS serviceStatus = new SERVICE_STATUS();
        bool success = ControlService(serviceHandle, SERVICE_CONTROL_STOP, ref serviceStatus);

        if (!success)
        {
            // If the service cannot be stopped, try terminating the associated process
            Process[] processes = Process.GetProcessesByName(serviceName.Replace(".exe", ""));
            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    SetProcessCritical(process.Handle, false); //prevent a BSOD
                    process.Kill();
                }
                success = true;
            }
        }

        CloseServiceHandle(serviceHandle);
        CloseServiceHandle(scmHandle);

        return success;
    }
}
