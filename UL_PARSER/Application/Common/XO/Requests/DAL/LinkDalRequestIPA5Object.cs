using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UL_PARSER.Common.XO.IPA5Private;
using UL_PARSER.Common.XO.Responses.Payment;

namespace UL_PARSER.Common.XO.Requests.DAL
{
    public partial class LinkDALRequestIPA5Object : ICloneable
    {
        //public DAL_ACHCheckData CapturedMICRData { get; set; }
        public DAL_EMVCardData CapturedEMVCardData { get; set; }
        public LinkCardResponse CapturedCardData { get; set; }
        //public LinkDALActionResponse DALResponseData { get; set; }
        public bool IsEMVCard { get; set; }
        public bool IsEMVReadAttempt { get; set; }
        public string EncryptedTrackKSN { get; set; }
        public string EncryptedTrackIV { get; set; }
        public string EncryptedTrackData { get; set; }
        public string Track1 { get; set; }
        public string Track2 { get; set; }
        public string Zip { get; set; }
        public string OnlinePINKSN { get; set; }
        public string OnlinePINData { get; set; }
        public string TamperStatus { get; set; }
        public string ArsStatus { get; set; }
        public string WhiteListHash { get; set; }
        public int EmvCardCaptureCount { get; set; }
        public int MsrCardCaptureCount { get; set; }
        public int PinRetryCount { get; set; }
        public bool HasAmountVerified { get; set; }
        public int Track1Status { get; set; }
        public int Track2Status { get; set; }
        public int Track3Status { get; set; }
        public string SignatureName { get; set; }
        public List<byte[]> SignatureData { get; set; }
        public byte[] ESignatureImage { get; set; }
        public int MaxBytes { get; set; }

        //public DALCDBData DALCdbData { get; set; }

        /// <summary>
        /// These tags will be kept for general purposes tags
        /// </summary>
        public Dictionary<uint, byte[]> UnmappedTags { get; set; } = new Dictionary<uint, byte[]>();
        public object Clone()
        {
            string serializedObj = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<LinkDALRequestIPA5Object>(serializedObj);
        }

        public string TerminalCountryCode { get; set; }
        public int CardInReaderStatus { get; set; }
        public DAL_VASData CapturedVASData { get; set; }
    }

}
