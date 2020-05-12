using System;

namespace Serial2MQTT
{
    class Program
    {
        static void Main(string[] args)
        {
            ScriptLoader sl = new ScriptLoader();
            if (args.Length == 3)
            {
                sl.RunScript(args[0], args[1], args[2]);
            }
            else
            {
                Console.WriteLine("usage: Serial2MQTT [COM_PORT_NAME] [MQTT_SERVER_ADDRESS] [SCRIPT(*.sm)]");
            }
            Console.ReadKey();
        }
    }
}
