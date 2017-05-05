using System;
using System.Diagnostics;

namespace Twister.Utilities
{
    public static class ExceptionHandler
    {
        public static void WriteToEventLog(Exception ex)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Warning);
            }
        }
    }
}