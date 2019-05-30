using System;
using System.Xml;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace FlightGearWebApp.Models {
    // The server connection class handles all connecting
    // and reading tasks from the server.
    public class ServerConnection {
        // Class members.
        // Random for generating random values for the route.
        private static Random ran = new Random();
        // The thread we will run the server connection on.
        Thread thread;
        // The retry time for connection.
        int timer = 500;
        // The condition boolean that will indicate when a connection should stop.
        public volatile bool stopCondition = true;
        // The TcpClient that will connect to the server.
        private TcpClient tcpClient;
        // The mutex we will use.
        public static Mutex m = new Mutex();
        // Class properties.
        // The IP.
        public string IP { get; set; }
        // The Port.
        public int Port { get; set; }
        // The Latitude.
        public double Lat { get; set; }
        // The Longitude.
        public double Lon { get; set; }
        // List of data from file.
        public string[] dataPoints { get; set; }
        // Should read from file or not.
        public bool getFromFile { get; set; } = false;
        // What Lon and Lat from file points we're at.
        public int indexer { get; set; } = 0;
        // Connect to the server using the tcpClient.
        // Keeps trying until connection is established.
        public void Connect() {
            // Do nothing if there is no need to stop.
            if (!stopCondition) {
                return;
            }
            // Start the connection.
            // Set stopping condition to false.
            stopCondition = false;
            // Run the connection on a new thread.
            this.tcpClient = new TcpClient();
            this.thread = new Thread(() => {
                m.WaitOne();
                // While not connected keep trying to connect.
                while (!tcpClient.Connected) {
                    try {
                        tcpClient.Connect(IP, Port);
                        Thread.Sleep(timer);
                    }
                    // If failed try again.
                    catch (Exception) {
                        Console.WriteLine("Error, Connection failed ! Retrying...");
                    }
                }
                m.ReleaseMutex();
            });
            this.thread.Start();
        }
        // Stop the connection.
        public void Disconnect() {
            // Do nothing if we don't need to stop yet.
            if (stopCondition) {
                return;
            }
            // Close the connection.
            thread.Abort();
            this.tcpClient.Close();
            // Set stopping boolean to true.
            stopCondition = true;
        }
        // Parse the string from the server to get the needed values.
        public string SplitStr(string toBeParsed) {
            string[] result = toBeParsed.Split('=');
            result = result[1].Split('\'');
            result = result[1].Split('\'');
            return result[0];
        }
        // Function gets the Lon and Lat from the server.
        public void read() {
            // If we need to get from the file.
            if (getFromFile) {
                    if (indexer >= dataPoints.Length) {
                    indexer = 0;
                    getFromFile = false;
                }
                else {
                    Lon = (int)double.Parse(dataPoints[indexer]);
                    Lat = (int)double.Parse(dataPoints[indexer + 1]);
                    indexer += 2;
                }
            }
            // If we don't need to get from file and the client isn't null.
            if (!getFromFile && tcpClient != null) {
                // Create an empty string to store the command.
                string sendCommands = "";
                // Use a mutex to read each command one by one.
                m.WaitOne();
                // Longitude from server. 
                // Get stream from the server.
                NetworkStream nsWriter = this.tcpClient.GetStream();
                sendCommands = "get /position/longitude-deg\r\n";
                // Size of bytes to get from the server.
                int numOfBytes = Encoding.ASCII.GetByteCount(sendCommands);
                // Create buffer.
                byte[] bytesToSend = new byte[numOfBytes];
                // Get the message from the server and store in the buffer.
                bytesToSend = Encoding.ASCII.GetBytes(sendCommands);
                // Use a stream to move contents of buffer.
                nsWriter.Write(bytesToSend, 0, bytesToSend.Length);
                StreamReader strmReader = new StreamReader(nsWriter);
                string lon = SplitStr(strmReader.ReadLine());
                Lon = double.Parse(lon);
                // Latitude from server.
                sendCommands = "get /position/latitude-deg\r\n";
                // Size of bytes to get from the server.
                numOfBytes = Encoding.ASCII.GetByteCount(sendCommands);
                // Create buffer.
                bytesToSend = new byte[numOfBytes];
                // Get the message from the server and store in the buffer.
                bytesToSend = Encoding.ASCII.GetBytes(sendCommands);
                // Use a stream to move contents of buffer.
                nsWriter.Write(bytesToSend, 0, bytesToSend.Length);
                strmReader = new StreamReader(nsWriter);
                string lat = SplitStr(strmReader.ReadLine());
                Lat = double.Parse(lat);
                // Normalize the values from the server by taking their absolutes.
                Lat = (int)Math.Abs(Lat);
                Lon = (int)Math.Abs(Lon);
                // Free the mutex.
                m.ReleaseMutex();
            }
        }
        // Generate random numbers for the route display to see the animation in testing.
        public void readRandom() {
            Lat = ran.Next(900);
            Lon = ran.Next(500);
        }
        // Write to XML.
        public void ToXml(XmlWriter writer) {
            writer.WriteStartElement("NetworkConnection");
            writer.WriteElementString("Ip", this.IP);
            writer.WriteElementString("Port", this.Port.ToString());
            writer.WriteElementString("Lat", this.Lat.ToString());
            writer.WriteElementString("Lon", this.Lon.ToString());
            writer.WriteEndElement();
        }
    }
}
