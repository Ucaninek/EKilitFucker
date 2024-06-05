using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Task = System.Threading.Tasks.Task;

namespace EKilitFucker
{
    public partial class Form1 : Form
    {

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessInformation(IntPtr hProcess, int processInformationClass, ref bool processInformation, int processInformationSize);

        public const int ProcessInformationClass = 0x1D;

        static bool stopThread = false;

        public static void SetProcessCritical(IntPtr hProcess, bool isCritical)
        {
            bool processInformation = isCritical;
            SetProcessInformation(hProcess, ProcessInformationClass, ref processInformation, sizeof(bool));
        }

        public Form1()
        {
            InitializeComponent();
        }

        private async void RemoveEKilit()
        {
            try
            {
                Log("Starting removal :o");
                Log("Locating EKilit Service..");
                await System.Threading.Tasks.Task.Run(LocateAndStopEKilit).ContinueWith((t) =>
                {
                    if (t.IsFaulted) Log(t.Exception.Message);
                });
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private void disableService(string serviceName)
        {
            try
            {
                // Open the Windows Registry key for the specified service
                string keyPath = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        // Set the "Start" value to 4 (which represents "Disabled" start type)
                        key.SetValue("Start", 4);
                    }
                    else
                    {
                        Log($"Service '{serviceName}' not found in the Windows Registry.");
                    }
                }

                // Display the updated start type
                Log($"Updated Start Type: None");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }


        private void dumpSvcInfo(string serviceName)
        {
            try
            {
                // Get the service controller for the specified service name
                ServiceController sc = new ServiceController(serviceName);

                // Display service information
                Log($"Service Name: {sc.ServiceName}");
                Log($"Display Name: {sc.DisplayName}");

                Log($"Service Type: {sc.ServiceType}");
                Log($"Service Status: {sc.Status}");
                Log($"Start Type: {sc.StartType}");
                Log($"Service Path: {sc.ServiceName}");
                Log($"Can Pause and Continue: {sc.CanPauseAndContinue}");
                Log($"Can Shutdown: {sc.CanShutdown}");
                Log($"Can Stop: {sc.CanStop}");

                // Display service dependencies
                ServiceController[] dependencies = sc.DependentServices;
                if (dependencies.Length > 0)
                {
                    Log("Dependencies:");
                    foreach (ServiceController dependency in dependencies)
                    {
                        Log($"- {dependency.DisplayName}");
                    }
                }
                else
                {
                    Log("No dependencies.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        // ...

        private async void LocateAndStopEKilit()
        {
            try
            {
                string serviceName = GetEKilitServiceName();
                string EKilitAlias = serviceName.Substring(3, serviceName.Length - 7); //remove the prefix "svc" and suffix ".exe"
                string phaseOnePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "phase_one_completed");
                bool isPhaseOneCompleted = File.Exists(phaseOnePath);

                if (!isPhaseOneCompleted)
                {
                    Log($"Found EKilit Alias {EKilitAlias}");
                    //dumpSvcInfo(serviceName);
                    Log("Disabling EKilit Services..");

                    disableService(serviceName);
                    disableService("WinDelay");
                    SetUAC(false);
                    SetToRunOnNextLogon();

                    Log("EKilit Service is Disabled. Signing out in 5 seconds to safely delete leftovers.");
                    Log("## EKilitFucker will run on next logon to ensure the service is removed completely. ##");
                    File.Create(phaseOnePath).Close();

                    await Task.Delay(5000);
                    Process.Start("shutdown", "/l /f");
                }
                else
                {
                    Log("Starting Phase 2");
                    File.Delete(phaseOnePath);
                    SetUAC(true);
                    RemoveTaskSchedulerEntry();

                    Log("Creating thread to continuously kill EKilit processes");
                    Thread thread = new Thread(() => { ContinuouslyKillEKilitProcesses(EKilitAlias); });
                    thread.Start();

                    Log("Unregistering EKilit Services..");

                    DeleteService(serviceName);

                    StopService("WinDelay");
                    DeleteService("WinDelay");

                    await Task.Delay(1000);

                    Log("Removing leftover EKilit files..");
                    await DeleteEKilitFiles(serviceName);

                    Log("## EKilit is successfully removed :D ##");
                }
            }
            catch(Exception ex)
            {
                Log(ex.Message);
            }
        }

        private void SetUAC(bool enabled)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (key != null)
                    {
                        key.SetValue("EnableLUA", enabled ? 1 : 0);
                    }
                }

                Log(String.Format("UAC {0} successfully", enabled ? "enabled" : "disabled"));
            }
            catch (Exception ex)
            {
                Log($"Failed to disable UAC: {ex.Message}");
            }
        }

        private void RemoveTaskSchedulerEntry()
        {
            try
            {
                using (TaskService taskService = new TaskService())
                {
                    taskService.RootFolder.DeleteTask("EKilitFucker", false);
                }

                Log("Task scheduler entry removed.");
            }
            catch (Exception ex)
            {
                Log($"Failed to remove task scheduler entry: {ex.Message}");
            }
        }

        private void SetToRunOnNextLogon()
        {
            try
            {
                // Create a new task scheduler entry
                using (TaskService taskService = new TaskService())
                {
                    TaskDefinition taskDefinition = taskService.NewTask();
                    taskDefinition.RegistrationInfo.Description = "Run EKilitFucker on next logon";
                    taskDefinition.Triggers.Add(new LogonTrigger());
                    taskDefinition.Actions.Add(new ExecAction(Application.ExecutablePath));
                    taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

                    taskService.RootFolder.RegisterTaskDefinition("EKilitFucker", taskDefinition);
                }

                Log("Scheduled to run on next logon.");
            }
            catch (Exception ex)
            {
                Log($"Failed to schedule task: {ex.Message}");
            }
        }

        private async Task DeleteEKilitFiles(string serviceName)
        {
            List<string> SysWOW64Files = new List<string>() { "msdatacomp86.dll", "msdatacomp64.dll", "winrcsvc.dll.rtu", "windelayer.exe", "winrccomper.dll.rtu", "winresconfig.bin", "liblconfdata.rst", "libllistdata.rst" };

            List<string> WindirFiles = new List<string>() { "winsetup.dll.man" };

            string imagePath = GetImagePathByServiceName(serviceName);
            string? folderPath = Path.GetDirectoryName(imagePath); // get the folder path from the image path
            string WindowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            int tries = 0;
            bool retry = true;

            void handleRetry()
            {
                retry = true;
                tries++;
            }

            while (retry && tries < 5)
            {
                await Task.Delay(100);
                retry = false;
                foreach (string WindirFile in WindirFiles)
                {
                    string filePath = Path.Combine(WindowsPath, WindirFile);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            Log($"Deleted File {Path.GetFileName(Path.GetFileName(filePath))}");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to delete {WindirFile}: {ex.Message}");
                            handleRetry();
                        }
                    }
                }


                string SysWOW64Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
                foreach (string SysWOW64File in SysWOW64Files)
                {
                    string filePath = Path.Combine(SysWOW64Path, SysWOW64File);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            Log($"Deleted File {Path.GetFileName(Path.GetFileName(filePath))}");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to delete {SysWOW64File}: {ex.Message}");
                            handleRetry();
                        }
                    }
                }

