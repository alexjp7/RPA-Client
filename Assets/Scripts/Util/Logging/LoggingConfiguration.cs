

namespace Assets.Scripts.Util.Logging
{
    using log4net;
    using log4net.Config;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    public static class LoggingConfiguration
    {
        
        private static readonly ILog log = LogManager.GetLogger(typeof(LoggingConfiguration));
        private static readonly String LOG_CONFIGURATION_FILE = "log4net.xml";


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void configure()
        {
            XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/{LOG_CONFIGURATION_FILE}"));
        }
    }
}
