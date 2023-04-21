using UL_PARSER.Devices.Common.Helpers;
using static UL_PARSER.Devices.Common.SupportedDevices;

namespace UL_PARSER.Devices.Common
{
    public enum ManufacturerId
    {
        [StringValue("None")]
        None = 0,
        [StringValue(IdTechManufacturerId)]
        IdTech = 1,
        [StringValue(IngenicoManufacturerId)]
        Ingenico = 2,
        [StringValue(VerifoneManufacturerId)]
        Verifone = 3,
        [StringValue(MagTekManufacturerId)]
        MagTek = 4,
    }

    public enum DeviceType
    {
        [StringValue(VerifoneDeviceType)]
        Verifone = 1,
        [StringValue(IdTechDeviceType)]
        IdTech = 2,
        [StringValue(SimulatorDeviceType)]
        Simulator = 3,
        [StringValue(MockDeviceType)]
        Mock = 4,
        [StringValue(NullDeviceType)]
        NoDevice = 5,
        [StringValue(MagTekDeviceType)]
        MagTek = 6,
    }

    public enum TransactionConfigurations
    {
        [StringValue("ContactMSR")]
        ContactMSR = 1,
        [StringValue("ContactlessMSR")]
        ContactlessMSR = 2,
        [StringValue("ContactEMV")]
        ContactEMV = 3,
        [StringValue("ContactlessEMV")]
        ContactlessEMV = 4,
        [StringValue("ACH")]
        ACH = 5
    }

    public enum DeviceEvent
    {
        [StringValue("None")]
        None,
        [StringValue("Device plugged")]
        DevicePlugged,
        [StringValue("Device unplugged")]
        DeviceUnplugged,
        [StringValue("Payment cancelled by customer.")]
        CancelKeyPressed,
        [StringValue("ZIP code entered")]
        ZIPCodeEntered,
        [StringValue("PIN entered")]
        PINEntered,
        [StringValue("Device has timed out due to inactivity")]
        RequestTimeout,
        [StringValue("Payment could not be processed. Payment cancelled by user.")]
        CancellationRequest,
        [StringValue("Key Pressed")]
        KeyPressed,
        [StringValue("Device Not Found")]
        DeviceNotFound,
        [StringValue("Unable to Process Request")]
        UnableToProcessRequest,
        [StringValue("Device discovered")]
        DeviceDiscovered,
        [StringValue("Device card Removed")]
        DeviceCardRemoved,
        [StringValue("PIN Bypass Key Pressed")]
        PinBypassKeyPressed,
        [StringValue("Network Connection Failure")]
        NetworkConnectionFailure
    }

    public enum DeviceDiscovery
    {
        [StringValue("No device detected")]
        NoDeviceAvailable = 1,
        [StringValue("Device not specified")]
        NoDeviceSpecified = 2,
        [StringValue("No Device matching")]
        NoDeviceMatched = 3
    }

    public enum DalResponses
    {
        [StringValue("MESSAGE DISPLAYED")]
        AdaMessageDisplayed = 1,
        [StringValue("DEVICE SET TO IDLE")]
        DeviceSetToIdle = 2
    }

    public enum EMVTransactionType
    {
        [StringValue("Sale")]
        Sale = 0x00,
        [StringValue("Cash Advance")]
        CashAdvance = 0x01,
        [StringValue("Sale with cashback")]
        SaleCashback = 0x9,
        [StringValue("Refund")]
        Refund = 0x20,
        [StringValue("Balance Inquiry")]
        BalanceInquire = 0x30,
        [StringValue("Reservation ")]
        Reservation = 0x31,
        [StringValue("None")]
        None = 0xFE
    }

}
