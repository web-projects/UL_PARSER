using System;
using System.Collections.Generic;
using UL_PARSER.Common.XO.Requests.DAL;
using UL_PARSER.Common.XO.Responses.DAL;
using UL_PARSER.Devices.Verifone.VIPA;

namespace UL_PARSER.Devices.Verifone.Helpers
{
    public class DeviceInfoObject
    {
        public VipaSW1SW2Codes Status { get; set; }
        public List<string> TransactionConfigurations { get; set; } = new List<string> { Common.TransactionConfigurations.ContactMSR.GetStringValue() };
        public LinkDeviceResponse LinkDeviceResponse { get; set; }
        public LinkDALRequestIPA5Object LinkDALRequestIPA5Object { get; set; }
    }
}
