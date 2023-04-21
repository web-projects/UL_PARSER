namespace UL_PARSER.Devices.Common.Helpers.Templates
{
    /// <summary>
    /// This template encapsulates the whitelist hash from the device.
    /// </summary>
    public static class EFTemplate
    {
        public static readonly uint EFTemplateTag = 0xEF;
        public static readonly uint WhiteListHash = 0xDFDB09;
        public static readonly uint FirmwareVersion = 0xDF7F;
        public static readonly string ADKEVMKernel = "ADK_EMV_CT_Kern";
        public static readonly string ADKVault = "Vault";
        public static readonly string ADKOpenProtocol = "OpenProtocol";
        public static readonly string ADKSRED = "OS_SRED";
        public static readonly string ADKAppManager = "AppManager";
        public static readonly uint EMVLibraryName = 0xDF8106;
        public static readonly uint EMVLibraryVersion = 0xDF8107;
        public static readonly uint EMVKernelFrontend = 0xDF815A;
        public static readonly uint EMVKernelChecksum = 0xDF815B;
    }
}
