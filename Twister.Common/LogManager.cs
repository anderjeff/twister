using System;
using System.IO;

namespace Twister.Common
{
    public class LogManager : ILogManager
    {
        static LogManager()
        {
	        var configPath = @"G:\Engineering\Programs\Twister 2015\log4net.config";
	        if (File.Exists(configPath))
	        {
		        log4net.Config.XmlConfigurator.Configure(new FileInfo(configPath));
	        }
	        else
	        {
		        log4net.Config.XmlConfigurator.Configure();
	        }
        }

        public ILogger GetLogger(Type type)
        {
            var logger = log4net.LogManager.GetLogger(type);
            return new LoggerAdapter(logger);
        }
    }
}