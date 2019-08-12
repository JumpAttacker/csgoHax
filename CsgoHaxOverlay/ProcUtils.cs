using System;
using System.Diagnostics;

namespace CsgoHaxOverlay
{
    public class ProcUtils
    {
        #region PROPERTIES
        public Process Process { get; private set; }
        public IntPtr Handle { get; private set; }
        #endregion
        #region STATIC METHODS
        public static bool ProcessIsRunning(string name)
        {
            return Process.GetProcessesByName(name).Length > 0;
        }
        public static bool ProcessIsRunning(int id)
        {
            return Process.GetProcessById(id) != null;
        }
        public static IntPtr OpenHandleByProcessID(int id, WinApi.ProcessAccessFlags flags)
        {
            return WinApi.OpenProcess(flags, false, id);
        }
        public static IntPtr OpenHandleByProcessName(string name, WinApi.ProcessAccessFlags flags)
        {
            return OpenHandleByProcessID(Process.GetProcessesByName(name)[0].Id, flags);
        }
        public static IntPtr OpenHandleByProcess(Process process, WinApi.ProcessAccessFlags flags)
        {
            return OpenHandleByProcessID(process.Id, flags);
        }
        public static void CloseHandleToProcess(IntPtr handle)
        {
            try
            {
                WinApi.CloseHandle(handle);
            }
            catch (Exception)
            {
                Printer.PrintError("kek");
            }
            
        }
        #endregion
        #region CONSTRUCTOR/DESTRUCTOR
        public ProcUtils(string processName, WinApi.ProcessAccessFlags handleFlags)
            : this(Process.GetProcessesByName(processName)[0],handleFlags)
        { }
        public ProcUtils(int id, WinApi.ProcessAccessFlags handleFlags)
            : this(Process.GetProcessById(id), handleFlags)
        { }

        public ProcUtils(Process process, WinApi.ProcessAccessFlags handleFlags)
        {
            Process = process;
            Handle = OpenHandleByProcess(process, handleFlags);
        }
        ~ProcUtils()
        {
            CloseHandleToProcess(Handle);
        }
        #endregion
    }
}
