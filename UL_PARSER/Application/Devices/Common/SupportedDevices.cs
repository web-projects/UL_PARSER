namespace UL_PARSER.Devices.Common
{
    public static class SupportedDevices
    {
        // ---------------------------------------------------------------------
        // DO NOT CHANGE THESE CONSTANTS UNLESS YOU KNOW WHAT YOU ARE DOING!
        // ---------------------------------------------------------------------
        public const string Verifone_M400_Device = "Verifone-M400";
        public const string Verifone_M400BT_Device = "Verifone-M400WIFI/BT";
        public const string Verifone_P200_Device = "Verifone-P200";
        public const string Verifone_P200Plus_Device = "Verifone-P200Plus";
        public const string Verifone_P400_Device = "Verifone-P400Plus";
        public const string Verifone_UX300_Device = "Verifone-UX300";
        public const string Verifone_UX301_Device = "Verifone-UX301";
        public const string MagTek_ExcellaImageSafe_Device = "MagTek-ExcellaImageSafe";
        public const string Simulator_Device_Model = "Simulator-UI";
        public static readonly string[] VERIFONE_ATTENDED_DEVICES = {
            Verifone_M400_Device,
            Verifone_M400BT_Device,
            Verifone_P200_Device,
            Verifone_P200Plus_Device,
            Verifone_P400_Device
        };
        public static readonly string[] VERIFONE_UNATTENDED_DEVICES = {
            Verifone_UX300_Device,
            Verifone_UX301_Device
        };

        public const string Simulator_Device = "Simulator Device";
        public const string Null_Device = "NoDevice";

        public const string VerifoneManufacturerId = "Verifone";
        public const string IdTechManufacturerId = "IdTech";
        public const string IngenicoManufacturerId = "Ingenico";
        public const string SimulatorManufacturerId = "Simulator";
        public const string NullDeviceManufacturerId = "NoDevice";
        public const string MagTekManufacturerId = "MagTek";

        public const string VerifoneDeviceType = "Verifone Device";
        public const string IdTechDeviceType = "IdTech Device";
        public const string SimulatorDeviceType = Simulator_Device;
        public const string MockDeviceType = "Mock Device";
        public const string NullDeviceType = "NoDevice";
        public const string MagTekDeviceType = "MagTek Device";
    }

}
