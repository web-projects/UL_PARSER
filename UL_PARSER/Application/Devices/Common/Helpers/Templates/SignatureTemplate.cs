using System;
using System.Collections.Generic;
using System.Text;

namespace UL_PARSER.Devices.Common.Helpers.Templates
{
    public static class SignatureTemplate
    {
        public static readonly uint SignatureFile = 0xDFAA01;
        public static readonly uint HTMLKey = 0xDFAA02;
        public static readonly uint HTMLValue = 0xDFAA03;
        public static readonly uint HTMLResponse = 0xDFAA05;
        public static readonly string HTMLSignatureName = "signatureTwo";
    }
}
