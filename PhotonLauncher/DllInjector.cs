using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LuxonLauncher
{
    public static class DllInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_VM_READ = 0x0010,
            PROCESS_ALL_ACCESS = 0x001F0FFF
        }

        [Flags]
        private enum AllocationType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000
        }

        [Flags]
        private enum MemoryProtection : uint
        {
            ReadWrite = 0x04,
            ExecuteReadWrite = 0x40
        }

        private const uint MEM_RELEASE = 0x8000;

        /// <summary>
        /// Injects a DLL into a running process using CreateRemoteThread + LoadLibrary
        /// </summary>
        public static bool InjectIntoProcess(Process process, string dllPath)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            if (string.IsNullOrEmpty(dllPath))
                throw new ArgumentException("DLL path cannot be empty", nameof(dllPath));

            if (!File.Exists(dllPath))
                throw new FileNotFoundException($"DLL not found: {dllPath}");

            IntPtr hProcess = IntPtr.Zero;
            IntPtr allocatedMemory = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // Open the target process
                hProcess = OpenProcess(ProcessAccessFlags.PROCESS_CREATE_THREAD |
                                      ProcessAccessFlags.PROCESS_QUERY_INFORMATION |
                                      ProcessAccessFlags.PROCESS_VM_OPERATION |
                                      ProcessAccessFlags.PROCESS_VM_WRITE |
                                      ProcessAccessFlags.PROCESS_VM_READ,
                                      false, process.Id);

                if (hProcess == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Exception($"Failed to open process (Error: {error}). Try running as Administrator.");
                }

                // Get address of LoadLibraryA
                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (loadLibraryAddr == IntPtr.Zero)
                {
                    throw new Exception("Failed to get LoadLibraryA address");
                }

                // Allocate memory in target process for DLL path
                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
                allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)(dllPathBytes.Length + 1),
                                                  AllocationType.Commit | AllocationType.Reserve,
                                                  MemoryProtection.ReadWrite);

                if (allocatedMemory == IntPtr.Zero)
                {
                    throw new Exception("Failed to allocate memory in target process");
                }

                // Write DLL path to allocated memory
                if (!WriteProcessMemory(hProcess, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out _))
                {
                    throw new Exception("Failed to write DLL path to target process memory");
                }

                // Create remote thread that calls LoadLibraryA with our DLL path
                hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, IntPtr.Zero);

                if (hThread == IntPtr.Zero)
                {
                    throw new Exception("Failed to create remote thread");
                }

                // Wait for the thread to complete (max 10 seconds)
                WaitForSingleObject(hThread, 10000);

                // Check if LoadLibrary succeeded (exit code 0 means failure)
                if (GetExitCodeThread(hThread, out uint exitCode) && exitCode == 0)
                {
                    throw new Exception("LoadLibrary returned 0 - the DLL failed to load. Check if the DLL has all dependencies.");
                }

                return true;
            }
            finally
            {
                // Cleanup
                if (hThread != IntPtr.Zero)
                    CloseHandle(hThread);

                // Free the allocated memory in the target process
                if (allocatedMemory != IntPtr.Zero && hProcess != IntPtr.Zero)
                    VirtualFreeEx(hProcess, allocatedMemory, 0, MEM_RELEASE);

                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// Launches a process and injects DLL using native Windows API
        /// </summary>
        public static Process LaunchAndInject(string executablePath, string arguments, string dllPath)
        {
            if (!File.Exists(executablePath))
                throw new FileNotFoundException($"Executable not found: {executablePath}");

            if (!File.Exists(dllPath))
                throw new FileNotFoundException($"DLL not found: {dllPath}");

            // First, launch the game normally
            var process = new Process();
            process.StartInfo.FileName = executablePath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = true;
            process.Start();

            // Give the process time to initialize
            System.Threading.Thread.Sleep(1500);

            // Inject the DLL
            if (InjectIntoProcess(process, dllPath))
            {
                return process;
            }

            throw new Exception("Failed to inject DLL after process launch");
        }
    }
}