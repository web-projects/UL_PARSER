namespace UL_PARSER.Common.XO.Responses
{
    public class LinkErrorValue : LinkFutureCompatibility
    {
        public LinkErrorValue() { }

        public LinkErrorValue(string type, string code, string description)
        {
            Type = type;
            Code = code;
            Description = description;
        }

        public string Type { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
