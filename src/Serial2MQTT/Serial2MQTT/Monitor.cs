//[*
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Net.Http;
using uPLibrary.Networking.M2Mqtt;
using Codeplex.Data;

namespace Serial2MQTT
{
//*]
    public class Monitor
    {
        SerialPort serialPort;
        public string PortName { get; private set; }
        public string Host { get; private set; }

        MqttClient client = null;

        string clientId = "";

        public Monitor(string portName, string host)
        {
            PortName = portName;
            Host = host;

            Initialize();
        }

        private void Initialize()
        {
            client = new MqttClient(Host);
            client.MqttMsgPublishReceived += (sender, e) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(e.Message));
            };
            clientId = Guid.NewGuid().ToString();
            Open();
        }


        void Open()
        {
            // Set default value
            serialPort = new SerialPort();
            serialPort.PortName = PortName;
            serialPort.DataReceived += SerialPort_DataReceived;

            Setup(serialPort);

            serialPort.Open();
            serialPort.ReadExisting();
            client.Connect(clientId);
            Console.WriteLine($"Connected to {Host}");
        }

        void Publish(string path, object json)
        {
            if (!client.IsConnected)
            {
                Console.WriteLine("Reconnected.");
                client.Connect(clientId);
            }
            var jsonString = DynamicJson.Serialize(json);
            Console.WriteLine($"{Host}/{path} <= {jsonString}");
            client.Publish(path, Encoding.UTF8.GetBytes(jsonString));

        }
        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
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
