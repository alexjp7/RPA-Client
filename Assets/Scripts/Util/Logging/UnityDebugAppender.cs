namespace Assets.Scripts.Util.Logging
{
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using UnityEngine;

    using Assets.Scripts.Util;

    public class UnityDebugAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            MDC.Set("StateID", StateManager.currentState.ToString());
            var message = RenderLoggingEvent(loggingEvent);

            switch (loggingEvent.Level.Name)
            {
                case "DEBUG":
                case "INFO":
                    Debug.Log(message);
                    break;

                case "WARN":
                    Debug.LogWarning(message);
                    break;

                case "ERROR":
                    Debug.LogError(message);
                    break;
                default:
                    break;
            }
            MDC.Clear();
        }
    }
}
