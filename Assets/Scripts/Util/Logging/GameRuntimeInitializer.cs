using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util.Logging
{
    /// <summary>
    /// Used for capturing and logging any relevant information during game startup.
    /// </summary>
    class GameRuntimeInitializer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameRuntimeInitializer));

        /// <summary>
        /// Logs initial startup information AFTER the main menu scene is initialized
        /// This is done AFTER to ensure log configurator is initialized exists.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void init()
        {
            log.Info("************************ NEW GAME BEGIN ************************");
            log.Info(TestSimulator.isDeveloping ? "TEST DATA <b><color=green>ENABLED</color></b>" : "TEST DATA <b><color=red>DISABLED</color></b>");

        }
    }
}
