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
    private static readonly String STATE_ID = "STATE_ID";
    private static readonly String LOG_FILE = "Logs/RPA_Log.log";
    private static readonly String MARKDOWN_REGEX = "<.*?>";

    protected override void Append(LoggingEvent loggingEvent)
    {
        //Add state ID to any 
        MDC.Set(STATE_ID, StateManager.currentState.ToString());

        //Remove rich text for file logging
        string logMessage = Regex.Replace(RenderLoggingEvent(loggingEvent), MARKDOWN_REGEX, String.Empty);

        File.AppendAllText(LOG_FILE, logMessage);

        MDC.Clear();
    }
}