                if (Directory.Exists(folderPath))
                {
                    string[] exeFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories); // get all the .exe files in the folder and subfolders
                    foreach (string exeFile in exeFiles)
                    {
                        try
                        {
                            // perform actions on each exe file
                            //Log($"Processing {exeFile}...");
                            //string newFileName = Path.ChangeExtension(exeFile, ".exe.femboy");

                            // Modify security permissions
                            //FileInfo fileInfo = new FileInfo(exeFile);
                            //FileSecurity fileSecurity = fileInfo.GetAccessControl();
                            //fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                            //fileInfo.SetAccessControl(fileSecurity);
                            //Log($"Modified security permissions for {Path.GetFileName(exeFile)}");

                            File.Delete(exeFile);
                            Log($"Deleted File {Path.GetFileName(exeFile)}");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to delete {Path.GetFileName(exeFile)}: {ex.Message}");
                            handleRetry();
                        }
                    }

                    string[] dirFiles = Directory.GetDirectories(folderPath);

                    foreach (string dirFile in dirFiles)
                    {
                        try
                        {
                            Directory.Delete(dirFile, true);
                            Log($"Deleted Directory {dirFile}");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to delete {dirFile}: {ex.Message}");
                            handleRetry();
                        }
                    }
                }
                if (tries > 0)
                {
                    await Task.Delay(500);
                    Log($"retry #{tries + 1}");
                    await Task.Delay(500);
                }
            }
        }

        private void ContinuouslyKillEKilitProcesses(string eKilitAlias)
        {
            List<string> EKilitProcessNames = new List<string> { $"svc{eKilitAlias}", $"cli{eKilitAlias}", eKilitAlias, "TeacherMan", "rollcall", "mcnmain", "mcn_client" };
            while (stopThread == false)
            {
                Process[] processes = Process.GetProcesses();
                foreach (string processName in EKilitProcessNames)
                {
                    foreach (Process process in processes)
                    {
                        if (process.ProcessName == processName)
                        {
                            try
                            {
                                SetProcessCritical(process.Handle, false);
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = "taskkill",
                                    Arguments = $"/f /im {processName}.exe",
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                })?.WaitForExit();
                                Log($"Killed process ${processName}");
                            }
                            catch (Exception ex)
                            {
                                Log($"Failed to kill {process.ProcessName} process: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void DeleteService(string serviceName)
        {
            try
            {
                // Open the Windows Registry key for the specified service
                string keyPath = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        // Delete the registry key for the service
                        Registry.LocalMachine.DeleteSubKeyTree(keyPath);
                        Log($"Deleted service '{serviceName}' from the Windows Registry.");
                    }
                    else
                    {
                        Log($"Service '{serviceName}' not found in the Windows Registry.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private string? GetImagePathByServiceName(string serviceName)
        {
            string keyPath = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
            string? imagePath = null;

            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    imagePath = key.GetValue("ImagePath") as string;
                }
            }

            return imagePath;
        }

        private string GetEKilitServiceName() //returns the E-Kilit service Image Path
        {
            string serviceName = string.Empty;

            // Get the list of all services
            ServiceController[] services = ServiceController.GetServices();

            // Loop through the services and find the one with the description "Sound Normalization Service"
            bool serviceFound = false;
            foreach (ServiceController service in services)
            {
                if (service.DisplayName == "Sound Normalization Service")
                {
                    serviceFound = true;
                    serviceName = service.ServiceName;
                    break;
                }
            }

            if (!serviceFound)
            {
                //Log("EKilit Service not found, falling back to default values.");
                //serviceName = "svcvcpkg_64.exe";
                throw new Exception("Service not found :(");
            }

            return serviceName;
        }

        private void Log(string message)
        {
            if (TB_Logs.InvokeRequired)
            {
                TB_Logs.Invoke((MethodInvoker)delegate
                {
                    TB_Logs.Text += message + Environment.NewLine;
                    TB_Logs.SelectionStart = TB_Logs.Text.Length;
                    TB_Logs.ScrollToCaret();
                });
            }
            else
            {
                TB_Logs.Text += message + Environment.NewLine;
                TB_Logs.SelectionStart = TB_Logs.Text.Length;
                TB_Logs.ScrollToCaret();
            }
        }

        private void LL_SelfAd_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = "/c start https://zemi.space"
            };
            Process.Start(psi);
        }

        private void B_Start_Click(object sender, EventArgs e)
        {
            RemoveEKilit();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string phaseOnePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "phase_one_completed");
            bool isPhaseOneCompleted = File.Exists(phaseOnePath);

            if (isPhaseOneCompleted)
            {
                B_Start.PerformClick();
            }
        }

        private void StopService(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);
            try
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    Log($"Stopped service '{serviceName}'");
                }
                else
                {
                    Log($"Service '{serviceName}' is not running.");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to stop service '{serviceName}': {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopThread = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task task = Task.Run(async () =>
            {
                while(true)
                {
                    ChangeTitle();
                    await Task.Delay(1000);
                }
            });
        }

        private void ChangeTitle()
        {
            string[] variant_one = { "É", "Ḱ", "í", "ĺ", "í", "t́", "F́", "ú", "ć", "ḱ", "é", "ŕ" };
            string[] variant_two = { "E̤", "K̤", "i̤", "l̤'", "i̤", "t̤", "F̤", "ṳ", "c̤", "k̤", "e̤", "r̤" };
            string[] variant_three = { "Ё", "K̈", "ï", "l̈", "ï", "ẗ", "F̈", "ü", "c̈", "k̈", "ë", "r̈" };
            string[] variant_four = { "Ë̤", "K̤̈", "ï̤", "l̤̈", "ï̤", "ẗ̤", "F̤̈", "ṳ̈", "c̤̈", "k̤̈", "ë̤", "r̤̈" };

            Random random = new Random();
            string result = "";

            for (int i = 0; i < "EKilitFucker".Length; i++)
            {
                int randomNumber = random.Next(1, 5);

                switch (randomNumber)
                {
                    case 1:
                        result += variant_one[i];
                        break;
                    case 2:
                        result += variant_two[i];
                        break;
                    case 3:
                        result += variant_three[i];
                        break;
                    case 4:
                        result += variant_four[i];
                        break;
                }
            }

            this.Invoke((MethodInvoker)delegate
            {
                this.Text = result;
            });
        }
    }
}
