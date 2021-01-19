namespace Assets.Scripts.Util.Logging
{
    using System.IO;
    using log4net.Config;
    using UnityEngine;

    /// <summary>
    /// Configuration class that loads logging properties found in <b>log4net.xml</b>.
    /// </summary>
    public static class LoggingConfiguration
    {
        private static readonly string LOG_CONFIGURATION_FILE = "log4net.xml";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void configure()
        {
            XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/{LOG_CONFIGURATION_FILE}"));
        }
    }
}
