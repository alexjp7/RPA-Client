using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util.Logging
{
    public static class LoggingConfiguration
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoggingConfiguration));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void configure()
        {
            XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/log4net.xml"));
        }
    }
}
