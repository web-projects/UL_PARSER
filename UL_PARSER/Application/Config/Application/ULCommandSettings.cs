using System;
using System.Collections.Generic;

namespace UL_PARSER.Config.Application
{
    [Serializable]
    public class ULCommandSettings
    {
        public List<CommandResponse> ULDataGroup { get; internal set; } = new List<CommandResponse>();
    }
}
