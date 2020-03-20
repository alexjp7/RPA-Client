/*---------------------------------------------------------------
                           CLIENT
 ---------------------------------------------------------------*/
/***************************************************************
* The Client class provides static functions which allow for a
  single instanced TCP connection to a remote server.
* The Client class also includes functionality for sending and
  recieving data to/from the server, aswell as a boolean
  function for returning whether there is data available
  to be read.
**************************************************************/
using System;
using System.IO;
using System.Net.Sockets;

namespace Assets.Scripts.RPA_Client
{
    class Client 
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;

        //Server IP 
        //private const string IP = "142.93.58.123";
        private const string IP = "localhost";
        private const int PORT = 10001;

        /***************************************************************
        * Attempts to connect to the remote address and instanciates
          a streamreader object used for reading packets sent from the
          server.
        **************************************************************/
        public static void connect()  
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
        * Returns the connection status, whether it the client is
          connected or not to the remote address.
        **************************************************************/
        public static bool isConnected()
        {
            return client.Connected;
        }
        /***************************************************************
        * Closes the connection and stream reader, terminating
          connection to the remote address.
        **************************************************************/
        public static void dissconnect()
        {
            stream.Close();
            client.Close();
        }

        /***************************************************************
        * returns the string of a JSON serialized object when trasnfering
          game data OR the character 'a' as an "isAlive" check.
        **************************************************************/
        public static string read()
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
        public static void send(string message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message+"\n");
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        /***************************************************************
        * flags that there is new data available from the server.
        **************************************************************/
        public static bool ready()
        {
            return stream == null ? false: stream.DataAvailable;
        }

    }
}
