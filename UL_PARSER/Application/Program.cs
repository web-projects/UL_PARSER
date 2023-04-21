using Common.LoggerManager;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UL_PARSER.Application.Config;
using UL_PARSER.Config.Application;
using UL_PARSER.Devices.Common;
using UL_PARSER.DTE;
using static UL_PARSER.Devices.Common.Types;

namespace UL_PARSER
{
    class Program
    {
        static AppConfig configuration;
        static DeviceLogHandler deviceLogHandler = DeviceLogger;
        static private List<CommandResponse> commandResponseList;

        static void Main(string[] args)
        {
            SetupEnvironment();

            ConfigurationLoad();

            Console.WriteLine($"COMMANDS TO PROCESS = {commandResponseList.Count()}");

            foreach (CommandResponse commandResponse in commandResponseList)
            {
                Logger.info($"COMMAND : {commandResponse.Command}");
                Logger.info($"RESPONSE: {commandResponse.Response}");
            }
        }

        static private void SetupEnvironment()
        {
            try
            {
                // Get appsettings.json config - AddEnvironmentVariables() requires package: Microsoft.Extensions.Configuration.EnvironmentVariables
                configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build()
                    .Get<AppConfig>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application Exception: [{ex}].");
            }

            // logger manager
            SetLogging();

            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");
        }

        static void SetLogging()
        {
            try
            {
                //string[] logLevels = GetLoggingLevels(0);
                string[] logLevels = configuration.LoggerManager.Logging.Levels.Split("|");

                if (logLevels.Length > 0)
                {
                    string fullName = Assembly.GetEntryAssembly().Location;
                    string logname = Path.GetFileNameWithoutExtension(fullName) + ".log";
                    string path = Directory.GetCurrentDirectory();
                    string filepath = path + "\\logs\\" + logname;

                    int levels = 0;
                    foreach (string item in logLevels)
                    {
                        foreach (LOGLEVELS level in LogLevels.LogLevelsDictionary.Where(x => x.Value.Equals(item)).Select(x => x.Key))
                        {
                            levels += (int)level;
                        }
                    }

                    Logger.SetFileLoggerConfiguration(filepath, levels);

                    Logger.info($"{Assembly.GetEntryAssembly().GetName().Name} ({Assembly.GetEntryAssembly().GetName().Version}) - LOGGING INITIALIZED.");
                }
            }
            catch (Exception e)
            {
                Logger.error("main: SetupLogging() - exception={0}", e.Message);
            }
        }

        static void ConfigurationLoad()
        {
            // Get appsettings.json config.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // UL DATA GROUP
            ULDataGroup();
        }

        static void ULDataGroup()
        {
            // Capture all data in proper object
            commandResponseList = new List<CommandResponse>();

            // Retrieve All Command/Response pairs in the list
            foreach (ULCommandResponseGroup ulCommand in configuration.ULCommandResponseGroup)
            {
                XmlSerializer xs = new XmlSerializer(typeof(CommandResponse));
                commandResponseList.Add((CommandResponse)xs.Deserialize(new StringReader(ulCommand.CommandResponse)));
            }
        }

        public static void DeviceLogger(LogLevel logLevel, string message)
        {
            Console.WriteLine($"[{logLevel}]: {message}");
        }
    }
}
