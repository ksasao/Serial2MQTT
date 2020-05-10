//[*
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Net.Http;
using uPLibrary.Networking.M2Mqtt;
using Codeplex.Data;
using System.Net;
using System.Timers;
using System.Threading;

namespace Serial2MQTT
{
//*]
    public class Template
    {
        SerialPort serialPort;
        public string PortName { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; } = 1883; // MQTT default port

        /// <summary>
        /// Auto restart duration (sec)
        /// </summary>
        public int NoSignalRestart { get; set; } = 3 * 60;
        
        MqttClient client = null;

        // for freeze check
        System.Timers.Timer watchDogTimer = new System.Timers.Timer(500);

        DateTime lastUpdate;

        string clientId = "";

        public Template(string portName, string host)
        {
            PortName = portName;
            int pos = host.LastIndexOf(":");
            if (pos > 0)
            {
                Host = host.Substring(0, pos);
                Port = Convert.ToInt32(host.Substring(pos + 1));
            }
            else
            {
                Host = host;
            }
            Console.WriteLine($"Connecting to {Host}:{Port}");

            Initialize();
        }

        private void Initialize()
        {
            watchDogTimer.Elapsed += WatchDogTimer_Elapsed;
            watchDogTimer.Start();
            Reset();
        }

        private void Reset()
        {
            bool hasError = true;

            while (hasError)
            {
                lastUpdate = DateTime.Now;

                // MQTT Server reconnect
                try
                {
                    if (client != null && client.IsConnected)
                    {
                        client.Disconnect();
                    }
                    client = null;
                    client = new MqttClient(Host, Port, false, null, null, MqttSslProtocols.None);
                    client.MqttMsgPublishReceived += (sender, e) =>
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(e.Message));
                    };
                    client.ConnectionClosed += (sender, e) =>
                    {
                        Console.WriteLine("MQTT Connection Closed.");
                        Reset();
                    };
                    clientId = Guid.NewGuid().ToString();
                    client.Connect(clientId);
                    Console.WriteLine("MQTT Connected.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MQTT Connection failed. : {ex.Message}");
                    Reset();
                }

                // Serial port reset
                try
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                        serialPort.Dispose();
                        serialPort = null;
                    }
                    serialPort = new SerialPort();
                    serialPort.PortName = PortName;
                    serialPort.DataReceived += SerialPort_DataReceived;

                    Setup(serialPort);

                    serialPort.Open();
                    serialPort.ReadExisting();
                    Console.WriteLine($"{serialPort.PortName} Connected.");
                    hasError = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{serialPort.PortName} Connection failed. : {ex.Message}");
                    Retry();
                }
            }
        }
        private void Retry()
        {
            Console.WriteLine("Waiting 60 seconds.");
            Thread.Sleep(60 * 1000);
        }

        private void WatchDogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            if((now-lastUpdate).TotalSeconds > NoSignalRestart)
            {
                lastUpdate = now;
                Console.WriteLine("Restarting...");
                Reset();
            }
        }


        void Publish(string path, object json)
        {
            var jsonString = DynamicJson.Serialize(json);
            Console.WriteLine($"{Host}/{path} <= {jsonString}");
            try
            {
                client.Publish(path, Encoding.UTF8.GetBytes(jsonString));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Network error. : {ex.Message}");
                Reset();
            }

        }
        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lastUpdate = DateTime.Now;
            string data = serialPort.ReadLine();
            OnSerialReceived(data);
        }
//[SCRIPT]
//[*
        void Setup(SerialPort serial)
        {
            // Sample
            serial.BaudRate = 9600;
            serial.StopBits = StopBits.One;
            serial.NewLine = "\n";
            serial.DtrEnable = true;
            serial.Parity = Parity.None;
        }
        void OnSerialReceived(string data)
        {
            // Sample
            Console.WriteLine($"{PortName} => {data}");
            var json = new
            {
                Data = "World"
            };
            Publish("Hello", json);
        }
//*]
    }
//[*
}
//*]
