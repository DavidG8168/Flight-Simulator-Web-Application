using System;
using System.IO;

namespace FlightGearWebApp.Models {
    // Holds all the information needed for the server connection.
    public class Information {
        // The class members.
        // The class instance.
        private static Information instance = null;
        // The writer we will use to write to the file.
        private StreamWriter writer;
        // The class properties.
        // The time interval.
        public int Time { get; set; }
        // The time it takes to finish.
        public int Timeout { get; set; }
        // The path of the .csv file.
        public string FilePath { get; set; }
        // The server connection.
        public ServerConnection serverConnection { get; private set; }
        // The constructor.
        private Information() {
            serverConnection = new ServerConnection();
        }
        public static Information Instance {
            get {
                // If no instance was created then create a new one.
                if (instance == null) {
                    instance = new Information();
                }
                // Return the instance.
                return instance;
            }
        }
        // Start connecting to the server.
        public void Start() {
            serverConnection.Connect();
        }
        // Create a file.
        public void CreateFile(string filePath) {
            writer = new StreamWriter(filePath);
        }
        // Write to a file.
        public void WriteToFile() {
            // Writing format.
            string toWrite = serverConnection.Lon.ToString() + "," + serverConnection.Lat.ToString();
            try {
                writer.WriteLineAsync(toWrite);
            }
            catch (Exception) {
                Console.WriteLine("Problem writing to file.");
            }
        }
        // Close a file after writing.
        public void CloseFile() {
            try {
                writer.Close();
            }
            catch (Exception) {
                Console.WriteLine("Problem closing the file.");
            }
        }
    }
}