/*---------------------------------------------------------------
                           CLIENT
 ---------------------------------------------------------------*/
/***************************************************************
* The Client class maintains a singleton instance of functions
  which allow for a single instanced TCP connection to a remote 
  server.

* The Client class also includes functionality for sending and
  recieving data to/from the server, aswell as a boolean
  function for returning whether there is data available
  to be read.
**************************************************************/

using System;
using System.IO;
using System.Net.Sockets;

namespace Assets.Scripts.RPA_Game
{
    public sealed class Client 
    {
        //Singleton instance
        public static Client INSTANCE;

        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;

        //Server IP 
        //private const string IP = "142.93.58.123";
        private const string IP = "localhost";
        private const int PORT = 10001;

        private Client() { }

        public static Client getInstance()
        {
            return INSTANCE == null ? new Client() : INSTANCE;
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
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message+"\n");
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
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
