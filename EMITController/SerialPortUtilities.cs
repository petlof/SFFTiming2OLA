using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace EMITController
{
    public class SerialPortUtilities
    {
        public class COMPortInfo
        {
            public string PortName { get; set; }
            public string FriendlyName;

            public string Name => string.IsNullOrEmpty(FriendlyName) ? PortName : PortName + " - " + FriendlyName;


            public override string ToString()
            {
                return PortName + (!string.IsNullOrEmpty(FriendlyName) ? " - " + FriendlyName : "");
            }
        }

        public static List<COMPortInfo> GetSerialPorts()
        {
            string[] serials = System.IO.Ports.SerialPort.GetPortNames();
            if (LastSerialPortList == null || serials.Length != LastSerialPortList.Length
                || serials.Intersect(LastSerialPortList).Count() != serials.Length)
            {
                //Something changed since last lookup
                ComPortInformation.Clear();
                SetupComPortInformation();
            }

            return ComPortInformation;
        }
        static readonly string[] LastSerialPortList = null;
        private static readonly List<COMPortInfo> ComPortInformation = new List<COMPortInfo>();

        private static void SetupComPortInformation()
        {
            var portNames = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string s in portNames)
            {
                // s is like "COM14"
                COMPortInfo ci = new COMPortInfo
                {
                    PortName = s,
                    FriendlyName = ""
                };
                ComPortInformation.Add(ci);
            }

            string[] usbDevs = GetUsbcomDevices();
            foreach (string s in usbDevs)
            {
                // Name will be like "USB Bridge (COM14)"
                int start = s.IndexOf("(COM", StringComparison.Ordinal) + 1;
                if (start >= 0)
                {
                    int end = s.IndexOf(")", start + 3, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        // cname is like "COM14"
                        string cname = s.Substring(start, end - start);
                        foreach (COMPortInfo port in ComPortInformation)
                        {
                            if (port.PortName == cname)
                            {
                                port.FriendlyName = s;
                            }
                        }
                    }
                }
            }
        }

        static string[] GetUsbcomDevices()
        {
            var list = new List<string>();

            var searcher2 = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
            foreach (var o in searcher2.Get())
            {
                var mo2 = (ManagementObject) o;
                if (mo2 != null && mo2["Name"] != null)
                {
                    string name = mo2["Name"].ToString();
                    // Name will have a substring like "(COM12)" in it.
                    if (name.Contains("(COM"))
                    {
                        list.Add(name);
                    }
                }
            }

            // remove duplicates, sort alphabetically and convert to array
            string[] usbDevices = list.Distinct().OrderBy(s => s).ToArray();
            return usbDevices;
        }
    }
}
