/*---------------------------------------------------------------
                TEST-SIMULATOR(Debug/Development Only)
 ---------------------------------------------------------------*/
#define REQUIRE_TEST_DATA //Uncomment to generate test data

using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public class TestSimulator
    {
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


        private TestSimulator() { }

        public static void initTestEnvironment(GameState gameState)
        {
            INSTANCE.runTest(gameState);
        }

        //################################################################
        /*---------------------------------------------------------------
                                TEST-DATA/PROCEDURES
         ---------------------------------------------------------------*/
        //Test Data
        private Texture2D[] classIcons;

        private void runTest(GameState gameState)
        {
            StateManager.currentState = gameState;
            createTestPlayers();

            switch (gameState)
            {
                case GameState.MAIN_MENU:

                    break;

                case GameState.CHARACTER_CREATION:
                    break;

                case GameState.BATTLE_STATE:
                    initClassData();
                    break;

                default:
                    break;
            }
        }

        /***************************************************************
        *  Creates a list of players for testing/debuggin purposes.
        **************************************************************/
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

        /***************************************************************
        *  Parses json file for class data and populates icon textures
        **************************************************************/
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
