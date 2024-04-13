using System;
using System.Runtime.InteropServices;

namespace PortProxyGUI.Native
{
    [Flags]
    internal enum ServiceRights : uint
    {
        SERVICE_QUERY_CONFIG = 0x0001,
        SERVICE_CHANGE_CONFIG = 0x0002,
        SERVICE_QUERY_STATUS = 0x0004,
        SERVICE_ENUMERATE_DEPENDENTS = 0x0008,
        SERVICE_START = 0x0010,
        SERVICE_STOP = 0x0020,
        SERVICE_PAUSE_CONTINUE = 0x0040,
        SERVICE_INTERROGATE = 0x0080,
        SERVICE_USER_DEFINED_CONTROL = 0x0100,

        SERVICE_ALL_ACCESS =
            SERVICE_QUERY_CONFIG
            | SERVICE_CHANGE_CONFIG
            | SERVICE_QUERY_STATUS
            | SERVICE_ENUMERATE_DEPENDENTS
            | SERVICE_START
            | SERVICE_STOP
            | SERVICE_PAUSE_CONTINUE
            | SERVICE_INTERROGATE
            | SERVICE_USER_DEFINED_CONTROL
    }
    
    internal enum ServiceState : int
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceStatus
    {
        public uint dwServiceType;
        public ServiceState dwCurrentState;
        public uint dwControlsAccepted;
        public uint dwWin32ExitCode;
        public uint dwServiceSpecificExitCode;
        public uint dwCheckPoint;
        public uint dwWaitHint;
    }
}
