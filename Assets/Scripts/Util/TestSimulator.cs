/*---------------------------------------------------------------
                TEST-SIMULATOR(Debug/Development Only)
 ---------------------------------------------------------------*/
#define REQUIRE_TEST_DATA //Uncomment to generate test data

namespace Assets.Scripts.Util
{

    using SimpleJSON;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    using Assets.Scripts.Entities.Players;
    using Assets.Scripts.RPA_Game;

    /// <summary>
    /// Testing/debug class for initialising test data.
    /// </summary>
    public class TestSimulator
    {

        /// <summary>
        /// Control whether or not test data is loaded during scene initialiation.
        /// </summary>
        public static bool isDeveloping
        {
            get
            {
#if REQUIRE_TEST_DATA
                return true;
#else
                return false;
#endif
            }
        }

        private static TestSimulator _instance;
        public static TestSimulator INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestSimulator();
                }

                return _instance;
            }
        }
        private static bool hasTestData = false;

        private TestSimulator() { }

        /// <summary>
        /// Primary access method which executes the test procedures of the current state. <see cref="StateManager"/>.
        /// </summary>
        /// <param name="gameState"></param>
        public static void initTestEnvironment(GameState gameState)
        {
            INSTANCE.runTest(gameState);
        }

        //Test Data
        private Texture2D[] classIcons;

        /// <summary>
        /// Adds specific test data to allow testing of specific game states.
        /// </summary>
        /// <param name="gameState"></param>
        private void runTest(GameState gameState)
        {
            if (!hasTestData)
            {
                StateManager.currentState = gameState;

                switch (gameState)
                {
                    case GameState.MAIN_MENU:
                        break;

                    case GameState.CHARACTER_CREATION:
                        break;

                    case GameState.BATTLE_STATE:
                        initClassData();
                        createTestPlayers();
                        break;

                    default:
                        break;
                }

                hasTestData = true;
            }
        }

        /// <summary>
        ///  Creates a list of players for testing/debuggin purposes.
        /// </summary>
        private void createTestPlayers()
        {
            //Multiple game states can run tests, we only need to make player party once
            if (Game.players != null)
            {
                return;
            }

            Game.isSinglePlayer = true;

            Game.players = new List<Player>();

            for (int i = 0; i < Game.PARTY_LIMIT; i++)
            {
                Game.players.Add(new Player());
            }


            //@Test Data
            Game.addPlayer("Alexjp", PlayerClasses.CLERIC);
            /*
             Game.addPlayer("Frictionburn", PlayerClasses.ROGUE);
             Game.addPlayer("Kozza", PlayerClasses.WARRIOR);
             Game.addPlayer("Wizzledonker", PlayerClasses.CLERIC);
            */
            Game.partyLeaderId = Game.players[0].id;

            // Resize Players list to remove unitialised player objets
            int connectedPlayers = Game.connectedPlayers;

            if (connectedPlayers != Game.PARTY_LIMIT)
            {
                Game.players.RemoveRange(connectedPlayers, Game.PARTY_LIMIT - connectedPlayers);
            }

            foreach (Player player in Game.players)
            {
                player.applyClass();
            }

            Game.players[0].isClientPlayer = true;
        }

        /// <summary>
        /// Parses json file for class data and populates icon textures
        /// </summary>
        private void initClassData()
        {
            const string CLASS_DATA_FILE = "assets/resources/data/meta_data.json";
            string classJSON = File.ReadAllText(CLASS_DATA_FILE);

            JSONNode json = JSON.Parse(classJSON);
            JSONArray classes = json["classes"].AsArray;

            int classCount = classes.Count;
            classIcons = new Texture2D[classCount];

            for (int i = 0; i < classCount; i++)
            {
                classIcons[i] = Resources.Load(classes[i]["icon_path"].Value) as Texture2D;
                Adventurer.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_data"].Value);
            }
        }
    }
}
