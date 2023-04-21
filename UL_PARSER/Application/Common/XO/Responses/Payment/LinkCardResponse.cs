using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace UL_PARSER.Common.XO.Responses.Payment
{
    public class LinkCardResponse : LinkFutureCompatibility
    {
        public string AuthorizationCode { get; set; }
        public string LeadingMaskedPAN { get; set; }
        public string TrailingMaskedPAN { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string CardholderName { get; set; }
        public bool SignatureRequested { get; set; }
        public string TenderType { get; set; }
        public bool DebitCard { get; set; }
        public LinkCardPaymentResponseEntryMode EntryMode { get; set; }
        public string AVSStatus { get; set; }
        public string CommercialCard { get; set; }
        public string CardIdentifier { get; set; }
        public Guid? HeldCardDataID { get; set; }
        //public LinkOnlineApproval OnlineApproval { get; set; }
        public string CardSource { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum LinkCardPaymentResponseEntryMode
    {
        [Description("None")]
        None,

        [Description("Swipe")]
        Swipe,

        [Description("EMV Chip Read")]
        EMV,

        [Description("Manual")]
        Manual,

        [Description("Contactless")]
        Contactless,

        [Description("Contactless EMV")]
        ContactlessEMV,

        [Description("ACH Keyed")]
        ACHKeyed,

        [Description("MICR Reader")]
        MICRReader
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardSource
    {
        App,
        Card
    }
}
