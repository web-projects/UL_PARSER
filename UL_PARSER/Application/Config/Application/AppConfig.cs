using System;
using System.Collections.Generic;
using UL_PARSER.Config.Application;

namespace UL_PARSER.Application.Config
{
    [Serializable]
    public class AppConfig
    {
        public string ULTestCase { get; set; }
        public LoggerManager LoggerManager { get; set; }
        public List<ULCommandResponseGroup> ULCommandResponseGroup { get; internal set; } = new List<ULCommandResponseGroup>();
    }
}
