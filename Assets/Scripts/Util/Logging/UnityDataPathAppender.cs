using System;
using System.IO;
using System.Text.RegularExpressions;
using Assets.Scripts.Util;
using log4net;
using log4net.Appender;
using log4net.Core;
using UnityEngine;

public class UnityDataPathAppender : AppenderSkeleton
{
    protected override void Append(LoggingEvent loggingEvent)
    {
        //Add state ID to any 
        MDC.Set("StateID", StateManager.currentState.ToString());

        //Remove rich text for file logging
        string logMessage = Regex.Replace(RenderLoggingEvent(loggingEvent), "<.*?>", String.Empty);

        File.AppendAllText("Logs/RPA_Log.log", logMessage);

        MDC.Clear();
    }
}