using BisCore;
using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace MQTTLogger
{
    public class MQTTLogger : IDisposable
    {
        public string Host { get; private set; }
        public int Port { get; private set; } = 1883; // MQTT default port
        MqttClient client = null;
        Logger logger = Logger.GetInstance();

        public MQTTLogger(string host)
        {
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
            client = new MqttClient(Host, Port, false, null, null, MqttSslProtocols.None);
            client.MqttMsgPublishReceived += (sender, e) =>
            {
                string topic = e.Topic;
                string message = Encoding.UTF8.GetString(e.Message);
                logger.Output($"{topic}\t{message}");
            };

            string clientId = Guid.NewGuid().ToString();
            client.Subscribe(new string[] { "#" }, new byte[] { 0 });
            client.Connect(clientId);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Disconnect();
                    logger.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
