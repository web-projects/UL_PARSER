using System;
using System.Collections.Generic;

namespace UL_PARSER.Common.XO.Common.DAL
{
    //Core card workflow controls (note: only values that affect device workflow are included)
    public class LinkCardWorkflowControls : LinkFutureCompatibility
    {
        #region PreventForceDebitForceCredit From showing in Response to POS
        public static bool IncludeForceDebitCredit { get; set; } = true;  //Must be true initially

        public bool ShouldSerializeForceDebit()
        {
            return IncludeForceDebitCredit;
        }
        public bool ShouldSerializeForceCredit()
        {
            return IncludeForceDebitCredit;
        }
        #endregion PreventForceDebitForceCredit From showing in Response to POS

        public Responses.LinkErrorValue Validate(bool synchronousRequest = true)
        {
            // No validation on these values
            return null;
        }

        public int? CardCaptureTimeout { get; set; }
        public int? ManualCardTimeout { get; set; }
        public int? SignatureCaptureTimeout { get; set; }
        public bool? DebitEnabled { get; set; }
        public bool? EMVEnabled { get; set; }
        public bool? ContactlessEnabled { get; set; }
        public bool? ContactlessEMVEnabled { get; set; }
        public bool? CVVEnabled { get; set; }
        public bool? VerifyAmountEnabled { get; set; }
        public bool? CardExpEnabled { get; set; }
        public bool? AVSEnabled { get; set; }
        public int? PinMaximumLength { get; set; }
        public bool? PinRetryEnabled { get; set; }

        [Obsolete]
        public List<string> AVSType { get; set; }
        public bool? SignatureEnabled { get; set; }
        public bool? AllowCardInReader { get; set; }
        public bool? ForceDebit { get; set; }
        public bool? ForceCredit { get; set; }
        public bool? MSRFallback { get; set; }
        public string FallbackType { get; set; }
        //public List<LinkThresholdAmount> SignatureThresholds { get; set; }
    }
}
