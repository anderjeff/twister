using System;
using System.IO;

namespace Twister.Common
{
    public class LogManager : ILogManager
    {
        static LogManager()
        {
            //log4net.Config.XmlConfigurator.Configure();
            log4net.Config.XmlConfigurator.Configure(
                new FileInfo(@"G:\Engineering\Programs\Twister 2015\log4net.config"));
        }

        public ILogger GetLogger(Type type)
        {
            var logger = log4net.LogManager.GetLogger(type);
            return new LoggerAdapter(logger);
        }
    }
}