namespace Assets.Scripts.Util.Logging
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using log4net;
    using log4net.Appender;
    using log4net.Core;

    using Assets.Scripts.Util;

    /// <summary>
    /// Log appender for writing to external log files.
    /// </summary>
    public class UnityDataPathAppender : AppenderSkeleton
    {
        /// <summary>
        /// Property to pipe into thread bound map (<see cref="MDC"/>).
        /// Each log entry is prefixed with the game state ID.
        /// </summary>
        private const string STATE_ID = "STATE_ID";

        /// <summary>
        /// Regex used to match any unwanted markdown syntax which is used in unity console output.
        /// </summary>
        /// <remarks>
        /// Markdown syntax does not provide rich text in log files.
        /// </remarks>
        private const string MARKDOWN_REGEX = "<.*?>";

        //Log files
        private const string LOG_FILE = "Logs/rpa.log";
        private const string ASSET_LOAD = "Logs/assetload.log";

        /// <summary>
        /// Provides the implementation for logging to external output file.
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            //Add state ID to any 
            MDC.Set(STATE_ID, StateManager.currentState.ToString());
            //Remove rich text for file logging
            string logMessage = Regex.Replace(RenderLoggingEvent(loggingEvent), MARKDOWN_REGEX, String.Empty);

            File.AppendAllText(getLogFile(loggingEvent), logMessage);
            MDC.Clear();
        }

        /// <summary>
        /// Checks to see if any specifc log files are used for a given logger name. Which is provided by <see cref="LoggingEvent.LoggerName"/>
        /// </summary>
        /// <remarks>
        /// Key areas are logged into their own files.
        /// </remarks>
        /// <param name="loggingEvent">Incoming logging event that is generated each time a log entry is made</param>
        private string getLogFile(LoggingEvent loggingEvent)
        {
            string loggerName = loggingEvent.LoggerName;
            string logFile = LOG_FILE;

            if (loggerName == typeof(AssetLoader).FullName)
            {
                logFile = ASSET_LOAD;
            }

            return logFile;
        }
    }
}