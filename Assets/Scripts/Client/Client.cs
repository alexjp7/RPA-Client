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

        //private const string IP = "142.93.58.123";
        private const string IP = "localhost";
        private const int PORT = 10001;

        public static void connect()
        {
            client = new TcpClient();
            client.Connect(IP, PORT);
            stream = client.GetStream();
            reader = new StreamReader(stream);

        }

        public static void dissconnect()
        {
            client.GetStream().Close();
        }

        public static string read()
        {
            //Byte[] data = new Byte[256];
            //String responseData = String.Empty;
            //Int32 bytes = stream.Read(data, 0, data.Length);
            //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            return reader.ReadLine();

        }

        public static void send(string message)
        {

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message+"\n");
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

     

        public static bool ready()
        {
            return stream.DataAvailable;
        }

    }
}
