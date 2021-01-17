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
namespace Assets.Scripts.RPA_Game
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    public sealed class Client 
    {
        //Singleton instance
        public static Client INSTANCE;

        public static readonly char SERVER_ALIVE_TOKEN = 'a';

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
            if(INSTANCE == null)
            {
                INSTANCE = new Client();
            }
            return INSTANCE;
        }

        /// <summary>
        /// Attempts to connect to the remote address and instanciates a streamreader object used for reading packets sent from the server
        /// </summary>
        public void connect()  
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

 
        /// <summary>
        /// 
        /// </summary>
        /// <returns> True: The connection is open. False: The connection is closed. </returns>
        public bool isConnected()
        {
            return client.Connected;
        }

        /***************************************************************
        **************************************************************/
        /// <summary>
        ///  Closes the connection and stream reader, terminating  connection to the remote address.
        /// </summary>
        public void dissconnect()
        {
            if (stream == null) return;
            stream.Close();
            client.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> the string of a JSON serialized object when trasnfering game data OR the character 'a' as an "isAlive" check.</returns>
        public string read()
        {
            return reader.ReadLine();
        }

        /// <summary>
        /// Formats the serialized string data into a byte array and sends it to the server.
        /// <para>
        ///  The message is suffixed with a newline '\n' character, in order to distinguish the end of a message when read by the server.
        /// </para>
        /// </summary>
        /// <param name="message"> The JSON serialized data created from an  appropraite Message object.</param>
        public void send(string message)
        {
            try
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\n");
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch (NullReferenceException)
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> True: The Streamreader has data available to read. False: There is no new data to read.</returns>
        public bool ready()
        {
            return stream == null ? false: stream.DataAvailable;
        }
    }
}
