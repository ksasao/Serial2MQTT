using BisCore;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;

namespace MQTTLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            MQTTLogger logger;
            if (args.Length == 1)
            {
                logger = new MQTTLogger(args[0]);
            }
            else
            {
                Console.WriteLine("usage: MQTTLogger [MQTT_BROKER_ADDRESS]");
                return;
            }

            Console.WriteLine("[ESC] to exit...");
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;
            logger.Dispose();
        }
    }
}
  