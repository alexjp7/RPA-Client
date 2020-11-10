/*---------------------------------------------------------------
                           CLIENT
 ---------------------------------------------------------------*/
/***************************************************************
* The NetworkClient class maintains a singleton instance of functions
  which allow for a single instanced TCP connection to a remote 
  server.

* The NetworkClient class also includes functionality for sending and
  recieving data to/from the server, aswell as a boolean
  function for returning whether there is data available
  to be read.
**************************************************************/

using System;
using System.IO;
using System.Net.Sockets;

namespace Assets.Scripts.RPA_Game
{
    public sealed class NetworkClient 
    {
        // Singleton instance
        public static NetworkClient INSTANCE;
        // Leading character in a message sent from server -> client to tell the game client that the server is alive
        public static readonly char SERVER_ALIVE_TOKEN = 'a';

        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;

        // Server IP 
        //private const string IP = "142.93.58.123";
        private const string IP = "localhost";
        private const int PORT = 10001;

        private NetworkClient() { }

        public static NetworkClient getInstance()
        {
            if(INSTANCE == null)
            {
                INSTANCE = new NetworkClient();
            }
            return INSTANCE;
        }

        /***************************************************************
        * Attempts to connect to the remote address and instanciates
          a streamreader object used for reading packets sent from the
          server.
        **************************************************************/
        public  void connect()  
        {
            try
            {
                client = new TcpClient();
                client.Connect(IP, PORT);
                stream = client.GetStream();
                reader = new StreamReader(stream);
            }
            catch(SocketException) {}
        }
        /***************************************************************
        @return - true: The connection is open.
                - false: The connection is closed.
        **************************************************************/
        public bool isConnected()
        {
            return client.Connected;
        }

        /***************************************************************
        * Closes the connection and stream reader, terminating
          connection to the remote address.
        **************************************************************/
        public void dissconnect()
        {
            if (stream == null) return;
            stream.Close();
            client.Close();
        }

        /***************************************************************
        @return - the string of a JSON serialized object when trasnfering
        game data OR the character 'a' as an "isAlive" check.
        **************************************************************/
        public string read()
        {
            return reader.ReadLine();
        }

        /***************************************************************
        * formats the serialized string data into a byte array and 
          sends it to the server.
        
        * The message is suffixed with a newline '\n' character, in order
          to distinguish the end of a message when read by the server.

        @param - message: the JSON serialized data created from an 
        appropraite Message object.
        **************************************************************/
        public void send(string message)
        {
            try
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\n");
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch (NullReferenceException)
            {

            }
        }

        /***************************************************************
        @return - true: The Streamreader has data available to read.
                - false: There is no new data to read.
        **************************************************************/
        public bool ready()
        {
            return stream == null ? false: stream.DataAvailable;
        }
    }
}
