﻿using Microsoft.Win32;
using PortProxyGUI.Data;
using PortProxyGUI.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace PortProxyGUI.Utils {
    public static class PortPorxyUtil {
        internal enum ScmRights : uint {
            SC_MANAGER_CONNECT = 0x0001,
            SC_MANAGER_CREATE_SERVICE = 0x0002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x0004,
            SC_MANAGER_LOCK = 0x0008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x0010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020,
            SC_MANAGER_ALL_ACCESS = 0x000F0000 | SC_MANAGER_CONNECT | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_LOCK | SC_MANAGER_QUERY_LOCK_STATUS | SC_MANAGER_MODIFY_BOOT_CONFIG
        }
        internal static readonly string ServiceName = "iphlpsvc";
        internal static readonly string ServiceFriendlyName = "IP Helper";
        private static InvalidOperationException InvalidPortProxyType(string type) => new($"Invalid port proxy type ({type}).");
        private static readonly string[] ProxyTypes = new[] { "v4tov4", "v4tov6", "v6tov4", "v6tov6" };
        private static string GetKeyName(string type) { return $@"SYSTEM\CurrentControlSet\Services\PortProxy\{type}\tcp"; }
        public static Rule[] GetProxies(){
            var ruleList = new List<Rule>();
            foreach (var type in ProxyTypes) {
                var keyName = GetKeyName(type);
                var key = Registry.LocalMachine.OpenSubKey(keyName);
                if (key is not null) {
                    foreach (var name in key.GetValueNames()) {
                        var listenParts = name.Split('/');
                        var listenOn = listenParts[0];
                        if (!int.TryParse(listenParts[1], out var listenPort)) continue;
                        var connectParts = key.GetValue(name).ToString().Split('/');
                        var connectTo = connectParts[0];
                        if (!int.TryParse(connectParts[1], out var connectPort)) continue;
                        ruleList.Add(new Rule {
                            Type = type,
                            ListenOn = listenOn,
                            ListenPort = listenPort,
                            ConnectTo = connectTo,
                            ConnectPort = connectPort,
                            //Comment = comment,
                            //Group = group,
                        });
                    }
                }
            }
            return ruleList.ToArray();
        }
        public static void AddOrUpdateProxy(Rule rule) {
            if (!ProxyTypes.Contains(rule.Type)) throw InvalidPortProxyType(rule.Type);
            var keyName = GetKeyName(rule.Type);
            var key = Registry.LocalMachine.OpenSubKey(keyName, true);
            var name = $"{rule.ListenOn}/{rule.ListenPort}";
            var value = $"{rule.ConnectTo}/{rule.ConnectPort}";
            if (key is null) Registry.LocalMachine.CreateSubKey(keyName);
            key = Registry.LocalMachine.OpenSubKey(keyName, true);
            key?.SetValue(name, value);
        }
        public static void DeleteProxy(Rule rule) {
            if (!ProxyTypes.Contains(rule.Type)) throw InvalidPortProxyType(rule.Type);
            var keyName = GetKeyName(rule.Type);
            var key = Registry.LocalMachine.OpenSubKey(keyName, true);
            var name = $"{rule.ListenOn}/{rule.ListenPort}";
            try { key?.DeleteValue(name); } catch { }
        }
        [Flags]
        internal enum GenericRights : uint {
            GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000, GENERIC_EXECUTE = 0x20000000, GENERIC_ALL = 0x10000000,
        }
        public static bool IsServiceRunning() {
            var hManager = NativeMethods.OpenSCManager(null, null, (uint)GenericRights.GENERIC_READ);
            if (hManager == IntPtr.Zero) throw new InvalidOperationException("Open SC Manager failed.");
            var hService = NativeMethods.OpenService(hManager, ServiceName, ServiceRights.SERVICE_QUERY_STATUS);
            if (hService == IntPtr.Zero) {
                NativeMethods.CloseServiceHandle(hManager);
                throw new InvalidOperationException($"Open Service ({ServiceName}) failed.");
            }
            var status = new ServiceStatus();
            NativeMethods.QueryServiceStatus(hService, ref status);
            NativeMethods.CloseServiceHandle(hService);
            NativeMethods.CloseServiceHandle(hManager);
            return status.dwCurrentState == ServiceState.SERVICE_RUNNING;
        }
        public static void StartService() {
            var hManager = NativeMethods.OpenSCManager(null, null, (uint)GenericRights.GENERIC_READ | (uint)ScmRights.SC_MANAGER_CONNECT);
            if (hManager == IntPtr.Zero) throw new InvalidOperationException("Open SC Manager failed.");
            var hService = NativeMethods.OpenService(hManager, ServiceName, ServiceRights.SERVICE_START);
            if (hService == IntPtr.Zero) {
                NativeMethods.CloseServiceHandle(hManager);
                throw new InvalidOperationException($"Open Service ({ServiceName}) failed.");
            }
            NativeMethods.StartService(hService, 0, null);
            NativeMethods.CloseServiceHandle(hService);
            NativeMethods.CloseServiceHandle(hManager);
        }
        public static void ParamChange() {
            var hManager = NativeMethods.OpenSCManager(null, null, (uint)GenericRights.GENERIC_READ);
            if (hManager == IntPtr.Zero) throw new InvalidOperationException("Open SC Manager failed.");
            var hService = NativeMethods.OpenService(hManager, ServiceName, ServiceRights.SERVICE_PAUSE_CONTINUE);
            if (hService == IntPtr.Zero) {
                NativeMethods.CloseServiceHandle(hManager);
                throw new InvalidOperationException($"Open Service ({ServiceName}) failed.");
            }
            var status = new ServiceStatus();
            NativeMethods.ControlService(hService, ServiceControls.SERVICE_CONTROL_PARAMCHANGE, ref status);
            NativeMethods.CloseServiceHandle(hService);
            NativeMethods.CloseServiceHandle(hManager);
        }
    }
}
