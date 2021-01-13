namespace Assets.Scripts.Util.Logging
{
    using System;
    using System.IO;
    using log4net.Config;
    using UnityEngine;
    public static class LoggingConfiguration
    {
        private static readonly String LOG_CONFIGURATION_FILE = "log4net.xml";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void configure()
        {
            XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/{LOG_CONFIGURATION_FILE}"));
        }
    }
}
