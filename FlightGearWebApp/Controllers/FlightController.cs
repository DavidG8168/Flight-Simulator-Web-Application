using System;
using System.Web.Mvc;
using FlightGearWebApp.Models;
using System.Xml;
using System.Text;
using System.IO;

namespace FlightGearWebApp.Controllers {
    // The controller class implements the actions for the routes.
    public class FlightController : Controller {
        // GET: Map Index
        // Display the default page, will show the map and nothing else.
        public ActionResult Index() {
            return View();
        }
        // GET: Map display
        // Set the server parameters and start the connection to the server to read data
        // and display the current location of the plane.
        public ActionResult display(string serverIP, int serverPort) {
            // Time interval for the connection.
            const int timer = 1000000;
            // Differentiate reading from the server or file based on the IP being the file name.
            if (serverIP == "flight1") {
                Session["fromFile"] = 1;
                Session["time"] = 4;
                return View();
            }
            // Set the properties.
            Information.Instance.serverConnection.IP = serverIP;
            Information.Instance.serverConnection.Port = serverPort;
            Information.Instance.Time = timer;
            // Start the connection.
            Information.Instance.Start();
            // Pass value to the CSHTML.
            Session["fromFile"] = 0;
            Session["time"] = timer;
            return View();
        }
        // GET: Map displayRoute
        // Set the server parameters and start the connection to the server to read data
        // and display the route the plane takes according to the data points read.
        public ActionResult displayRoute(string serverIP, int serverPort, int timer) {
            Information.Instance.serverConnection.IP = serverIP;
            Information.Instance.serverConnection.Port = serverPort;
            Information.Instance.Time = timer;
            Information.Instance.Start();
            Session["time"] = timer;
            return View();
        }
        // GET: Map save
        // Save the parameters recieved from the server into a .csv file.
        public ActionResult save(string serverIP, int serverPort, int timer, int stopTimer, string path) {
            Information.Instance.serverConnection.IP = serverIP;
            Information.Instance.serverConnection.Port = serverPort;
            Information.Instance.Time = timer;
            Information.Instance.Timeout = stopTimer;
            Information.Instance.FilePath = AppDomain.CurrentDomain.BaseDirectory + path + ".txt";
            Information.Instance.Start();
            Session["time"] = timer;
            Session["timeout"] = stopTimer;
            return View();
        }
        // ** File Operations **
        [HttpPost]
        // Create a new file.
        public string CreateFile() {
            string name = Information.Instance.FilePath;
            Information.Instance.CreateFile(name);
            return name;
        }
        [HttpPost]
        // Write to a file.
        public string WriteFile() {
            string name = Information.Instance.FilePath;
            Information.Instance.WriteToFile();
            return name;
        }
        [HttpPost]
        // Close a file.
        public string CloseFile() {
            string name = Information.Instance.FilePath;
            Information.Instance.CloseFile();
            return name;
        }
        [HttpPost]
        // Read contents of a file.
        public string[] ReadFromFile() {
            string path = AppDomain.CurrentDomain.BaseDirectory + "flight1" + ".txt";
            StreamReader stream = new StreamReader(path, Encoding.UTF8);
            // Read and parse contents of the file.
            string contents = stream.ReadToEnd();
            string[] separatingStrings = { "\r\n", ","};
            string[] words = contents.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
            // Give the points to the ServerConnection to be used in the read function.
            Information.Instance.serverConnection.dataPoints = words;
            // Set the points to be taken from the file after parsing.
            Information.Instance.serverConnection.getFromFile = true;
            // Reset the indexer.
            Information.Instance.serverConnection.indexer = 0;
            stream.Close();
            return words;
        }
        // ** Server Operations **
        [HttpPost]
        // Get and Latitude and Longitude from the server.
        public string ReadFromServer() {
            var connection = Information.Instance.serverConnection;
            connection.read();
            return ToXml(connection);
        }
        [HttpPost]
        // Get randomly generated values for the route animation testing.
        public string GetRandomPoints() {
            var connection = Information.Instance.serverConnection;
            connection.readRandom();
            return ToXml(connection);
        }
        // Convert to XML.
        private string ToXml(ServerConnection network) {
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = XmlWriter.Create(builder, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("serverConnections");
            network.ToXml(writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            return builder.ToString();
        }
    }
}