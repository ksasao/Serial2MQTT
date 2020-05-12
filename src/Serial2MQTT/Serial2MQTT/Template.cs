//[*
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using Codeplex.Data;
using System.Timers;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

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

        public bool SerialPortEnabled { get; private set; } = false;
        public string[] Topics { get; private set; }
        

        MqttFactory factory = new MqttFactory();
        IMqttClient client;

        // for freeze check
        System.Timers.Timer watchDogTimer = new System.Timers.Timer(500);

        DateTime lastUpdate;

        string clientId = "";

        public Template(string portName, string host)
        {
            PortName = portName;
            SerialPortEnabled = (portName.Trim() != "");


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


                // Serial port reset
                if (SerialPortEnabled)
                {
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
                else
                {
                    Setup(new SerialPort()); // dummy
                    hasError = false;
                }

                // MQTT client setup
                try
                {
                    if (client != null)
                    {
                        client.Dispose();
                    }
                    client = null;
                    client = factory.CreateMqttClient();

                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(Host, Port)
                        .Build();

                    // Set topic filters
                    client.UseConnectedHandler(async e =>
                    {
                        if (Topics != null && Topics.Length > 0)
                        {
                            var filters = new TopicFilter[Topics.Length];
                            for(int i=0; i<filters.Length; i++)
                            {
                                filters[i] = new TopicFilterBuilder().WithTopic(Topics[i]).Build();
                            }
                            await client.SubscribeAsync(filters);
                        }
                    });
                    // Message receive event
                    client.UseApplicationMessageReceivedHandler(e => {
                        string topic = e.ApplicationMessage.Topic;
                        string data = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        OnTopicReceived(topic, data);
                    });
                    // Disconnect Event
                    client.UseDisconnectedHandler(async e =>
                    {
                        Console.WriteLine("MQTT Connection Closed.");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        Reset();
                    });
                    clientId = Guid.NewGuid().ToString();
                    Task.Run(async () => {
                        await client.ConnectAsync(options);
                        Console.WriteLine("MQTT Connected.");
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MQTT Connection failed. : {ex.Message}");
                    Reset();
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
            if (SerialPortEnabled)
            {
                DateTime now = DateTime.Now;
                if ((now - lastUpdate).TotalSeconds > NoSignalRestart)
                {
                    lastUpdate = now;
                    Console.WriteLine("No signal from serial port ({NoSignalRestart} sec.). Restarting...");
                    Reset();
                }
            }
        }
        void Subscribe(string[] topics)
        {
            Topics = topics;
        }

        void Publish(string topic, object json)
        {
            var jsonString = DynamicJson.Serialize(json);
            Console.WriteLine($"{Host}/{topic} <= {jsonString}");
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(Encoding.UTF8.GetBytes(jsonString))
                    .WithRetainFlag()
                    .Build();

                Task.Run(async () =>
                {
                    await client.PublishAsync(message, CancellationToken.None);
                });

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

            // Subscribe Topics
            string[] topics = new string[]{ "#" };
            Subscribe(topics);
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
        void OnTopicReceived(string topic, string data)
        {
            Console.WriteLine($"{topic}: {data}");
        }
//*]
    }
//[*
}
//*]
