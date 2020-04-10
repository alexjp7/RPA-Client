/*---------------------------------------------------------------
                TEST-SIMULATOR(Debug/Development Only)
 ---------------------------------------------------------------*/
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

        private static TestSimulator _instance;
        public static TestSimulator INSTANCE
        {
            get
            {
                if(_instance == null)
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
            ViewController.INSTANCE.setStateScript(gameState);
            createTestPlayers();
            initClassData();
        }

        /***************************************************************
        *  Creates a list of players for testing/debuggin purposes.
        **************************************************************/
        private void createTestPlayers()
        {
            Game.players = new List<Player>();
            //@Test Data
            Game.players.Add(new Player(6, "Alexjp", (int)PlayerClasses.WARRIOR, true));
            //Game.players.Add(new Player(8, "Frictionburn", 0, true));
            //Game.players.Add(new Player(11, "Kozza", 2, true));
            //Game.players.Add(new Player(4, "Wizzledonker", 3, true));

            Game.connectedPlayers = Game.players.Count;

            foreach (Player player in Game.players) player.applyClass();
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
                Adventurer.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_path"].Value);
            }
        }
    }
}
