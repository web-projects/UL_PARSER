using System;
using System.Collections.Generic;
using System.Text;
using UL_PARSER.Devices.Common.SerialPort;

namespace UL_PARSER.Devices.Common
{
    public class DeviceConfig
    {
        public enum DeviceConnectionType
        {
            Unknown,
            Comm,
            USB,
            TCPIP
        }

        public DeviceConnectionType ConnectionType { get; set; }
        public string Connection { get; set; }
        public int? Speed { get; set; }
        public int? Timeout { get; set; }
        public bool Valid { get; set; } = false;
        public SerialDeviceConfig SerialConfig { get; private set; }
        //public SupportedTransactions SupportedTransactions { get; set; }
        public DeviceConfig SetSerialDeviceConfig(in SerialDeviceConfig serialDeviceConfig)
        {
            SerialConfig = serialDeviceConfig;
            return this;
        }
    }
}
