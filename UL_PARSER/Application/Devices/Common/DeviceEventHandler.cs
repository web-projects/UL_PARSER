using System.Threading.Tasks;
using static UL_PARSER.Devices.Common.Types;

namespace UL_PARSER.Devices.Common
{
    public delegate void DeviceLogHandler(LogLevel logLevel, string message);
    //public delegate void DeviceEventHandler(DeviceEvent deviceEvent, DeviceInformation deviceInformation);
    //public delegate Task ComPortEventHandler(PortEventType comPortEvent, string portNumber);
    public delegate void QueueEventOccured();
}
