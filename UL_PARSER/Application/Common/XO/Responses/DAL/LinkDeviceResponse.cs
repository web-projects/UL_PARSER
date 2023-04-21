using System;
using System.Collections.Generic;
using System.Text;
using UL_PARSER.Common.XO.Common.DAL;

namespace UL_PARSER.Common.XO.Responses.DAL
{
    public class LinkDeviceResponse : LinkFutureCompatibility
    {
        public List<LinkErrorValue> Errors { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string FirmwareVersion { get; set; }
        public string TerminalId { get; set; }
        public string SerialNumber { get; set; }
        public string Port { get; set; }
        public List<string> Features { get; set; }
        public List<string> Configurations { get; set; }
        //CardWorkflowControls only used when request Action = 'DALStatus'; can be null
        public LinkCardWorkflowControls CardWorkflowControls { get; set; }
        public LinkDevicePowerOnNotification PowerOnNotification { get; set; }
    }
}
