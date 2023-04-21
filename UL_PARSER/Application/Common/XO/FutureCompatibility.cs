using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UL_PARSER.Common.XO
{
    public partial class LinkFutureCompatibility
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> Properties { get; set; }
    }
}
